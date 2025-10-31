using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;

namespace IdeaBranch.Infrastructure.Resilience;

/// <summary>
/// Central registry for resilience policies with telemetry support.
/// Provides standard retry (exponential backoff + jitter) and circuit breaker policies.
/// </summary>
public sealed class ResiliencePolicyRegistry
{
    private readonly ILogger<ResiliencePolicyRegistry> _logger;
    private readonly ResilienceTelemetryEmitter _telemetry;

    public ResiliencePolicyRegistry(ILogger<ResiliencePolicyRegistry> logger, ResilienceTelemetryEmitter? telemetry = null)
    {
        _logger = logger;
        _telemetry = telemetry ?? new ResilienceTelemetryEmitter(logger);
    }

    /// <summary>
    /// Standard retry policy with exponential backoff and decorrelated jitter.
    /// Retries transient HTTP errors (408, 429, 5xx except 501, 505).
    /// </summary>
    public IAsyncPolicy<HttpResponseMessage> GetStandardHttpRetryPolicy(string policyName = "StandardHttpRetry")
    {
        return Policy<HttpResponseMessage>
            .HandleResult(response => IsTransientHttpError(response))
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>() // Timeout
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: (retryAttempt, result, context) =>
                {
                    // Exponential backoff with decorrelated jitter
                    // Formula: random_between(0, min(cap, base * 2^attempt))
                    var baseDelay = TimeSpan.FromMilliseconds(100);
                    var maxDelay = TimeSpan.FromSeconds(30);
                    var exponential = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, retryAttempt - 1));
                    var capped = TimeSpan.FromMilliseconds(Math.Min(exponential.TotalMilliseconds, maxDelay.TotalMilliseconds));
                    var jitter = TimeSpan.FromMilliseconds(Random.Shared.NextDouble() * capped.TotalMilliseconds);
                    
                    var delay = jitter;
                    
                    _telemetry.EmitRetryAttempt(
                        policyName,
                        retryAttempt,
                        delay,
                        result?.Exception?.Message ?? result?.Result?.StatusCode.ToString() ?? "Unknown",
                        context);
                    
                    return delay;
                },
                onRetryAsync: (result, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "HTTP request retry {RetryCount} after {Delay}ms. Status: {Status}, Error: {Error}",
                        retryCount,
                        timespan.TotalMilliseconds,
                        result.Result?.StatusCode.ToString() ?? (result.Exception != null ? "Exception" : null),
                        result.Exception?.Message ?? result.Result?.ReasonPhrase);
                    return Task.CompletedTask;
                });
    }

    /// <summary>
    /// Circuit breaker policy that opens after consecutive failures exceed threshold.
    /// </summary>
    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        string policyName = "StandardCircuitBreaker",
        int handledEventsAllowedBeforeBreaking = 5,
        TimeSpan durationOfBreak = default)
    {
        if (durationOfBreak == default)
        {
            durationOfBreak = TimeSpan.FromSeconds(30);
        }

        return Policy<HttpResponseMessage>
            .HandleResult(response => IsTransientHttpError(response))
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking,
                durationOfBreak: durationOfBreak,
                onBreak: (result, duration, context) =>
                {
                    var status = result.Result?.StatusCode.ToString() ?? (result.Exception != null ? "Exception" : null);
                    _telemetry.EmitCircuitBreakerOpened(
                        policyName,
                        duration,
                        status ?? result.Exception?.Message ?? "Unknown",
                        context);
                    
                    _logger.LogWarning(
                        "Circuit breaker opened for {Duration}s. Last failure: {Status}, Error: {Error}",
                        duration.TotalSeconds,
                        status,
                        result.Exception?.Message ?? result.Result?.ReasonPhrase);
                },
                onReset: context =>
                {
                    _telemetry.EmitCircuitBreakerReset(policyName, context);
                    _logger.LogInformation("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    _telemetry.EmitCircuitBreakerHalfOpen(policyName);
                    _logger.LogInformation("Circuit breaker half-open (testing)");
                });
    }

    /// <summary>
    /// Combined policy: retry with exponential backoff + circuit breaker.
    /// </summary>
    public IAsyncPolicy<HttpResponseMessage> GetStandardResiliencePolicy(string policyName = "StandardResilience")
    {
        return Policy.WrapAsync(
            GetCircuitBreakerPolicy($"{policyName}_CircuitBreaker"),
            GetStandardHttpRetryPolicy($"{policyName}_Retry"));
    }

    /// <summary>
    /// Policy for idempotent operations (GET, PUT, DELETE).
    /// Uses standard retry + circuit breaker.
    /// </summary>
    public IAsyncPolicy<HttpResponseMessage> GetIdempotentHttpPolicy(string policyName = "IdempotentHttp")
    {
        return GetStandardResiliencePolicy(policyName);
    }

    /// <summary>
    /// Policy for non-idempotent operations (POST).
    /// By default, NO retries unless explicitly configured with idempotency safeguards.
    /// </summary>
    public IAsyncPolicy<HttpResponseMessage> GetNonIdempotentHttpPolicy(string policyName = "NonIdempotentHttp")
    {
        // Default: no retries for non-idempotent operations
        // Only retry on timeout/connection errors that are clearly safe to retry
        return Policy<HttpResponseMessage>
            .Handle<TaskCanceledException>() // Timeout only
            .Or<HttpRequestException>(ex => ex.InnerException is System.Net.Sockets.SocketException)
            .WaitAndRetryAsync(
                retryCount: 1,
                sleepDurationProvider: (retryAttempt, result, context) =>
                {
                    _telemetry.EmitRetryAttempt(
                        policyName,
                        retryAttempt,
                        TimeSpan.FromMilliseconds(500),
                        result?.Exception?.Message ?? "ConnectionError",
                        context);
                    
                    return TimeSpan.FromMilliseconds(500);
                },
                onRetryAsync: (result, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Non-idempotent request retry {RetryCount} (connection/timeout error only) after {Delay}ms. Error: {Error}",
                        retryCount,
                        timespan.TotalMilliseconds,
                        result.Exception?.Message);
                    return Task.CompletedTask;
                });
    }

    private static bool IsTransientHttpError(HttpResponseMessage? response)
    {
        if (response == null)
            return false;

        var statusCode = (int)response.StatusCode;
        
        // Transient errors: 408 (RequestTimeout), 429 (TooManyRequests), 5xx (except 501, 505)
        return statusCode == 408 ||
               statusCode == 429 ||
               (statusCode >= 500 && statusCode != 501 && statusCode != 505);
    }
}


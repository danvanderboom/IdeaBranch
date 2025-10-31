using Microsoft.Extensions.Logging;
using Polly;

namespace IdeaBranch.Infrastructure.Resilience;

/// <summary>
/// Emits telemetry events for resilience policy operations (retries, circuit breaker state, outcomes).
/// </summary>
public sealed class ResilienceTelemetryEmitter
{
    private readonly ILogger _logger;

    public ResilienceTelemetryEmitter(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Emits telemetry for a retry attempt.
    /// </summary>
    public void EmitRetryAttempt(
        string policyName,
        int attemptNumber,
        TimeSpan delay,
        string reason,
        Context? context = null)
    {
        var correlationId = context?.ContainsKey("CorrelationId") == true 
            ? context["CorrelationId"]?.ToString() 
            : null;
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N")[..8];
        }
        
        _logger.LogInformation(
            "[Resilience] Retry attempt {AttemptNumber} for policy '{PolicyName}'. Delay: {Delay}ms, Reason: {Reason}, CorrelationId: {CorrelationId}",
            attemptNumber,
            policyName,
            delay.TotalMilliseconds,
            reason,
            correlationId);

        // TODO: Add structured telemetry/metrics collection here
        // e.g., Application Insights, OpenTelemetry, or custom metrics
    }

    /// <summary>
    /// Emits telemetry when circuit breaker opens.
    /// </summary>
    public void EmitCircuitBreakerOpened(
        string policyName,
        TimeSpan duration,
        string reason,
        Context? context = null)
    {
        var correlationId = context?.ContainsKey("CorrelationId") == true 
            ? context["CorrelationId"]?.ToString() 
            : null;
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N")[..8];
        }
        
        _logger.LogWarning(
            "[Resilience] Circuit breaker opened for policy '{PolicyName}'. Duration: {Duration}s, Reason: {Reason}, CorrelationId: {CorrelationId}",
            policyName,
            duration.TotalSeconds,
            reason,
            correlationId);

        // TODO: Add structured telemetry/metrics collection here
    }

    /// <summary>
    /// Emits telemetry when circuit breaker resets (closes).
    /// </summary>
    public void EmitCircuitBreakerReset(string policyName, Context? context = null)
    {
        var correlationId = context?.ContainsKey("CorrelationId") == true 
            ? context["CorrelationId"]?.ToString() 
            : null;
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N")[..8];
        }
        
        _logger.LogInformation(
            "[Resilience] Circuit breaker reset for policy '{PolicyName}'. CorrelationId: {CorrelationId}",
            policyName,
            correlationId);

        // TODO: Add structured telemetry/metrics collection here
    }

    /// <summary>
    /// Emits telemetry when circuit breaker enters half-open state.
    /// </summary>
    public void EmitCircuitBreakerHalfOpen(string policyName)
    {
        _logger.LogInformation(
            "[Resilience] Circuit breaker half-open for policy '{PolicyName}'",
            policyName);

        // TODO: Add structured telemetry/metrics collection here
    }

    /// <summary>
    /// Emits telemetry for successful operation outcome.
    /// </summary>
    public void EmitSuccess(string policyName, TimeSpan duration, Context? context = null)
    {
        var correlationId = context?.ContainsKey("CorrelationId") == true 
            ? context["CorrelationId"]?.ToString() 
            : null;
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N")[..8];
        }
        
        _logger.LogDebug(
            "[Resilience] Operation succeeded for policy '{PolicyName}'. Duration: {Duration}ms, CorrelationId: {CorrelationId}",
            policyName,
            duration.TotalMilliseconds,
            correlationId);

        // TODO: Add structured telemetry/metrics collection here
    }

    /// <summary>
    /// Emits telemetry for failed operation outcome (after all retries exhausted).
    /// </summary>
    public void EmitFailure(string policyName, string reason, Context? context = null)
    {
        var correlationId = context?.ContainsKey("CorrelationId") == true 
            ? context["CorrelationId"]?.ToString() 
            : null;
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N")[..8];
        }
        
        _logger.LogError(
            "[Resilience] Operation failed for policy '{PolicyName}'. Reason: {Reason}, CorrelationId: {CorrelationId}",
            policyName,
            reason,
            correlationId);

        // TODO: Add structured telemetry/metrics collection here
    }
}


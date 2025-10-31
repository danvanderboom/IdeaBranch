using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Polly;

namespace IdeaBranch.Infrastructure.Resilience;

/// <summary>
/// Emits telemetry events for resilience policy operations (retries, circuit breaker state, outcomes).
/// Uses structured logging and OpenTelemetry for observability.
/// </summary>
public sealed class ResilienceTelemetryEmitter
{
    private readonly ILogger _logger;
    private static readonly ActivitySource ActivitySource = new("IdeaBranch.Resilience");

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
        
        // Structured logging with event name
        _logger.LogInformation(
            "Resilience retry event: {Operation} attempt {Attempt} delay {DelayMs}ms reason {Reason}",
            policyName,
            attemptNumber,
            delay.TotalMilliseconds,
            reason);

        // OpenTelemetry activity event
        using var activity = ActivitySource.StartActivity("resilience.retry");
        activity?.SetTag("operation", policyName);
        activity?.SetTag("attempt", attemptNumber);
        activity?.SetTag("delayMs", delay.TotalMilliseconds);
        activity?.SetTag("outcome", "retry");
        activity?.SetTag("reason", reason);
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
        
        // Structured logging with event name
        _logger.LogWarning(
            "Resilience circuit open event: {Operation} duration {DurationMs}ms reason {Reason}",
            policyName,
            duration.TotalMilliseconds,
            reason);

        // OpenTelemetry activity event
        using var activity = ActivitySource.StartActivity("resilience.circuit_open");
        activity?.SetTag("operation", policyName);
        activity?.SetTag("durationMs", duration.TotalMilliseconds);
        activity?.SetTag("outcome", "circuit_open");
        activity?.SetTag("reason", reason);
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
        
        // Structured logging with event name
        _logger.LogInformation(
            "Resilience circuit reset event: {Operation}",
            policyName);

        // OpenTelemetry activity event
        using var activity = ActivitySource.StartActivity("resilience.circuit_reset");
        activity?.SetTag("operation", policyName);
        activity?.SetTag("outcome", "circuit_reset");
    }

    /// <summary>
    /// Emits telemetry when circuit breaker enters half-open state.
    /// </summary>
    public void EmitCircuitBreakerHalfOpen(string policyName)
    {
        // Structured logging with event name
        _logger.LogInformation(
            "Resilience circuit half-open event: {Operation}",
            policyName);

        // OpenTelemetry activity event
        using var activity = ActivitySource.StartActivity("resilience.circuit_half_open");
        activity?.SetTag("operation", policyName);
        activity?.SetTag("outcome", "circuit_half_open");
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
        
        // Structured logging with event name
        _logger.LogDebug(
            "Resilience success event: {Operation} duration {DurationMs}ms",
            policyName,
            duration.TotalMilliseconds);

        // OpenTelemetry activity event
        using var activity = ActivitySource.StartActivity("resilience.success");
        activity?.SetTag("operation", policyName);
        activity?.SetTag("durationMs", duration.TotalMilliseconds);
        activity?.SetTag("outcome", "success");
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
        
        // Structured logging with event name
        _logger.LogError(
            "Resilience failure event: {Operation} reason {Reason}",
            policyName,
            reason);

        // OpenTelemetry activity event
        using var activity = ActivitySource.StartActivity("resilience.failure");
        activity?.SetTag("operation", policyName);
        activity?.SetTag("outcome", "failure");
        activity?.SetTag("reason", reason);
    }
}


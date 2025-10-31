using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace IdeaBranch.App.Services;

/// <summary>
/// Service for emitting telemetry events with consent support.
/// </summary>
public class TelemetryService
{
    private readonly ILogger _logger;
    private static readonly ActivitySource NavigationActivitySource = new("IdeaBranch.Navigation");
    private static readonly ActivitySource CrudActivitySource = new("IdeaBranch.CRUD");
    private static readonly ActivitySource LlmActivitySource = new("IdeaBranch.LLM");
    private bool _consentGranted = true; // Default to true until settings are implemented

    public TelemetryService(ILogger<TelemetryService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets or sets whether telemetry consent is granted.
    /// </summary>
    public bool ConsentGranted
    {
        get => _consentGranted;
        set => _consentGranted = value;
    }

    /// <summary>
    /// Emits a navigation telemetry event.
    /// </summary>
    public void EmitNavigationEvent(string pageName)
    {
        if (!_consentGranted)
            return;

        // Structured logging
        _logger.LogInformation(
            "Navigation event: {Page}",
            pageName);

        // OpenTelemetry activity event
        using var activity = NavigationActivitySource.StartActivity($"navigation.{pageName.ToLowerInvariant()}");
        activity?.SetTag("page", pageName);
        activity?.SetTag("event", "navigation");
    }

    /// <summary>
    /// Emits a CRUD operation telemetry event.
    /// </summary>
    /// <param name="operation">The CRUD operation (e.g., "create", "update", "delete", "move").</param>
    /// <param name="nodeId">The ID of the node being operated on.</param>
    public void EmitCrudEvent(string operation, string nodeId)
    {
        if (!_consentGranted)
            return;

        // Structured logging
        _logger.LogInformation(
            "CRUD event: {Operation} on node {NodeId}",
            operation,
            nodeId);

        // OpenTelemetry activity event
        using var activity = CrudActivitySource.StartActivity($"crud.{operation.ToLowerInvariant()}");
        activity?.SetTag("operation", operation);
        activity?.SetTag("node_id", nodeId);
        activity?.SetTag("event", "crud");
    }

    /// <summary>
    /// Emits an LLM operation telemetry event.
    /// </summary>
    /// <param name="operation">The LLM operation (e.g., "generate_response", "suggest_title").</param>
    /// <param name="nodeId">The ID of the node being operated on.</param>
    /// <param name="success">Whether the operation succeeded.</param>
    /// <param name="errorMessage">Optional error message if the operation failed.</param>
    public void EmitLlmEvent(string operation, string nodeId, bool success, string? errorMessage = null)
    {
        if (!_consentGranted)
            return;

        // Structured logging
        if (success)
        {
            _logger.LogInformation(
                "LLM event: {Operation} succeeded for node {NodeId}",
                operation,
                nodeId);
        }
        else
        {
            _logger.LogWarning(
                "LLM event: {Operation} failed for node {NodeId}: {ErrorMessage}",
                operation,
                nodeId,
                errorMessage ?? "Unknown error");
        }

        // OpenTelemetry activity event
        using var activity = LlmActivitySource.StartActivity($"llm.{operation.ToLowerInvariant()}");
        activity?.SetTag("operation", operation);
        activity?.SetTag("node_id", nodeId);
        activity?.SetTag("success", success.ToString());
        activity?.SetTag("event", "llm");
        
        if (!success && !string.IsNullOrWhiteSpace(errorMessage))
        {
            activity?.SetTag("error", errorMessage);
        }
    }
}

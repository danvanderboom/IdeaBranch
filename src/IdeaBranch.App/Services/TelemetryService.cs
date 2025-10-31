using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace IdeaBranch.App.Services;

/// <summary>
/// Service for emitting telemetry events with consent support.
/// </summary>
public class TelemetryService
{
    private readonly ILogger _logger;
    private static readonly ActivitySource ActivitySource = new("IdeaBranch.Navigation");
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
        using var activity = ActivitySource.StartActivity($"navigation.{pageName.ToLowerInvariant()}");
        activity?.SetTag("page", pageName);
        activity?.SetTag("event", "navigation");
    }
}

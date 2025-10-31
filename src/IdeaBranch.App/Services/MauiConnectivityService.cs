using IdeaBranch.Infrastructure.Sync;
using Microsoft.Maui.Networking;

namespace IdeaBranch.App.Services;

/// <summary>
/// MAUI-based implementation of IConnectivityService.
/// Uses Microsoft.Maui.Networking.Connectivity to detect network availability.
/// </summary>
public class MauiConnectivityService : IConnectivityService
{
    /// <summary>
    /// Initializes a new instance of the MauiConnectivityService class.
    /// Subscribes to connectivity changes.
    /// </summary>
    public MauiConnectivityService()
    {
        // Subscribe to connectivity changes
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    /// <inheritdoc/>
    public Task<bool> IsOnlineAsync(CancellationToken cancellationToken = default)
    {
        // Check NetworkAccess from MAUI Connectivity
        var networkAccess = Connectivity.Current.NetworkAccess;
        var isOnline = networkAccess == NetworkAccess.Internet ||
                      networkAccess == NetworkAccess.ConstrainedInternet;

        return Task.FromResult(isOnline);
    }

    /// <inheritdoc/>
    public event EventHandler? ConnectivityChanged;

    /// <summary>
    /// Handles connectivity change events from MAUI Connectivity.
    /// </summary>
    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        ConnectivityChanged?.Invoke(this, EventArgs.Empty);
    }
}


using IdeaBranch.Infrastructure.Sync;

namespace IdeaBranch.IntegrationTests.Sync;

/// <summary>
/// Helper class to simulate offline/online states for testing sync workflows.
/// </summary>
public class OfflineSimulator : ISyncService
{
    private bool _isOnline = true;
    private bool _hasPendingChanges = false;

    /// <summary>
    /// Sets the system to offline state.
    /// </summary>
    public void SetOffline()
    {
        var wasOnline = _isOnline;
        _isOnline = false;
        
        if (wasOnline)
        {
            ConnectivityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Sets the system to online state.
    /// </summary>
    public void SetOnline()
    {
        var wasOffline = !_isOnline;
        _isOnline = true;
        
        if (wasOffline)
        {
            ConnectivityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Manually triggers the connectivity changed event.
    /// </summary>
    public void RaiseConnectivityChanged()
    {
        ConnectivityChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Marks that there are pending changes waiting to sync.
    /// </summary>
    public void MarkPendingChanges()
    {
        _hasPendingChanges = true;
    }

    /// <summary>
    /// Clears the pending changes flag (after successful sync).
    /// </summary>
    public void ClearPendingChanges()
    {
        _hasPendingChanges = false;
    }

    public Task<bool> IsOnlineAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_isOnline);
    }

    public async Task<bool> SyncPendingChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!_isOnline)
        {
            return false;
        }

        if (!_hasPendingChanges)
        {
            return true;
        }

        // Simulate sync operation
        await Task.Delay(50, cancellationToken);
        
        _hasPendingChanges = false;
        return true;
    }

    public bool HasPendingChanges => _hasPendingChanges;

    public event EventHandler? ConnectivityChanged;
}


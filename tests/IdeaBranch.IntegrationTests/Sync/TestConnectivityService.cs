using IdeaBranch.Infrastructure.Sync;

namespace IdeaBranch.IntegrationTests.Sync;

/// <summary>
/// Test implementation of IConnectivityService for integration testing.
/// </summary>
public class TestConnectivityService : IConnectivityService
{
    private bool _isOnline = true;

    /// <summary>
    /// Sets the online state.
    /// </summary>
    public void SetOnline(bool online)
    {
        var wasOnline = _isOnline;
        _isOnline = online;

        if (wasOnline != _isOnline)
        {
            ConnectivityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    public Task<bool> IsOnlineAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_isOnline);
    }

    /// <inheritdoc/>
    public event EventHandler? ConnectivityChanged;
}


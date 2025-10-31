namespace IdeaBranch.Infrastructure.Sync;

/// <summary>
/// Interface for connectivity detection services.
/// Abstracts platform-specific connectivity checking.
/// </summary>
public interface IConnectivityService
{
    /// <summary>
    /// Checks if the system is currently online.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if online, false if offline.</returns>
    Task<bool> IsOnlineAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when connectivity status changes (online/offline).
    /// </summary>
    event EventHandler? ConnectivityChanged;
}


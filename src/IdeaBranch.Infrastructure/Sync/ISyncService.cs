namespace IdeaBranch.Infrastructure.Sync;

/// <summary>
/// Interface for synchronization service operations.
/// Enables testing of sync workflows without requiring full implementation.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Checks if the system is currently online and able to sync.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if online, false if offline.</returns>
    Task<bool> IsOnlineAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes pending local changes to the remote server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if sync completed successfully, false otherwise.</returns>
    Task<bool> SyncPendingChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether there are pending changes waiting to be synchronized.
    /// </summary>
    bool HasPendingChanges { get; }

    /// <summary>
    /// Event raised when connectivity status changes (online/offline).
    /// </summary>
    event EventHandler? ConnectivityChanged;
}


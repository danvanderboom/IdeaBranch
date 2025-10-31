using IdeaBranch.Domain;

namespace IdeaBranch.Infrastructure.Sync;

/// <summary>
/// Interface for remote synchronization client operations.
/// Handles pushing local changes and pulling remote changes from a remote server.
/// </summary>
public interface IRemoteSyncClient
{
    /// <summary>
    /// Pings the remote server to verify connectivity.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if ping succeeded, false otherwise.</returns>
    Task<bool> PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pulls topic nodes that have been updated since the specified timestamp.
    /// </summary>
    /// <param name="since">The timestamp to pull changes since.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of topic nodes that have been updated since the timestamp.</returns>
    Task<IReadOnlyList<TopicNode>> PullChangesSinceAsync(DateTime since, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pushes local topic nodes to the remote server.
    /// </summary>
    /// <param name="nodes">The topic nodes to push.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if push succeeded, false otherwise.</returns>
    Task<bool> PushChangesAsync(IReadOnlyList<TopicNode> nodes, CancellationToken cancellationToken = default);
}


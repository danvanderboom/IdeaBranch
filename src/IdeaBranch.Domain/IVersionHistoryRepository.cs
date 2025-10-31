using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.Domain;

/// <summary>
/// Repository interface for version history operations.
/// </summary>
public interface IVersionHistoryRepository
{
    /// <summary>
    /// Saves a version history entry for a topic node.
    /// </summary>
    /// <param name="version">The version history entry to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveAsync(TopicNodeVersion version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all version history entries for a topic node, ordered by timestamp descending (newest first).
    /// </summary>
    /// <param name="nodeId">The identifier of the topic node.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of version history entries, ordered by timestamp descending.</returns>
    Task<IReadOnlyList<TopicNodeVersion>> GetByNodeIdAsync(Guid nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent version history entry for a topic node, if any.
    /// </summary>
    /// <param name="nodeId">The identifier of the topic node.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The most recent version history entry, or null if none exists.</returns>
    Task<TopicNodeVersion?> GetLatestAsync(Guid nodeId, CancellationToken cancellationToken = default);
}


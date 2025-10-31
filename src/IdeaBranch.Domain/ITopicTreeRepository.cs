namespace IdeaBranch.Domain;

/// <summary>
/// Repository interface for persisting and loading topic tree structures.
/// </summary>
public interface ITopicTreeRepository
{
    /// <summary>
    /// Gets the root topic node of the tree.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The root topic node, or creates a default root if none exists.</returns>
    Task<TopicNode> GetRootAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the topic tree starting from the root node.
    /// </summary>
    /// <param name="root">The root topic node to save.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the save operation.</returns>
    Task SaveAsync(TopicNode root, CancellationToken cancellationToken = default);
}

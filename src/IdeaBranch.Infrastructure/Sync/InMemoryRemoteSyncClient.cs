using System.Collections.Concurrent;
using IdeaBranch.Domain;

namespace IdeaBranch.Infrastructure.Sync;

/// <summary>
/// In-memory implementation of IRemoteSyncClient for testing and development.
/// Stores topic nodes in memory and simulates remote server behavior.
/// Can be replaced with a REST client implementation later.
/// </summary>
public class InMemoryRemoteSyncClient : IRemoteSyncClient
{
    private readonly ConcurrentDictionary<Guid, TopicNode> _remoteNodes = new();
    private bool _pingSucceeds = true;

    /// <summary>
    /// Sets whether ping operations should succeed (for testing offline scenarios).
    /// </summary>
    public void SetPingSucceeds(bool succeeds)
    {
        _pingSucceeds = succeeds;
    }

    /// <inheritdoc/>
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_pingSucceeds);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<TopicNode>> PullChangesSinceAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        var changes = _remoteNodes.Values
            .Where(node => node.UpdatedAt > since)
            .OrderBy(node => node.UpdatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<TopicNode>>(changes);
    }

    /// <inheritdoc/>
    public Task<bool> PushChangesAsync(IReadOnlyList<TopicNode> nodes, CancellationToken cancellationToken = default)
    {
        // Store nodes in remote (cloning to avoid reference issues)
        foreach (var node in nodes)
        {
            // Create a copy to avoid sharing references
            var remoteNode = CloneNode(node);
            _remoteNodes.AddOrUpdate(
                remoteNode.Id,
                remoteNode,
                (key, existing) => remoteNode);
        }

        return Task.FromResult(true);
    }

    /// <summary>
    /// Clones a topic node and its children recursively.
    /// </summary>
    private static TopicNode CloneNode(TopicNode source)
    {
        var cloned = new TopicNode(source.Prompt, source.Title)
        {
            Response = source.Response,
            Order = source.Order
        };

        // Use reflection to set internal properties (Id, CreatedAt, UpdatedAt)
        var idProperty = typeof(TopicNode).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var createdAtProperty = typeof(TopicNode).GetProperty("CreatedAt", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var updatedAtProperty = typeof(TopicNode).GetProperty("UpdatedAt", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (idProperty?.SetMethod != null)
            idProperty.SetValue(cloned, source.Id);
        if (createdAtProperty?.SetMethod != null)
            createdAtProperty.SetValue(cloned, source.CreatedAt);
        if (updatedAtProperty?.SetMethod != null)
            updatedAtProperty.SetValue(cloned, source.UpdatedAt);

        // Clone children recursively
        foreach (var child in source.Children)
        {
            var clonedChild = CloneNode(child);
            cloned.AddChild(clonedChild);
        }

        return cloned;
    }

    /// <summary>
    /// Clears all stored remote nodes (for testing).
    /// </summary>
    public void Clear()
    {
        _remoteNodes.Clear();
    }

    /// <summary>
    /// Gets all stored remote nodes (for testing).
    /// </summary>
    public IReadOnlyList<TopicNode> GetAllNodes()
    {
        return _remoteNodes.Values.ToList();
    }
}


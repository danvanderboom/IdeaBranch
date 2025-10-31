using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.Domain;

/// <summary>
/// In-memory implementation of ITopicTreeRepository for testing or when no persistence is needed.
/// </summary>
public class InMemoryTopicTreeRepository : ITopicTreeRepository
{
    private TopicNode? _root;

    /// <inheritdoc/>
    public Task<TopicNode> GetRootAsync(CancellationToken cancellationToken = default)
    {
        if (_root == null)
        {
            _root = new TopicNode("What would you like to explore?", "Root Topic");
            _root.SetResponse("This is a placeholder response.", parseListItems: false);
        }

        return Task.FromResult(_root);
    }

    /// <inheritdoc/>
    public Task SaveAsync(TopicNode root, CancellationToken cancellationToken = default)
    {
        _root = root;
        return Task.CompletedTask;
    }
}

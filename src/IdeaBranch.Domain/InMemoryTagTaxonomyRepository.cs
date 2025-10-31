using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.Domain;

/// <summary>
/// In-memory implementation of ITagTaxonomyRepository for testing or when no persistence is needed.
/// </summary>
public class InMemoryTagTaxonomyRepository : ITagTaxonomyRepository
{
    private TagTaxonomyNode? _root;

    /// <inheritdoc/>
    public Task<TagTaxonomyNode> GetRootAsync(CancellationToken cancellationToken = default)
    {
        if (_root == null)
        {
            _root = new TagTaxonomyNode("Root", null);
        }

        return Task.FromResult(_root);
    }

    /// <inheritdoc/>
    public Task<TagTaxonomyNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_root == null)
            return Task.FromResult<TagTaxonomyNode?>(null);

        var node = FindNode(_root, id);
        return Task.FromResult(node);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<TagTaxonomyNode>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        if (_root == null)
            return Task.FromResult<IReadOnlyList<TagTaxonomyNode>>(Array.Empty<TagTaxonomyNode>());

        if (parentId == null)
        {
            // Return root's children
            return Task.FromResult<IReadOnlyList<TagTaxonomyNode>>(_root.Children);
        }

        var parent = FindNode(_root, parentId.Value);
        if (parent == null)
            return Task.FromResult<IReadOnlyList<TagTaxonomyNode>>(Array.Empty<TagTaxonomyNode>());

        return Task.FromResult<IReadOnlyList<TagTaxonomyNode>>(parent.Children);
    }

    /// <inheritdoc/>
    public Task SaveAsync(TagTaxonomyNode node, CancellationToken cancellationToken = default)
    {
        // Just store the root - in memory, we keep the tree structure
        if (node.ParentId == null)
        {
            _root = node;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_root == null)
            return Task.FromResult(false);

        if (_root.Id == id)
        {
            _root = null;
            return Task.FromResult(true);
        }

        var node = FindNode(_root, id);
        if (node?.Parent != null)
        {
            node.Parent.RemoveChild(node);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    private TagTaxonomyNode? FindNode(TagTaxonomyNode node, Guid id)
    {
        if (node.Id == id)
            return node;

        foreach (var child in node.Children)
        {
            var found = FindNode(child, id);
            if (found != null)
                return found;
        }

        return null;
    }
}


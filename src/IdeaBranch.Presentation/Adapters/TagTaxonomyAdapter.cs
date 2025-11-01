using System;
using System.Collections.Generic;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.Domain;

namespace IdeaBranch.Presentation.Adapters;

/// <summary>
/// Adapter that builds TreeNode&lt;TagTaxonomyPayload&gt; tree from domain TagTaxonomyNode aggregate.
/// Maintains mapping between DomainNodeId and ITreeNode for bidirectional updates.
/// </summary>
public class TagTaxonomyAdapter
{
    private readonly Dictionary<Guid, ITreeNode> _domainNodeIdToTreeNode = new();
    private readonly Dictionary<ITreeNode, Guid> _treeNodeToDomainNodeId = new();

    /// <summary>
    /// Builds a TreeNode tree from a domain tag taxonomy tree structure.
    /// </summary>
    /// <param name="rootDomainNode">The root domain TagTaxonomyNode</param>
    /// <returns>Root TreeNode&lt;TagTaxonomyPayload&gt; suitable for TreeView</returns>
    public TreeNode<TagTaxonomyPayload> BuildTree(TagTaxonomyNode rootDomainNode)
    {
        if (rootDomainNode == null)
            throw new ArgumentNullException(nameof(rootDomainNode));

        // Clear mappings for a fresh build
        _domainNodeIdToTreeNode.Clear();
        _treeNodeToDomainNodeId.Clear();

        // Build the tree recursively
        var rootTreeNode = BuildTreeNode(rootDomainNode, null);
        return rootTreeNode;
    }

    /// <summary>
    /// Recursively builds a TreeNode from a domain TagTaxonomyNode.
    /// </summary>
    private TreeNode<TagTaxonomyPayload> BuildTreeNode(TagTaxonomyNode domainNode, TreeNode<TagTaxonomyPayload>? parentTreeNode)
    {
        var payload = new TagTaxonomyPayload
        {
            DomainNodeId = domainNode.Id,
            Name = domainNode.Name,
            Order = domainNode.Order,
            CreatedAt = domainNode.CreatedAt,
            UpdatedAt = domainNode.UpdatedAt
        };

        var treeNode = parentTreeNode == null
            ? new TreeNode<TagTaxonomyPayload>(payload)
            : new TreeNode<TagTaxonomyPayload>(payload, parentTreeNode);
        
        // Add to mappings
        _domainNodeIdToTreeNode[domainNode.Id] = treeNode;
        _treeNodeToDomainNodeId[treeNode] = domainNode.Id;

        // Recursively add children
        foreach (var childDomainNode in domainNode.Children)
        {
            var childTreeNode = BuildTreeNode(childDomainNode, treeNode);
            // Avoid SetParent early-return issue when parent is already set by constructor
            treeNode.Children.Add(childTreeNode, updateParent: false);
        }

        return treeNode;
    }

    /// <summary>
    /// Gets the TreeNode associated with a domain node ID.
    /// </summary>
    public ITreeNode? GetTreeNode(Guid domainNodeId)
    {
        return _domainNodeIdToTreeNode.TryGetValue(domainNodeId, out var node) ? node : null;
    }

    /// <summary>
    /// Gets the domain node ID associated with a TreeNode.
    /// </summary>
    public Guid? GetDomainNodeId(ITreeNode treeNode)
    {
        return _treeNodeToDomainNodeId.TryGetValue(treeNode, out var id) ? id : null;
    }
}


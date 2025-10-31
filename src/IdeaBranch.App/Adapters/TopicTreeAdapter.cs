using System;
using System.Collections.Generic;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.Domain;

namespace IdeaBranch.App.Adapters;

/// <summary>
/// Adapter that builds TreeNode&lt;TopicNodePayload&gt; tree from domain TopicNode aggregate.
/// Maintains mapping between DomainNodeId and ITreeNode for bidirectional updates.
/// </summary>
public class TopicTreeAdapter
{
    private readonly Dictionary<Guid, ITreeNode> _domainNodeIdToTreeNode = new();
    private readonly Dictionary<ITreeNode, Guid> _treeNodeToDomainNodeId = new();

    /// <summary>
    /// Builds a TreeNode tree from a domain topic tree structure.
    /// </summary>
    /// <param name="rootDomainNode">The root domain TopicNode</param>
    /// <returns>Root TreeNode&lt;TopicNodePayload&gt; suitable for TreeView</returns>
    public TreeNode<TopicNodePayload> BuildTree(TopicNode rootDomainNode)
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
    /// Recursively builds a TreeNode from a domain TopicNode.
    /// </summary>
    private TreeNode<TopicNodePayload> BuildTreeNode(TopicNode domainNode, TreeNode<TopicNodePayload>? parentTreeNode)
    {
        var payload = new TopicNodePayload
        {
            DomainNodeId = domainNode.Id,
            Title = domainNode.Title,
            Prompt = domainNode.Prompt,
            Response = domainNode.Response,
            Order = domainNode.Order,
            CreatedAt = domainNode.CreatedAt,
            UpdatedAt = domainNode.UpdatedAt
        };

        var treeNode = new TreeNode<TopicNodePayload>(payload, parentTreeNode);
        
        // Add to mappings
        _domainNodeIdToTreeNode[domainNode.Id] = treeNode;
        _treeNodeToDomainNodeId[treeNode] = domainNode.Id;

        // Recursively add children
        foreach (var childDomainNode in domainNode.Children)
        {
            var childTreeNode = BuildTreeNode(childDomainNode, treeNode);
            treeNode.Children.Add(childTreeNode);
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


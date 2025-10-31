using System;
using System.Collections.Generic;
using CriticalInsight.Data.Hierarchical;

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
    /// <param name="rootDomainNode">The root domain node (placeholder until domain model exists)</param>
    /// <returns>Root TreeNode&lt;TopicNodePayload&gt; suitable for TreeView</returns>
    public TreeNode<TopicNodePayload> BuildTree(object rootDomainNode)
    {
        // TODO: Replace with actual domain TopicNode once implemented
        // For now, create a deterministic 3-level tree for testing with stable GUIDs
        
        // Fixed GUIDs for deterministic AutomationIds in tests
        var rootId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var childId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var grandchildId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        
        var rootPayload = new TopicNodePayload
        {
            DomainNodeId = rootId,
            Title = "Root Topic",
            Prompt = "What would you like to explore?",
            Response = "This is a placeholder response.",
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var rootTreeNode = new TreeNode<TopicNodePayload>(rootPayload);
        _domainNodeIdToTreeNode[rootPayload.DomainNodeId] = rootTreeNode;
        _treeNodeToDomainNodeId[rootTreeNode] = rootPayload.DomainNodeId;

        // Add child node
        var childPayload = new TopicNodePayload
        {
            DomainNodeId = childId,
            Title = "Child Topic",
            Prompt = "Explore this child",
            Response = "Child response content.",
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var childTreeNode = new TreeNode<TopicNodePayload>(childPayload, rootTreeNode);
        rootTreeNode.Children.Add(childTreeNode);
        _domainNodeIdToTreeNode[childPayload.DomainNodeId] = childTreeNode;
        _treeNodeToDomainNodeId[childTreeNode] = childPayload.DomainNodeId;

        // Add grandchild node
        var grandchildPayload = new TopicNodePayload
        {
            DomainNodeId = grandchildId,
            Title = "Grandchild Topic",
            Prompt = "Explore this grandchild",
            Response = "Grandchild response content.",
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var grandchildTreeNode = new TreeNode<TopicNodePayload>(grandchildPayload, childTreeNode);
        childTreeNode.Children.Add(grandchildTreeNode);
        _domainNodeIdToTreeNode[grandchildPayload.DomainNodeId] = grandchildTreeNode;
        _treeNodeToDomainNodeId[grandchildTreeNode] = grandchildPayload.DomainNodeId;

        return rootTreeNode;
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

    // TODO: Implement recursive child building once domain model exists
    // private void AddChildNodes(TopicNode domainNode, TreeNode<TopicNodePayload> parentTreeNode)
    // {
    //     foreach (var childDomainNode in domainNode.Children)
    //     {
    //         var childPayload = new TopicNodePayload
    //         {
    //             DomainNodeId = childDomainNode.Id,
    //             Title = childDomainNode.Title,
    //             Prompt = childDomainNode.Prompt,
    //             Response = childDomainNode.Response,
    //             Order = childDomainNode.Order,
    //             CreatedAt = childDomainNode.CreatedAt,
    //             UpdatedAt = childDomainNode.UpdatedAt
    //         };
    //         
    //         var childTreeNode = new TreeNode<TopicNodePayload>(childPayload, parentTreeNode);
    //         _domainNodeIdToTreeNode[childDomainNode.Id] = childTreeNode;
    //         _treeNodeToDomainNodeId[childTreeNode] = childDomainNode.Id;
    //         
    //         AddChildNodes(childDomainNode, childTreeNode);
    //     }
    // }
}


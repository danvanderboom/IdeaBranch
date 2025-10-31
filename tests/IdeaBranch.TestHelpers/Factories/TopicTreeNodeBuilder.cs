using System;
using CriticalInsight.Data.Hierarchical;

namespace IdeaBranch.TestHelpers.Factories;

/// <summary>
/// Builder for creating TreeNode instances with TestTopicNodePayload.
/// Supports building trees with parent-child relationships.
/// </summary>
public class TopicTreeNodeBuilder : EntityBuilder<TreeNode<TestTopicNodePayload>>
{
    private Guid? _nodeId;
    private TestTopicNodePayload? _payload;
    private TreeNode<TestTopicNodePayload>? _parent;
    private readonly List<TopicTreeNodeBuilder> _children = new();

    /// <summary>
    /// Initializes a new instance with an optional seed for deterministic generation.
    /// </summary>
    /// <param name="seed">The seed value. Defaults to 1.</param>
    public TopicTreeNodeBuilder(int? seed = null)
        : base(seed)
    {
    }

    /// <summary>
    /// Sets the node ID.
    /// </summary>
    public TopicTreeNodeBuilder WithNodeId(Guid nodeId)
    {
        _nodeId = nodeId;
        return this;
    }

    /// <summary>
    /// Sets the payload using a builder action.
    /// </summary>
    public TopicTreeNodeBuilder WithPayload(Action<TopicNodePayloadBuilder> configure)
    {
        var payloadBuilder = new TopicNodePayloadBuilder(Seed);
        configure(payloadBuilder);
        _payload = payloadBuilder.Build();
        return this;
    }

    /// <summary>
    /// Sets the payload directly.
    /// </summary>
    public TopicTreeNodeBuilder WithPayload(TestTopicNodePayload payload)
    {
        _payload = payload;
        return this;
    }

    /// <summary>
    /// Adds a child node using a builder action.
    /// </summary>
    public TopicTreeNodeBuilder WithChild(Action<TopicTreeNodeBuilder> configure)
    {
        var childBuilder = new TopicTreeNodeBuilder(Seed + _children.Count + 1);
        configure(childBuilder);
        _children.Add(childBuilder);
        return this;
    }

    /// <summary>
    /// Sets the parent node. Used internally when building children.
    /// </summary>
    internal void SetParent(TreeNode<TestTopicNodePayload> parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Builds and returns a TreeNode with payload and relationships configured.
    /// </summary>
    public override TreeNode<TestTopicNodePayload> Build()
    {
        var payload = _payload ?? new TopicNodePayloadBuilder(Seed).Build();
        var nodeId = _nodeId ?? payload.DomainNodeId;

        var treeNode = new TreeNode<TestTopicNodePayload>(payload);
        treeNode.NodeId = nodeId.ToString();

        // Build and attach children
        foreach (var childBuilder in _children)
        {
            var childNode = childBuilder.Build();
            childNode.SetParent(treeNode, updateChildNodes: true);
        }

        // Attach to parent if provided
        if (_parent != null)
        {
            treeNode.SetParent(_parent, updateChildNodes: true);
        }

        return treeNode;
    }
}


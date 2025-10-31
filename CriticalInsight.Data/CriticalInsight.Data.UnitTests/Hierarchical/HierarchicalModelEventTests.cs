using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using NUnit.Framework;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

[TestFixture]
public class HierarchicalModelEventTests
{
    [Test]
    public void RaiseDepthChangedEvent_BubblesUpToRoot()
    {
        // Arrange: Create a tree structure
        var root = new TreeNode<Space>();
        root.Payload.Name = "Root";
        
        var child1 = new TreeNode<Space>();
        child1.Payload.Name = "Child1";
        
        var grandchild1 = new TreeNode<Space>();
        grandchild1.Payload.Name = "Grandchild1";
        
        child1.Children.Add(grandchild1);
        root.Children.Add(child1);

        // Track PropertyChanged events
        var propertyChangedEvents = new List<(object? sender, PropertyChangedEventArgs e)>();
        
        root.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));
        child1.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));
        grandchild1.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));

        // Act: Move grandchild1 to be a direct child of root (changes depth)
        grandchild1.SetParent(root, updateChildNodes: false);

        // Assert: Depth change should bubble up to root
        Assert.That(propertyChangedEvents.Count, Is.GreaterThan(0));
        
        // Verify that Depth property changed events were raised
        var depthChangedEvents = propertyChangedEvents.Where(e => e.e.PropertyName == "Depth").ToList();
        Assert.That(depthChangedEvents.Count, Is.GreaterThan(0));
        
        // Verify the events were raised on the correct nodes
        var depthChangedSenders = depthChangedEvents.Select(e => e.sender).ToList();
        Assert.That(depthChangedSenders, Does.Contain(grandchild1));
        Assert.That(depthChangedSenders, Does.Contain(root));
    }

    [Test]
    public void RaiseAncestorChangedEvent_NotifiesAllDescendants()
    {
        // Arrange: Create a tree structure
        var root = new TreeNode<Space>();
        root.Payload.Name = "Root";
        
        var child1 = new TreeNode<Space>();
        child1.Payload.Name = "Child1";
        
        var child2 = new TreeNode<Space>();
        child2.Payload.Name = "Child2";
        
        var grandchild1 = new TreeNode<Space>();
        grandchild1.Payload.Name = "Grandchild1";
        
        var grandchild2 = new TreeNode<Space>();
        grandchild2.Payload.Name = "Grandchild2";
        
        child1.Children.Add(grandchild1);
        child1.Children.Add(grandchild2);
        root.Children.Add(child1);
        root.Children.Add(child2);

        // Track AncestorChanged events
        var ancestorChangedEvents = new List<(ITreeNode sender, NodeChangeType changeType, ITreeNode node)>();
        
        root.AncestorChanged += (changeType, node) => ancestorChangedEvents.Add((root, changeType, node));
        child1.AncestorChanged += (changeType, node) => ancestorChangedEvents.Add((child1, changeType, node));
        child2.AncestorChanged += (changeType, node) => ancestorChangedEvents.Add((child2, changeType, node));
        grandchild1.AncestorChanged += (changeType, node) => ancestorChangedEvents.Add((grandchild1, changeType, node));
        grandchild2.AncestorChanged += (changeType, node) => ancestorChangedEvents.Add((grandchild2, changeType, node));

        // Act: Raise ancestor changed event from root
        root.RaiseAncestorChangedEvent(NodeChangeType.NodeAdded, root);

        // Assert: All descendants should be notified
        Assert.That(ancestorChangedEvents.Count, Is.EqualTo(5)); // child1, child2, grandchild1, grandchild2, root
        
        // Verify all descendants received the event
        var notifiedNodes = ancestorChangedEvents.Select(e => e.sender).ToList();
        Assert.That(notifiedNodes, Does.Contain(child1));
        Assert.That(notifiedNodes, Does.Contain(child2));
        Assert.That(notifiedNodes, Does.Contain(grandchild1));
        Assert.That(notifiedNodes, Does.Contain(grandchild2));
        Assert.That(notifiedNodes, Does.Contain(root));
        
        // Verify the event details
        Assert.That(ancestorChangedEvents.All(e => e.changeType == NodeChangeType.NodeAdded), Is.True);
        Assert.That(ancestorChangedEvents.All(e => e.node == root), Is.True);
    }

    [Test]
    public void RaiseDescendantChangedEvent_BubblesUpToRoot()
    {
        // Arrange: Create a tree structure
        var root = new TreeNode<Space>();
        root.Payload.Name = "Root";
        
        var child1 = new TreeNode<Space>();
        child1.Payload.Name = "Child1";
        
        var grandchild1 = new TreeNode<Space>();
        grandchild1.Payload.Name = "Grandchild1";
        
        child1.Children.Add(grandchild1);
        root.Children.Add(child1);

        // Track DescendantChanged events
        var descendantChangedEvents = new List<(ITreeNode sender, NodeChangeType changeType, ITreeNode node)>();
        
        root.DescendantChanged += (changeType, node) => descendantChangedEvents.Add((root, changeType, node));
        child1.DescendantChanged += (changeType, node) => descendantChangedEvents.Add((child1, changeType, node));
        grandchild1.DescendantChanged += (changeType, node) => descendantChangedEvents.Add((grandchild1, changeType, node));

        // Act: Raise descendant changed event from grandchild1
        grandchild1.RaiseDescendantChangedEvent(NodeChangeType.NodeRemoved, grandchild1);

        // Assert: Event should bubble up to root
        Assert.That(descendantChangedEvents.Count, Is.EqualTo(3)); // grandchild1, child1, root
        
        // Verify the event bubbled up correctly
        var notifiedNodes = descendantChangedEvents.Select(e => e.sender).ToList();
        Assert.That(notifiedNodes, Does.Contain(grandchild1));
        Assert.That(notifiedNodes, Does.Contain(child1));
        Assert.That(notifiedNodes, Does.Contain(root));
        
        // Verify the event details
        Assert.That(descendantChangedEvents.All(e => e.changeType == NodeChangeType.NodeRemoved), Is.True);
        Assert.That(descendantChangedEvents.All(e => e.node == grandchild1), Is.True);
    }

    [Test]
    public void SetParent_TriggersAppropriateEvents()
    {
        // Arrange: Create a tree structure
        var root = new TreeNode<Space>();
        root.Payload.Name = "Root";
        
        var child1 = new TreeNode<Space>();
        child1.Payload.Name = "Child1";
        
        var child2 = new TreeNode<Space>();
        child2.Payload.Name = "Child2";
        
        var grandchild1 = new TreeNode<Space>();
        grandchild1.Payload.Name = "Grandchild1";
        
        child1.Children.Add(grandchild1);
        root.Children.Add(child1);
        root.Children.Add(child2);

        // Track events
        var descendantChangedEvents = new List<(ITreeNode sender, NodeChangeType changeType, ITreeNode node)>();
        var propertyChangedEvents = new List<(object? sender, PropertyChangedEventArgs e)>();
        
        root.DescendantChanged += (changeType, node) => descendantChangedEvents.Add((root, changeType, node));
        child1.DescendantChanged += (changeType, node) => descendantChangedEvents.Add((child1, changeType, node));
        child2.DescendantChanged += (changeType, node) => descendantChangedEvents.Add((child2, changeType, node));
        
        root.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));
        child1.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));
        child2.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));
        grandchild1.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));

        // Act: Move grandchild1 from child1 to child2
        grandchild1.SetParent(child2, updateChildNodes: false);

        // Assert: Appropriate events should be triggered
        // Should have descendant changed events for removal and addition
        Assert.That(descendantChangedEvents.Count, Is.GreaterThan(0));
        
        // Verify grandchild1's depth is still 2 (Root -> Child2 -> Grandchild1)
        Assert.That(grandchild1.Depth, Is.EqualTo(2));
        
        // Verify descendant changed events were triggered
        Assert.That(descendantChangedEvents.Any(e => e.changeType == NodeChangeType.NodeRemoved), Is.True);
        Assert.That(descendantChangedEvents.Any(e => e.changeType == NodeChangeType.NodeAdded), Is.True);
    }

    [Test]
    public void PropertyChanged_IsRaisedOnParentChange()
    {
        // Arrange: Create nodes
        var root = new TreeNode<Space>();
        root.Payload.Name = "Root";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";

        // Track PropertyChanged events
        var propertyChangedEvents = new List<(object? sender, PropertyChangedEventArgs e)>();
        child.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));

        // Act: Change parent
        child.SetParent(root, updateChildNodes: false);

        // Assert: Parent property changed event should be raised
        var parentChangedEvents = propertyChangedEvents.Where(e => e.e.PropertyName == "Parent").ToList();
        Assert.That(parentChangedEvents.Count, Is.EqualTo(1));
        Assert.That(parentChangedEvents[0].sender, Is.EqualTo(child));
    }

    [Test]
    public void PropertyChanged_IsRaisedOnPayloadChange()
    {
        // Arrange: Create a node
        var node = new TreeNode<Space>();
        node.Payload.Name = "Original";

        // Track PropertyChanged events
        var propertyChangedEvents = new List<(object? sender, PropertyChangedEventArgs e)>();
        node.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));

        // Act: Change payload
        node.PayloadObject = new Space { Name = "Changed" };

        // Assert: Payload property changed event should be raised
        var payloadChangedEvents = propertyChangedEvents.Where(e => e.e.PropertyName == "Payload").ToList();
        Assert.That(payloadChangedEvents.Count, Is.EqualTo(1));
        Assert.That(payloadChangedEvents[0].sender, Is.EqualTo(node));
    }
}

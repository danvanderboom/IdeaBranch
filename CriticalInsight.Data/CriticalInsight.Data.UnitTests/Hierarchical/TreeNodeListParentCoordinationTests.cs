using System;
using System.Collections.Generic;
using System.Linq;
using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using NUnit.Framework;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

[TestFixture]
public class TreeNodeListParentCoordinationTests
{
    [Test]
    public void Add_WithUpdateParentTrue_SetsChildParent()
    {
        // Arrange: Create parent and child nodes
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";

        // Act: Add child with updateParent = true
        parent.Children.Add(child, updateParent: true);

        // Assert: Child's parent should be set
        Assert.That(child.Parent, Is.EqualTo(parent));
        Assert.That(parent.Children.Contains(child), Is.True);
        Assert.That(child.Depth, Is.EqualTo(1));
    }

    [Test]
    public void Add_WithUpdateParentFalse_DoesNotSetChildParent()
    {
        // Arrange: Create parent and child nodes
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";

        // Act: Add child with updateParent = false
        parent.Children.Add(child, updateParent: false);

        // Assert: Child's parent should NOT be set
        Assert.That(child.Parent, Is.Null);
        Assert.That(parent.Children.Contains(child), Is.True);
        Assert.That(child.Depth, Is.EqualTo(0)); // Still at root level
    }

    [Test]
    public void Add_DefaultBehavior_UpdatesParent()
    {
        // Arrange: Create parent and child nodes
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";

        // Act: Add child using default Add method
        parent.Children.Add(child);

        // Assert: Child's parent should be set (default behavior is updateParent = true)
        Assert.That(child.Parent, Is.EqualTo(parent));
        Assert.That(parent.Children.Contains(child), Is.True);
        Assert.That(child.Depth, Is.EqualTo(1));
    }

    [Test]
    public void Remove_WithUpdateParentTrue_ClearsChildParent()
    {
        // Arrange: Create parent and child nodes with relationship
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";
        
        parent.Children.Add(child, updateParent: true);
        Assert.That(child.Parent, Is.EqualTo(parent));

        // Act: Remove child with updateParent = true
        parent.Children.Remove(child, updateParent: true);

        // Assert: Child's parent should be cleared
        Assert.That(child.Parent, Is.Null);
        Assert.That(parent.Children.Contains(child), Is.False);
        Assert.That(child.Depth, Is.EqualTo(0)); // Back to root level
    }

    [Test]
    public void Remove_WithUpdateParentFalse_DoesNotClearChildParent()
    {
        // Arrange: Create parent and child nodes with relationship
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";
        
        parent.Children.Add(child, updateParent: true);
        Assert.That(child.Parent, Is.EqualTo(parent));

        // Act: Remove child with updateParent = false
        parent.Children.Remove(child, updateParent: false);

        // Assert: Child's parent should NOT be cleared
        Assert.That(child.Parent, Is.EqualTo(parent));
        Assert.That(parent.Children.Contains(child), Is.False);
        Assert.That(child.Depth, Is.EqualTo(1)); // Still has parent relationship
    }

    [Test]
    public void Remove_DefaultBehavior_ClearsParent()
    {
        // Arrange: Create parent and child nodes with relationship
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";
        
        parent.Children.Add(child, updateParent: true);
        Assert.That(child.Parent, Is.EqualTo(parent));

        // Act: Remove child using default Remove method
        parent.Children.Remove(child);

        // Assert: Child's parent should be cleared (default behavior is updateParent = true)
        Assert.That(child.Parent, Is.Null);
        Assert.That(parent.Children.Contains(child), Is.False);
        Assert.That(child.Depth, Is.EqualTo(0));
    }

    [Test]
    public void Remove_NonExistentNode_DoesNotThrow()
    {
        // Arrange: Create parent and child nodes
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";

        // Act & Assert: Remove non-existent child should not throw
        Assert.DoesNotThrow(() => parent.Children.Remove(child, updateParent: true));
        Assert.DoesNotThrow(() => parent.Children.Remove(child, updateParent: false));
    }

    [Test]
    public void Remove_NullNode_ThrowsArgumentNullException()
    {
        // Arrange: Create parent node
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";

        // Act & Assert: Remove null child should throw
        Assert.Throws<ArgumentNullException>(() => parent.Children.Remove(null!, updateParent: true));
        Assert.Throws<ArgumentNullException>(() => parent.Children.Remove(null!, updateParent: false));
    }

    [Test]
    public void Add_WithUpdateParentTrue_TriggersParentChangeEvents()
    {
        // Arrange: Create parent and child nodes
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";

        // Track PropertyChanged events
        var propertyChangedEvents = new List<(object? sender, System.ComponentModel.PropertyChangedEventArgs e)>();
        child.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));

        // Act: Add child with updateParent = true
        parent.Children.Add(child, updateParent: true);

        // Assert: Parent property changed event should be raised
        var parentChangedEvents = propertyChangedEvents.Where(e => e.e.PropertyName == "Parent").ToList();
        Assert.That(parentChangedEvents.Count, Is.EqualTo(1));
        Assert.That(parentChangedEvents[0].sender, Is.EqualTo(child));
    }

    [Test]
    public void Remove_WithUpdateParentTrue_TriggersParentChangeEvents()
    {
        // Arrange: Create parent and child nodes with relationship
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";
        
        parent.Children.Add(child, updateParent: true);

        // Track PropertyChanged events
        var propertyChangedEvents = new List<(object? sender, System.ComponentModel.PropertyChangedEventArgs e)>();
        child.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));

        // Act: Remove child with updateParent = true
        parent.Children.Remove(child, updateParent: true);

        // Assert: Parent property changed event should be raised
        var parentChangedEvents = propertyChangedEvents.Where(e => e.e.PropertyName == "Parent").ToList();
        Assert.That(parentChangedEvents.Count, Is.EqualTo(1));
        Assert.That(parentChangedEvents[0].sender, Is.EqualTo(child));
    }

    [Test]
    public void Add_WithUpdateParentTrue_TriggersDescendantChangedEvents()
    {
        // Arrange: Create parent and child nodes
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";

        // Track DescendantChanged events
        var descendantChangedEvents = new List<(ITreeNode sender, NodeChangeType changeType, ITreeNode node)>();
        parent.DescendantChanged += (changeType, node) => descendantChangedEvents.Add((parent, changeType, node));

        // Act: Add child with updateParent = true
        parent.Children.Add(child, updateParent: true);

        // Assert: DescendantChanged event should be raised
        Assert.That(descendantChangedEvents.Count, Is.EqualTo(1));
        Assert.That(descendantChangedEvents[0].changeType, Is.EqualTo(NodeChangeType.NodeAdded));
        Assert.That(descendantChangedEvents[0].node, Is.EqualTo(child));
    }

    [Test]
    public void Remove_WithUpdateParentTrue_TriggersDescendantChangedEvents()
    {
        // Arrange: Create parent and child nodes with relationship
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";
        
        parent.Children.Add(child, updateParent: true);

        // Track DescendantChanged events
        var descendantChangedEvents = new List<(ITreeNode sender, NodeChangeType changeType, ITreeNode node)>();
        parent.DescendantChanged += (changeType, node) => descendantChangedEvents.Add((parent, changeType, node));

        // Act: Remove child with updateParent = true
        parent.Children.Remove(child, updateParent: true);

        // Assert: DescendantChanged event should be raised
        Assert.That(descendantChangedEvents.Count, Is.EqualTo(1));
        Assert.That(descendantChangedEvents[0].changeType, Is.EqualTo(NodeChangeType.NodeRemoved));
        Assert.That(descendantChangedEvents[0].node, Is.EqualTo(child));
    }

    [Test]
    public void Add_WithUpdateParentFalse_DoesNotTriggerEvents()
    {
        // Arrange: Create parent and child nodes
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";

        // Track events
        var propertyChangedEvents = new List<(object? sender, System.ComponentModel.PropertyChangedEventArgs e)>();
        var descendantChangedEvents = new List<(ITreeNode sender, NodeChangeType changeType, ITreeNode node)>();
        
        child.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));
        parent.DescendantChanged += (changeType, node) => descendantChangedEvents.Add((parent, changeType, node));

        // Act: Add child with updateParent = false
        parent.Children.Add(child, updateParent: false);

        // Assert: No events should be triggered
        Assert.That(propertyChangedEvents.Count, Is.EqualTo(0));
        Assert.That(descendantChangedEvents.Count, Is.EqualTo(0));
    }

    [Test]
    public void Remove_WithUpdateParentFalse_DoesNotTriggerParentEvents()
    {
        // Arrange: Create parent and child nodes with relationship
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";
        
        parent.Children.Add(child, updateParent: true);

        // Track PropertyChanged events
        var propertyChangedEvents = new List<(object? sender, System.ComponentModel.PropertyChangedEventArgs e)>();
        child.PropertyChanged += (sender, e) => propertyChangedEvents.Add((sender, e));

        // Act: Remove child with updateParent = false
        parent.Children.Remove(child, updateParent: false);

        // Assert: Parent property changed event should NOT be raised
        var parentChangedEvents = propertyChangedEvents.Where(e => e.e.PropertyName == "Parent").ToList();
        Assert.That(parentChangedEvents.Count, Is.EqualTo(0));
    }

    [Test]
    public void Add_WithUpdateParentTrue_UpdatesDepthCorrectly()
    {
        // Arrange: Create a tree structure
        var root = new TreeNode<Space>();
        root.Payload.Name = "Root";
        
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";

        // Act: Build tree with updateParent = true
        root.Children.Add(parent, updateParent: true);
        parent.Children.Add(child, updateParent: true);

        // Assert: Depths should be correct
        Assert.That(root.Depth, Is.EqualTo(0));
        Assert.That(parent.Depth, Is.EqualTo(1));
        Assert.That(child.Depth, Is.EqualTo(2));
    }

    [Test]
    public void Remove_WithUpdateParentTrue_UpdatesDepthCorrectly()
    {
        // Arrange: Create a tree structure
        var root = new TreeNode<Space>();
        root.Payload.Name = "Root";
        
        var parent = new TreeNode<Space>();
        parent.Payload.Name = "Parent";
        
        var child = new TreeNode<Space>();
        child.Payload.Name = "Child";

        root.Children.Add(parent, updateParent: true);
        parent.Children.Add(child, updateParent: true);

        // Act: Remove parent from root
        root.Children.Remove(parent, updateParent: true);

        // Assert: Depths should be updated correctly
        Assert.That(root.Depth, Is.EqualTo(0));
        Assert.That(parent.Depth, Is.EqualTo(0)); // Now at root level
        Assert.That(child.Depth, Is.EqualTo(1)); // Still under parent
    }
}

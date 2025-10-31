using System;
using System.Collections.Generic;
using System.Linq;
using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using NUnit.Framework;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

[TestFixture]
public class DisposalSemanticsTests
{
    // Test payload that implements IDisposable
    public class DisposablePayload : IDisposable
    {
        public string Name { get; set; } = string.Empty;
        public bool IsDisposed { get; private set; }
        public List<string> DisposalOrder { get; set; } = new();

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            DisposalOrder.Add($"Disposed: {Name}");
        }
    }

    // Test node that implements IDisposable
    public class DisposableTreeNode : TreeNode<DisposablePayload>
    {
        public List<string> DisposalOrder { get; set; } = new();

        public DisposableTreeNode(string name)
        {
            Payload.Name = name;
        }

        public override void Dispose()
        {
            DisposalOrder.Add($"NodeDisposing: {Payload.Name}");
            base.Dispose();
            DisposalOrder.Add($"NodeDisposed: {Payload.Name}");
        }

        // Expose protected properties for testing
        public new UpDownTraversalType DisposeTraversal
        {
            get => base.DisposeTraversal;
            set => base.DisposeTraversal = value;
        }

        public new bool IsDisposed => base.IsDisposed;
    }

    // Test helper for non-disposable nodes
    public class TestTreeNode : TreeNode<Space>
    {
        public new bool IsDisposed => base.IsDisposed;
    }

    [Test]
    public void Dispose_BottomUpTraversal_DisposesChildrenBeforeParent()
    {
        // Arrange: Create a tree with disposable payloads
        var root = new DisposableTreeNode("Root");
        var child1 = new DisposableTreeNode("Child1");
        var child2 = new DisposableTreeNode("Child2");
        var grandchild1 = new DisposableTreeNode("Grandchild1");
        var grandchild2 = new DisposableTreeNode("Grandchild2");

        child1.Children.Add(grandchild1);
        child1.Children.Add(grandchild2);
        root.Children.Add(child1);
        root.Children.Add(child2);

        // Set BottomUp traversal (default)
        root.DisposeTraversal = UpDownTraversalType.BottomUp;

        // Act: Dispose the root
        root.Dispose();

        // Assert: Children should be disposed before parent
        Assert.That(root.IsDisposed, Is.True);
        Assert.That(child1.IsDisposed, Is.True);
        Assert.That(child2.IsDisposed, Is.True);
        Assert.That(grandchild1.IsDisposed, Is.True);
        Assert.That(grandchild2.IsDisposed, Is.True);

        // Verify disposal order for payloads
        var allDisposalOrder = new List<string>();
        allDisposalOrder.AddRange(root.Payload.DisposalOrder);
        allDisposalOrder.AddRange(child1.Payload.DisposalOrder);
        allDisposalOrder.AddRange(child2.Payload.DisposalOrder);
        allDisposalOrder.AddRange(grandchild1.Payload.DisposalOrder);
        allDisposalOrder.AddRange(grandchild2.Payload.DisposalOrder);

        // In BottomUp, grandchildren should be disposed before children, children before root
        var grandchildDisposals = allDisposalOrder.Where(s => s.Contains("Grandchild")).ToList();
        var childDisposals = allDisposalOrder.Where(s => s.Contains("Child") && !s.Contains("Grandchild")).ToList();
        var rootDisposals = allDisposalOrder.Where(s => s.Contains("Root")).ToList();

        Assert.That(grandchildDisposals.Count, Is.EqualTo(2));
        Assert.That(childDisposals.Count, Is.EqualTo(2));
        Assert.That(rootDisposals.Count, Is.EqualTo(1));
    }

    [Test]
    public void Dispose_TopDownTraversal_DisposesParentBeforeChildren()
    {
        // Arrange: Create a tree with disposable payloads
        var root = new DisposableTreeNode("Root");
        var child1 = new DisposableTreeNode("Child1");
        var child2 = new DisposableTreeNode("Child2");
        var grandchild1 = new DisposableTreeNode("Grandchild1");
        var grandchild2 = new DisposableTreeNode("Grandchild2");

        child1.Children.Add(grandchild1);
        child1.Children.Add(grandchild2);
        root.Children.Add(child1);
        root.Children.Add(child2);

        // Set TopDown traversal
        root.DisposeTraversal = UpDownTraversalType.TopDown;

        // Act: Dispose the root
        root.Dispose();

        // Assert: All nodes should be disposed
        Assert.That(root.IsDisposed, Is.True);
        Assert.That(child1.IsDisposed, Is.True);
        Assert.That(child2.IsDisposed, Is.True);
        Assert.That(grandchild1.IsDisposed, Is.True);
        Assert.That(grandchild2.IsDisposed, Is.True);

        // Verify disposal order for payloads
        var allDisposalOrder = new List<string>();
        allDisposalOrder.AddRange(root.Payload.DisposalOrder);
        allDisposalOrder.AddRange(child1.Payload.DisposalOrder);
        allDisposalOrder.AddRange(child2.Payload.DisposalOrder);
        allDisposalOrder.AddRange(grandchild1.Payload.DisposalOrder);
        allDisposalOrder.AddRange(grandchild2.Payload.DisposalOrder);

        // In TopDown, root should be disposed before children, children before grandchildren
        var grandchildDisposals = allDisposalOrder.Where(s => s.Contains("Grandchild")).ToList();
        var childDisposals = allDisposalOrder.Where(s => s.Contains("Child") && !s.Contains("Grandchild")).ToList();
        var rootDisposals = allDisposalOrder.Where(s => s.Contains("Root")).ToList();

        Assert.That(grandchildDisposals.Count, Is.EqualTo(2));
        Assert.That(childDisposals.Count, Is.EqualTo(2));
        Assert.That(rootDisposals.Count, Is.EqualTo(1));
    }

    [Test]
    public void Dispose_NonDisposablePayload_OnlyDisposesRoot()
    {
        // Arrange: Create a tree with non-disposable payloads
        var root = new TestTreeNode();
        root.Payload.Name = "Root";
        
        var child1 = new TestTreeNode();
        child1.Payload.Name = "Child1";
        
        var child2 = new TestTreeNode();
        child2.Payload.Name = "Child2";

        root.Children.Add(child1);
        root.Children.Add(child2);

        // Act: Dispose the root
        root.Dispose();

        // Assert: Only the root should be disposed (children are not disposed when payload is not IDisposable)
        Assert.That(root.IsDisposed, Is.True);
        Assert.That(child1.IsDisposed, Is.False);
        Assert.That(child2.IsDisposed, Is.False);
    }

    [Test]
    public void Dispose_TriggersDisposingEvent()
    {
        // Arrange: Create a node
        var node = new TestTreeNode();
        node.Payload.Name = "Test";

        var disposingEvents = new List<object?>();
        node.Disposing += (sender, e) => disposingEvents.Add(sender);

        // Act: Dispose the node
        node.Dispose();

        // Assert: Disposing event should be triggered
        Assert.That(disposingEvents.Count, Is.EqualTo(1));
        Assert.That(disposingEvents[0], Is.EqualTo(node));
    }

    [Test]
    public void Dispose_MultipleCalls_ThrowsOnSecondCall()
    {
        // Arrange: Create a node with disposable payload
        var payload = new DisposablePayload { Name = "Test" };
        var node = new TreeNode<DisposablePayload>();
        node.PayloadObject = payload;

        // Act: Dispose first time
        node.Dispose();

        // Assert: First disposal should work
        Assert.That(payload.IsDisposed, Is.True);
        Assert.That(payload.DisposalOrder.Count, Is.EqualTo(1));

        // Act & Assert: Second disposal should throw
        Assert.Throws<ObjectDisposedException>(() => node.Dispose());
    }

    [Test]
    public void Dispose_AfterDisposal_ThrowsObjectDisposedException()
    {
        // Arrange: Create a node
        var node = new TestTreeNode();
        node.Payload.Name = "Test";

        // Act: Dispose the node
        node.Dispose();

        // Assert: Operations after disposal should throw
        Assert.Throws<ObjectDisposedException>(() => node.CheckDisposed());
        Assert.Throws<ObjectDisposedException>(() => node.Dispose());
    }

    [Test]
    public void Dispose_BottomUpTraversal_RespectsHierarchyOrder()
    {
        // Arrange: Create a deep tree structure
        var root = new DisposableTreeNode("Root");
        var level1 = new DisposableTreeNode("Level1");
        var level2a = new DisposableTreeNode("Level2A");
        var level2b = new DisposableTreeNode("Level2B");
        var level3 = new DisposableTreeNode("Level3");

        level2a.Children.Add(level3);
        level1.Children.Add(level2a);
        level1.Children.Add(level2b);
        root.Children.Add(level1);

        root.DisposeTraversal = UpDownTraversalType.BottomUp;

        // Act: Dispose the root
        root.Dispose();

        // Assert: All nodes should be disposed
        Assert.That(root.IsDisposed, Is.True);
        Assert.That(level1.IsDisposed, Is.True);
        Assert.That(level2a.IsDisposed, Is.True);
        Assert.That(level2b.IsDisposed, Is.True);
        Assert.That(level3.IsDisposed, Is.True);
    }

    [Test]
    public void Dispose_TopDownTraversal_RespectsHierarchyOrder()
    {
        // Arrange: Create a deep tree structure
        var root = new DisposableTreeNode("Root");
        var level1 = new DisposableTreeNode("Level1");
        var level2a = new DisposableTreeNode("Level2A");
        var level2b = new DisposableTreeNode("Level2B");
        var level3 = new DisposableTreeNode("Level3");

        level2a.Children.Add(level3);
        level1.Children.Add(level2a);
        level1.Children.Add(level2b);
        root.Children.Add(level1);

        root.DisposeTraversal = UpDownTraversalType.TopDown;

        // Act: Dispose the root
        root.Dispose();

        // Assert: All nodes should be disposed
        Assert.That(root.IsDisposed, Is.True);
        Assert.That(level1.IsDisposed, Is.True);
        Assert.That(level2a.IsDisposed, Is.True);
        Assert.That(level2b.IsDisposed, Is.True);
        Assert.That(level3.IsDisposed, Is.True);
    }

    [Test]
    public void Dispose_MixedDisposableAndNonDisposable_HandlesCorrectly()
    {
        // Arrange: Create a tree with mixed payload types
        var root = new TestTreeNode(); // Non-disposable
        root.Payload.Name = "Root";
        
        var child1 = new DisposableTreeNode("Child1"); // Disposable
        var child2 = new TestTreeNode(); // Non-disposable
        child2.Payload.Name = "Child2";

        root.Children.Add(child1);
        root.Children.Add(child2);

        // Act: Dispose the root
        root.Dispose();

        // Assert: Only root should be disposed (children not disposed when root payload is not IDisposable)
        Assert.That(root.IsDisposed, Is.True);
        Assert.That(child1.IsDisposed, Is.False);
        Assert.That(child2.IsDisposed, Is.False);

        // Child1's payload should not be disposed yet
        Assert.That(child1.Payload.IsDisposed, Is.False);
    }

    [Test]
    public void DisposeTraversal_CanBeSetPerNode()
    {
        // Arrange: Create nodes with different traversal types
        var root = new DisposableTreeNode("Root");
        var child1 = new DisposableTreeNode("Child1");
        var child2 = new DisposableTreeNode("Child2");

        root.DisposeTraversal = UpDownTraversalType.BottomUp;
        child1.DisposeTraversal = UpDownTraversalType.TopDown;
        child2.DisposeTraversal = UpDownTraversalType.BottomUp;

        root.Children.Add(child1);
        root.Children.Add(child2);

        // Act: Dispose the root
        root.Dispose();

        // Assert: All nodes should be disposed
        Assert.That(root.IsDisposed, Is.True);
        Assert.That(child1.IsDisposed, Is.True);
        Assert.That(child2.IsDisposed, Is.True);
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using NUnit.Framework;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

// A simple payload for testing purposes.
public class DummyPayload
{
    public string Name { get; set; } = "Default";
}

[TestFixture]
public class TreeViewTests
{
    // Helper method to build a sample tree:
    //         Root
    //         /  \
    //    Child1  Child2
    //       |
    //  Grandchild1
    private TreeNode<DummyPayload> CreateSampleTree()
    {
        var root = new TreeNode<DummyPayload>();
        root.Payload.Name = "Root";

        var child1 = new TreeNode<DummyPayload>();
        child1.Payload.Name = "Child1";

        var child2 = new TreeNode<DummyPayload>();
        child2.Payload.Name = "Child2";

        var grandchild1 = new TreeNode<DummyPayload>();
        grandchild1.Payload.Name = "Grandchild1";

        child1.Children.Add(grandchild1);
        root.Children.Add(child1);
        root.Children.Add(child2);

        return root;
    }

    [Test]
    public void TestFullTreeViewProjection_WhenAllExpanded()
    {
        var root = CreateSampleTree();
        var treeView = new TreeView(root);

        // Expected flattened view (excluding the root): Child1, Grandchild1, Child2.
        Assert.That(3, Is.EqualTo(treeView.ProjectedCollection.Count));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[0]).Payload.Name, Is.EqualTo("Child1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[1]).Payload.Name, Is.EqualTo("Grandchild1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[2]).Payload.Name, Is.EqualTo("Child2"));
    }

    [Test]
    public void TestTreeViewProjection_WhenCollapsed()
    {
        var root = CreateSampleTree();
        var treeView = new TreeView(root);

        // Collapse Child1 in the view.
        var child1 = root.Children[0];
        treeView.SetIsExpanded(child1, false);
        treeView.UpdateProjectedCollection();

        // Expected view: Child1 and Child2 (Grandchild1 is hidden).
        Assert.That(2, Is.EqualTo(treeView.ProjectedCollection.Count));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[0]).Payload.Name, Is.EqualTo("Child1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[1]).Payload.Name, Is.EqualTo("Child2"));
    }

    [Test]
    public void TestMultipleTreeViews_DifferentProjections()
    {
        var root = CreateSampleTree();

        // Create two different views for the same tree.
        var view1 = new TreeView(root);
        var view2 = new TreeView(root);

        // In view1, collapse Child1.
        var child1 = root.Children[0];
        view1.SetIsExpanded(child1, false);
        view1.UpdateProjectedCollection();

        // view1 expected: Child1, Child2.
        Assert.That(2, Is.EqualTo(view1.ProjectedCollection.Count));
        Assert.That("Child1", Is.EqualTo(((TreeNode<DummyPayload>)view1.ProjectedCollection[0]).Payload.Name));
        Assert.That("Child2", Is.EqualTo(((TreeNode<DummyPayload>)view1.ProjectedCollection[1]).Payload.Name));

        // view2 remains fully expanded: Child1, Grandchild1, Child2.
        view2.UpdateProjectedCollection();
        Assert.That(3, Is.EqualTo(view2.ProjectedCollection.Count));
        Assert.That("Child1", Is.EqualTo(((TreeNode<DummyPayload>)view2.ProjectedCollection[0]).Payload.Name));
        Assert.That("Grandchild1", Is.EqualTo(((TreeNode<DummyPayload>)view2.ProjectedCollection[1]).Payload.Name));
        Assert.That("Child2", Is.EqualTo(((TreeNode<DummyPayload>)view2.ProjectedCollection[2]).Payload.Name));
    }

    [Test]
    public void TestTreeViewUpdatesOnNodeExpansionChange()
    {
        // Build a simple tree: Root -> Child1 -> Grandchild1.
        var root = new TreeNode<DummyPayload>();
        root.Payload.Name = "Root";
        var child1 = new TreeNode<DummyPayload>();
        child1.Payload.Name = "Child1";
        var grandchild1 = new TreeNode<DummyPayload>();
        grandchild1.Payload.Name = "Grandchild1";
        child1.Children.Add(grandchild1);
        root.Children.Add(child1);

        var treeView = new TreeView(root);
        treeView.UpdateProjectedCollection();

        // Initially, fully expanded: Projected view should be [Child1, Grandchild1].
        Assert.That(2, Is.EqualTo(treeView.ProjectedCollection.Count));

        // Collapse Child1 by changing its IsExpanded property.
        treeView.SetIsExpanded(child1, false);
        // The TreeView listens to property changes and updates automatically.
        Assert.That(1, Is.EqualTo(treeView.ProjectedCollection.Count));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[0]).Payload.Name, Is.EqualTo("Child1"));

        // Expand Child1 again.
        treeView.SetIsExpanded(child1, true);
        Assert.That(2, Is.EqualTo(treeView.ProjectedCollection.Count));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[0]).Payload.Name, Is.EqualTo("Child1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[1]).Payload.Name, Is.EqualTo("Grandchild1"));
    }

    // Constructs a simple tree:
    //          Root
    //         /    \
    //    Child1    Child2
    //       |
    //  Grandchild1
    private TreeNode<DummyPayload> CreateTestTree()
    {
        var root = new TreeNode<DummyPayload>();
        root.Payload.Name = "Root";

        var child1 = new TreeNode<DummyPayload>();
        child1.Payload.Name = "Child1";

        var child2 = new TreeNode<DummyPayload>();
        child2.Payload.Name = "Child2";

        var grandchild1 = new TreeNode<DummyPayload>();
        grandchild1.Payload.Name = "Grandchild1";

        child1.Children.Add(grandchild1);
        root.Children.Add(child1);
        root.Children.Add(child2);

        return root;
    }

    [Test]
    public void IncrementalUpdate_NoChange_Test()
    {
        var root = CreateTestTree();
        var treeView = new TreeView(root);
        treeView.UpdateProjectedCollection();

        // Expecting: [Child1, Grandchild1, Child2] (root is not displayed).
        Assert.That(3, Is.EqualTo(treeView.ProjectedCollection.Count));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[0]).Payload.Name, Is.EqualTo("Child1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[1]).Payload.Name, Is.EqualTo("Grandchild1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[2]).Payload.Name, Is.EqualTo("Child2"));
    }

    [Test]
    public void IncrementalUpdate_AddNode_Test()
    {
        var root = CreateTestTree();
        var treeView = new TreeView(root);
        treeView.UpdateProjectedCollection();

        // Add a new node under Child1.
        var child1 = root.Children[0];
        var newGrandchild = new TreeNode<DummyPayload>();
        newGrandchild.Payload.Name = "Grandchild2";
        child1.Children.Add(newGrandchild);

        treeView.UpdateProjectedCollection();

        // Expecting: [Child1, Grandchild1, Grandchild2, Child2].
        Assert.That(4, Is.EqualTo(treeView.ProjectedCollection.Count));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[0]).Payload.Name, Is.EqualTo("Child1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[1]).Payload.Name, Is.EqualTo("Grandchild1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[2]).Payload.Name, Is.EqualTo("Grandchild2"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[3]).Payload.Name, Is.EqualTo("Child2"));
    }

    [Test]
    public void IncrementalUpdate_RemoveNode_Test()
    {
        var root = CreateTestTree();
        var treeView = new TreeView(root);
        treeView.UpdateProjectedCollection();

        // Remove Child1 (and its subtree).
        var child1 = root.Children[0];
        root.Children.Remove(child1);

        treeView.UpdateProjectedCollection();

        // Expecting: [Child2] only.
        Assert.That(1, Is.EqualTo(treeView.ProjectedCollection.Count));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[0]).Payload.Name, Is.EqualTo("Child2"));
    }

    [Test]
    public void IncrementalUpdate_CollapseNode_Test()
    {
        var root = CreateTestTree();
        var treeView = new TreeView(root);
        treeView.UpdateProjectedCollection();

        // Collapse Child1 in the view.
        var child1 = root.Children[0];
        treeView.SetIsExpanded(child1, false);
        treeView.UpdateProjectedCollection();

        // Expected: [Child1, Child2] (Grandchild1 is hidden).
        Assert.That(2, Is.EqualTo(treeView.ProjectedCollection.Count));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[0]).Payload.Name, Is.EqualTo("Child1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[1]).Payload.Name, Is.EqualTo("Child2"));
    }

    [Test]
    public void IncrementalUpdate_ExpandNode_Test()
    {
        var root = CreateTestTree();
        var treeView = new TreeView(root);

        // Initially collapse Child1.
        var child1 = root.Children[0];
        treeView.SetIsExpanded(child1, false);
        treeView.UpdateProjectedCollection();
        Assert.That(2, Is.EqualTo(treeView.ProjectedCollection.Count));

        // Now expand Child1.
        treeView.SetIsExpanded(child1, true);
        treeView.UpdateProjectedCollection();

        // Expected: [Child1, Grandchild1, Child2].
        Assert.That(3, Is.EqualTo(treeView.ProjectedCollection.Count));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[0]).Payload.Name, Is.EqualTo("Child1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[1]).Payload.Name, Is.EqualTo("Grandchild1"));
        Assert.That(((TreeNode<DummyPayload>)treeView.ProjectedCollection[2]).Payload.Name, Is.EqualTo("Child2"));
    }

    [Test]
    public void Tree_IsExpanded_IsVisible()
    {
        // Create the test tree.
        var tree = TestHelpers.CreateTestSpaceTree();
        // Create a TreeView for the tree with default expanded state = true.
        var treeView = new TreeView(tree);

        // Get specific nodes.
        var shed = tree.Descendants.OfType<TreeNode<Space>>().First(n => n.Payload.Name == "Shed");
        var house = tree.Descendants.OfType<TreeNode<Space>>().First(n => n.Payload.Name == "House");
        var kitchen = tree.Descendants.OfType<TreeNode<Space>>().First(n => n.Payload.Name == "Kitchen");
        var bathroom = tree.Descendants.OfType<TreeNode<Space>>().First(n => n.Payload.Name == "Bathroom");
        var basement = tree.Descendants.OfType<TreeNode<Space>>().First(n => n.Payload.Name == "Basement");
        var basementBedroom = tree.Descendants.OfType<TreeNode<Space>>().First(n => n.Payload.Name == "Bedroom");

        // Initially, all nodes are expanded so the projected collection (excluding the root) should include 6 nodes:
        // [House, Kitchen, Bathroom, Shed, Basement, Bedroom]
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(6));

        // Collapse the "Basement" node.
        treeView.SetIsExpanded(basement, false);
        // "Bedroom" becomes hidden. Visible nodes: [House, Kitchen, Bathroom, Shed, Basement]
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(5));

        // Collapse the "House" node.
        treeView.SetIsExpanded(house, false);
        // Even when collapsed, a node remains visible. Now, House is visible but its children (Kitchen, Bathroom) are hidden.
        // So visible nodes become: [House, Shed, Basement] — count is 3.
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(3));

        // Collapse the root ("Property") node.
        treeView.SetIsExpanded(tree, false);
        // With the root collapsed, nothing in the subtree is visible.
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(0));

        // Re-expand the root, "House", and "Basement".
        treeView.SetIsExpanded(tree, true);
        treeView.SetIsExpanded(house, true);
        treeView.SetIsExpanded(basement, true);
        // All nodes become visible again.
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(6));

        // Collapse leaf nodes.
        treeView.SetIsExpanded(shed, false);
        treeView.SetIsExpanded(kitchen, false);
        treeView.SetIsExpanded(bathroom, false);
        treeView.SetIsExpanded(basementBedroom, false);
        // Since leaf nodes (nodes with no children) are still visible even if "collapsed," the count remains 6.
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(6));
    }

    [Test]
    public void Tree_ProjectedCollection_Add_Remove()
    {
        // Create a Property node as the root.
        var property = new TreeNode<Space>(new Space { Name = "Property", SquareFeet = 20000 });
        var treeView = new TreeView(property);

        // Initially, the root has no children so the projected collection is empty.
        Assert.That(property.Depth, Is.EqualTo(0));
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(0));

        // Add House and Shed as children of Property.
        var house = property.Children.Add(new TreeNode<Space>(new Space { Name = "House", SquareFeet = 2500 }));
        var shed = property.Children.Add(new TreeNode<Space>(new Space { Name = "Shed", SquareFeet = 240 }));

        Assert.That(house.Depth, Is.EqualTo(1));
        Assert.That(shed.Depth, Is.EqualTo(1));
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(2));
        Assert.That(TestHelpers.SpaceNames(treeView), Is.EqualTo(new List<string> { "House", "Shed" }));

        // Add Kitchen as a child of House.
        var kitchen = house.Children.Add(new TreeNode<Space>(new Space { Name = "Kitchen", SquareFeet = 600 }));
        Assert.That(kitchen.Depth, Is.EqualTo(2));
        Assert.That(TestHelpers.SpaceNames(treeView), Is.EqualTo(new List<string> { "House", "Kitchen", "Shed" }));

        // Add Bathroom as a child of House.
        var bathroom = house.Children.Add(new TreeNode<Space>(new Space { Name = "Bathroom", SquareFeet = 300 }));
        Assert.That(bathroom.Depth, Is.EqualTo(2));
        Assert.That(TestHelpers.SpaceNames(treeView), Is.EqualTo(new List<string> { "House", "Kitchen", "Bathroom", "Shed" }));

        // Remove Kitchen from House (make it a root-level node).
        kitchen.SetParent(null);
        Assert.That(kitchen.Depth, Is.EqualTo(0));
        // The projected collection of the Property tree should now exclude Kitchen.
        Assert.That(TestHelpers.SpaceNames(treeView), Is.EqualTo(new List<string> { "House", "Bathroom", "Shed" }));

        // Remove House from Property.
        house.SetParent(null);
        Assert.That(house.Depth, Is.EqualTo(0));
        // Now only Shed remains as a child of Property.
        Assert.That(TestHelpers.SpaceNames(treeView), Is.EqualTo(new List<string> { "Shed" }));
    }
}
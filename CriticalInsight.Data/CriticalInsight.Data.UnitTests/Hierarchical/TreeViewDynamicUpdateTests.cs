using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

[TestFixture]
public class TreeViewDynamicUpdateTests
{
    [Test]
    public void AddNode_WhenParentIsVisible_UpdatesProjectedCollection()
    {
        // Create a simple tree with root and two children.
        var property = new TreeNode<Space>(new Space { Name = "Property", SquareFeet = 20000 });
        var house = property.Children.Add(new TreeNode<Space>(new Space { Name = "House", SquareFeet = 2500 }));
        var shed = property.Children.Add(new TreeNode<Space>(new Space { Name = "Shed", SquareFeet = 240 }));

        // Create a TreeView with default expanded state true.
        var treeView = new TreeView(property);

        // Initially, projected collection (excluding root) should contain: House, Shed.
        Assert.That(TestHelpers.SpaceNames(treeView), Is.EqualTo(new List<string> { "House", "Shed" }));

        // Add a new child to the property node.
        var basement = property.Children.Add(new TreeNode<Space>(new Space { Name = "Basement", SquareFeet = 1500 }));

        // Since the property node is expanded, the new node should appear.
        Assert.That(TestHelpers.SpaceNames(treeView), Is.EqualTo(new List<string> { "House", "Shed", "Basement" }));
    }

    [Test]
    public void AddNode_WhenParentIsNotVisible_DoesNotUpdateProjectedCollection()
    {
        // Create a simple tree.
        var property = new TreeNode<Space>(new Space { Name = "Property", SquareFeet = 20000 });
        var house = property.Children.Add(new TreeNode<Space>(new Space { Name = "House", SquareFeet = 2500 }));
        var shed = property.Children.Add(new TreeNode<Space>(new Space { Name = "Shed", SquareFeet = 240 }));

        var treeView = new TreeView(property);

        // Collapse the root so that none of its children are visible.
        treeView.SetIsExpanded(property, false);
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(0));

        // Add a new node under the property.
        var basement = property.Children.Add(new TreeNode<Space>(new Space { Name = "Basement", SquareFeet = 1500 }));

        // Since the root is collapsed, the newly added node should not appear.
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveNode_WhenVisible_UpdatesProjectedCollection()
    {
        // Create a tree with two children.
        var property = new TreeNode<Space>(new Space { Name = "Property", SquareFeet = 20000 });
        var house = property.Children.Add(new TreeNode<Space>(new Space { Name = "House", SquareFeet = 2500 }));
        var shed = property.Children.Add(new TreeNode<Space>(new Space { Name = "Shed", SquareFeet = 240 }));

        var treeView = new TreeView(property);

        // Initially, projected collection should be: House, Shed.
        Assert.That(TestHelpers.SpaceNames(treeView), Is.EqualTo(new List<string> { "House", "Shed" }));

        // Remove the House node.
        property.Children.Remove(house);

        // Now projected collection should update to contain only Shed.
        Assert.That(TestHelpers.SpaceNames(treeView), Is.EqualTo(new List<string> { "Shed" }));
    }

    [Test]
    public void RemoveNode_WhenNotVisible_UpdatesProjectedCollection()
    {
        // Create a tree with two children.
        var property = new TreeNode<Space>(new Space { Name = "Property", SquareFeet = 20000 });
        var house = property.Children.Add(new TreeNode<Space>(new Space { Name = "House", SquareFeet = 2500 }));
        var shed = property.Children.Add(new TreeNode<Space>(new Space { Name = "Shed", SquareFeet = 240 }));

        var treeView = new TreeView(property);

        // Collapse the root so that its children are not visible.
        treeView.SetIsExpanded(property, false);
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(0));

        // Remove the House node.
        property.Children.Remove(house);

        // Projected collection remains empty.
        Assert.That(treeView.ProjectedCollection.Count, Is.EqualTo(0));
    }
}
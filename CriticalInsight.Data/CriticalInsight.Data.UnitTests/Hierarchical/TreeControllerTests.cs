using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

[TestFixture]
public class TreeControllerTests
{
    [Test]
    public void FindNode_ReturnsCorrectNode()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));
        // Find "House" node
        var expected = treeRoot.Children
            .OfType<TreeNode<Space>>()
            .First(n => n.PayloadObject is Space s && s.Name == "House");

        // Act
        var found = controller.FindNode(expected.NodeId);

        // Assert
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.NodeId, Is.EqualTo(expected.NodeId));
    }

    [Test]
    public void ExpandAndCollapseNode_UpdatesView()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));
        // Pick a child node
        var node = treeRoot.Children.OfType<TreeNode<Space>>().First();
        // Initially, assume default expanded state is true.
        Assert.That(controller.TreeView.GetIsExpanded(node), Is.True);

        // Act: Collapse the node.
        controller.CollapseNode(node.NodeId);
        // Assert
        Assert.That(controller.TreeView.GetIsExpanded(node), Is.False);

        // Act: Expand the node.
        controller.ExpandNode(node.NodeId);
        // Assert
        Assert.That(controller.TreeView.GetIsExpanded(node), Is.True);
    }

    [Test]
    public void ToggleNode_UpdatesView()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));
        var node = treeRoot.Children.OfType<TreeNode<Space>>().First();
        bool initialState = controller.TreeView.GetIsExpanded(node);

        // Act: Toggle once.
        controller.ToggleNode(node.NodeId);
        // Assert: State should be the opposite.
        Assert.That(controller.TreeView.GetIsExpanded(node), Is.EqualTo(!initialState));

        // Act: Toggle again.
        controller.ToggleNode(node.NodeId);
        // Assert: State should be back to initial.
        Assert.That(controller.TreeView.GetIsExpanded(node), Is.EqualTo(initialState));
    }

    [Test]
    public void ExpandAll_CollapseAll_Work()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));

        // Act: Collapse all.
        controller.CollapseAll();
        // Assert: Every node in the tree should be collapsed.
        void CheckCollapsed(ITreeNode node)
        {
            Assert.That(controller.TreeView.GetIsExpanded(node), Is.False);
            foreach (var child in node.Children)
                CheckCollapsed(child);
        }
        CheckCollapsed(treeRoot);

        // Act: Expand all.
        controller.ExpandAll();
        // Assert: Every node in the tree should be expanded.
        void CheckExpanded(ITreeNode node)
        {
            Assert.That(controller.TreeView.GetIsExpanded(node), Is.True);
            foreach (var child in node.Children)
                CheckExpanded(child);
        }
        CheckExpanded(treeRoot);
    }

    [Test]
    public void SetIncludedAndExcludedProperties_UpdateTreeView()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var treeView = new TreeView(treeRoot);
        var controller = new TreeController<TreeNode<Space>>(treeRoot, treeView);

        var included = new List<string> { "NodeId", "PayloadType", "IsExpanded", "ChildrenCount" };
        var excluded = new List<string> { "Payload.SquareFeet" };

        // Act
        controller.SetIncludedProperties(included);
        controller.SetExcludedProperties(excluded);

        // Assert
        Assert.That(included, Is.EqualTo(controller.TreeView.IncludedProperties));
        Assert.That(excluded, Is.EqualTo(controller.TreeView.ExcludedProperties));
    }

    [Test]
    public void AddChild_AddsNodeToParent()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));
        var parent = treeRoot.Children.OfType<TreeNode<Space>>().First();
        int initialCount = parent.Children.Count;

        // Create a new child node.
        var newChild = new TreeNode<Space>(new Space { Name = "New Room", SquareFeet = 500 });

        // Act
        controller.AddChild(parent.NodeId, newChild);

        // Assert
        Assert.That(parent.Children.Count, Is.EqualTo(initialCount + 1));
        Assert.That(newChild.Parent, Is.EqualTo(parent));
    }

    [Test]
    public void UpdateNodeProperty_ChangesPropertyValue()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));
        // Choose "House" node.
        var house = treeRoot.Children
            .OfType<TreeNode<Space>>()
            .First(n => ((Space)n.PayloadObject).Name == "House");
        string originalName = ((Space)house.PayloadObject).Name;
        string newName = originalName + " Updated";

        // Act
        controller.UpdateNodePayloadProperty(house.NodeId, "Name", newName);

        // Assert: Reflect update on the payload.
        var updatedName = ((Space)house.PayloadObject).Name;
        Assert.That(updatedName, Is.EqualTo(newName));
    }

    [Test]
    public void RemoveNode_RemovesNodeFromParent()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));
        // Choose a child node.
        var house = treeRoot.Children
            .OfType<TreeNode<Space>>()
            .First(n => ((Space)n.PayloadObject).Name == "House");
        int initialCount = treeRoot.Children.Count;

        // Act
        controller.RemoveNode(house.NodeId);

        // Assert: The parent's children should no longer include the removed node.
        Assert.That(treeRoot.Children.Count, Is.EqualTo(initialCount - 1));
        Assert.That(house.Parent, Is.Null);
    }

    [Test]
    public void MoveNode_ReparentsNodeCorrectly()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));
        // Select two different parent nodes.
        var house = treeRoot.Children
            .OfType<TreeNode<Space>>()
            .First(n => ((Space)n.PayloadObject).Name == "House");
        var shed = treeRoot.Children
            .OfType<TreeNode<Space>>()
            .First(n => ((Space)n.PayloadObject).Name == "Shed");
        // Assume house has at least one child.
        var childOfHouse = house.Children.First();
        int initialHouseCount = house.Children.Count;
        int initialShedCount = shed.Children.Count;

        // Act: Move the child from House to Shed.
        controller.MoveNode(childOfHouse.NodeId, shed.NodeId);

        // Assert
        Assert.That(house.Children.Count, Is.EqualTo(initialHouseCount - 1));
        Assert.That(shed.Children.Count, Is.EqualTo(initialShedCount + 1));
        Assert.That(childOfHouse.Parent, Is.EqualTo(shed));
    }

    [Test]
    public void GetDescendantsAndAncestors_ReturnsCorrectNodes()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));
        // Pick "Basement" node which has a child "Bedroom".
        var basement = treeRoot.Children
            .OfType<TreeNode<Space>>()
            .First(n => ((Space)n.PayloadObject).Name == "Basement");
        var bedroom = basement.Children.First();

        // Act
        var descendantsOfBasement = controller.GetDescendants(basement.NodeId).ToList();
        var ancestorsOfBedroom = controller.GetAncestors(bedroom.NodeId).ToList();

        // Assert: Basement's descendants should include Bedroom.
        Assert.That(descendantsOfBasement.Any(n => n.NodeId == bedroom.NodeId), Is.True);
        // Ancestors of Bedroom should include Basement and House (assuming Bedroom is under Basement which is under Property, etc.).
        Assert.That(ancestorsOfBedroom.Any(n => n.NodeId == basement.NodeId), Is.True);
    }

    [Test]
    public void SearchNodes_FindsNodesByPredicate()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));
        // Predicate: find nodes with SquareFeet less than 1000.
        Func<ITreeNode, bool> predicate = node =>
        {
            if (node.PayloadObject is Space space)
                return space.SquareFeet < 1000;
            return false;
        };

        // Act
        var results = controller.SearchNodes(predicate).ToList();

        // Assert: At least one node (e.g. "Bathroom" or "Shed") should match.
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(n => (n.PayloadObject as Space)?.Name == "Shed"), Is.True);
    }

    [Test]
    public void ExportToJsonAndImportFromJson_WorksCorrectly()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var treeView = new TreeView(treeRoot);
        var controller = new TreeController<TreeNode<Space>>(treeRoot, treeView);
        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        // Export to JSON.
        string jsonExport = controller.ExportToJson(payloadTypes, includeViewRoot: true);

        // Simulate external changes by creating a new TreeController with the same tree.
        var newTreeView = new TreeView(treeRoot);
        var newController = new TreeController<TreeNode<Space>>(treeRoot, newTreeView);
        // Import from JSON.
        newController.ImportFromJson(jsonExport, payloadTypes, id =>
        {
            // Build lookup from the current tree.
            var lookup = NodeLookupHelper.BuildLookup(treeRoot);
            return lookup.TryGetValue(id, out var node) ? node : null;
        });

        // Assert: Check that the TreeView settings are restored.
        Assert.That(treeView.IncludedProperties, Is.EqualTo(newController.TreeView.IncludedProperties));
        Assert.That(treeView.ExcludedProperties, Is.EqualTo(newController.TreeView.ExcludedProperties));
    }

    [Test]
    public void BatchOperations_UpdateAndRemoveNodes_Works()
    {
        // Arrange
        var treeRoot = TestHelpers.CreateTestSpaceTree();
        var controller = new TreeController<TreeNode<Space>>(treeRoot, new TreeView(treeRoot));
        // Collect nodeIds for all children of "House"
        var house = treeRoot.Children
            .OfType<TreeNode<Space>>()
            .First(n => ((Space)n.PayloadObject).Name == "House");
        var childIds = house.Children.Select(n => n.NodeId).ToList();

        // Act: Batch update the Name property of all children of House.
        string newSuffix = " - Updated";
        controller.UpdateNodesPayloadProperty(childIds, "Name", newSuffix);
        // Verify update.
        foreach (var child in house.Children)
        {
            var space = child.PayloadObject as Space;
            Assert.That(space, Is.Not.Null);
            Assert.That(space!.Name.EndsWith(newSuffix), Is.True);
        }

        // Act: Batch remove all children of House.
        controller.RemoveNodes(childIds);
        // Assert: House should now have no children.
        Assert.That(house.Children.Count, Is.EqualTo(0));
    }
}
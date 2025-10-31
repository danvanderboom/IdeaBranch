using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

[TestFixture]
public class TreeViewDeserializationTests
{
    /// <summary>
    /// In this test we create a full Space tree and then create a TreeView based on a non-root node ("House").
    /// We then serialize (with includeViewRoot=true) and deserialize the TreeView.
    /// We expect that the deserialized TreeView’s Root corresponds to the "House" node.
    /// </summary>
    [Test]
    public void TreeView_Deserialize_FromNonRootNode()
    {
        // Arrange: create the full sample tree.
        var fullTree = TestHelpers.CreateTestSpaceTree();
        var house = (TreeNode<Space>)fullTree.Children
            .First(n => n is TreeNode<Space> && ((TreeNode<Space>)n).Payload.Name == "House");

        // Create a TreeView based on the "House" node.
        var treeView = new TreeView(house);

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        // Build lookup dictionary and lookup function.
        var nodeDictionary = NodeLookupHelper.BuildLookup(house);
        var nodeLookup = new Func<string, ITreeNode?>(id => nodeDictionary.TryGetValue(id, out var node) ? node : null);

        // Serialize with includeViewRoot = true.
        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);

        // Act: Deserialize.
        TreeView? deserialized = TreeViewJsonSerializer.Deserialize(json, payloadTypes, nodeLookup);

        // Assert: The deserialized TreeView should be based on the "House" node.
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Root.NodeId, Is.EqualTo(house.NodeId));

        // Also verify that the payload's Name property is correct.
        var spacePayload = (Space)deserialized.Root.PayloadObject;
        Assert.That(spacePayload.Name, Is.EqualTo(house.PayloadObject is Space s ? s.Name : string.Empty));
    }

    /// <summary>
    /// Test that after deserialization the TreeView's expanded states are restored from the JSON.
    /// </summary>
    [Test]
    public void TreeView_Deserialize_RestoresExpandedStates()
    {
        // Arrange: Create a sample Space tree.
        var property = TestHelpers.CreateTestSpaceTree();
        var treeView = new TreeView(property, defaultExpanded: true);

        // Set custom expanded states.
        var house = (TreeNode<Space>)property.Children
            .First(n => n is TreeNode<Space> && ((TreeNode<Space>)n).Payload.Name == "House");
        var shed = (TreeNode<Space>)property.Children
            .First(n => n is TreeNode<Space> && ((TreeNode<Space>)n).Payload.Name == "Shed");

        treeView.SetIsExpanded(property, true);
        treeView.SetIsExpanded(house, false);
        treeView.SetIsExpanded(shed, true);

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        // Build lookup dictionary and lookup function.
        var nodeDictionary = NodeLookupHelper.BuildLookup(property);
        var nodeLookup = new Func<string, ITreeNode?>(id => nodeDictionary.TryGetValue(id, out var node) ? node : null);

        // Serialize with includeViewRoot = true.
        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);

        // Act: Deserialize.
        TreeView? deserialized = TreeViewJsonSerializer.Deserialize(json, payloadTypes, nodeLookup);

        // Assert: Verify that expanded states have been restored.
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.GetIsExpanded(property), Is.True);
        Assert.That(deserialized.GetIsExpanded(house), Is.False);
        Assert.That(deserialized.GetIsExpanded(shed), Is.True);
    }

    /// <summary>
    /// Test deserialization of a TreeView built on an inherited node tree.
    /// </summary>
    [Test]
    public void TreeView_Deserialize_WithInheritedNodes()
    {
        // Arrange: Create an inherited node tree (Forest, OakTree, Branch, Leaf).
        var forest = TestHelpers.CreateInheritedNodeTestSpaceTree();
        var treeView = new TreeView(forest, defaultExpanded: true);

        var payloadTypes = new Dictionary<string, Type>
        {
            { "Forest", typeof(Forest) },
            { "OakTree", typeof(OakTree) },
            { "Branch", typeof(Branch) },
            { "Leaf", typeof(Leaf) }
        };

        // Build lookup dictionary and lookup function.
        var nodeDictionary = NodeLookupHelper.BuildLookup(forest);
        var nodeLookup = new Func<string, ITreeNode?>(id => nodeDictionary.TryGetValue(id, out var node) ? node : null);

        // Serialize with includeViewRoot = true.
        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);

        // Act: Deserialize.
        TreeView? deserialized = TreeViewJsonSerializer.Deserialize(json, payloadTypes, nodeLookup);

        // Assert: Verify that the deserialized TreeView's root is the forest.
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Root.NodeId, Is.EqualTo(forest.NodeId));

        // Verify that at least one OakTree node exists in the deserialized tree.
        bool oakFound = false;

        void Traverse(ITreeNode node)
        {
            if (node.PayloadType.Contains("OakTree"))
                oakFound = true;
            foreach (var child in node.Children)
                Traverse(child);
        }

        Traverse(deserialized.Root);
        Assert.That(oakFound, Is.True);
    }

    /// <summary>
    /// Test deserializing a TreeView that consists of a single node (a leaf).
    /// The resulting TreeView should have the same root and no children.
    /// </summary>
    [Test]
    public void TreeView_Deserialize_SingleNode()
    {
        // Arrange: Create a single-node tree.
        var singleNode = new TreeNode<Space>(new Space { Name = "Solo", SquareFeet = 1000 });
        var treeView = new TreeView(singleNode);
        treeView.IncludedProperties = new List<string> { "NodeId", "IsExpanded", "ChildrenCount" };

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        var lookup = NodeLookupHelper.BuildLookup(singleNode);
        Func<string, ITreeNode?> nodeLookup = id => lookup.TryGetValue(id, out var node) ? node : null;

        // Serialize with includeViewRoot = true.
        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);

        // Act: Deserialize.
        TreeView? deserialized = TreeViewJsonSerializer.Deserialize(json, payloadTypes, nodeLookup);

        // Assert: The deserialized TreeView should have the same root and no children.
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Root.NodeId, Is.EqualTo(singleNode.NodeId));
        Assert.That(deserialized.Root.Children.Count, Is.EqualTo(0));
        Assert.That(deserialized.GetIsExpanded(singleNode), Is.EqualTo(treeView.GetIsExpanded(singleNode)));
    }

    /// <summary>
    /// Test that deserialization fails if the JSON is missing the RootNodeId property.
    /// </summary>
    [Test]
    public void TreeView_Deserialize_MissingRootNode_ThrowsJsonException()
    {
        // Arrange: Create a valid treeView and serialize it.
        var singleNode = new TreeNode<Space>(new Space { Name = "Solo", SquareFeet = 1000 });
        var treeView = new TreeView(singleNode);
        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        var lookup = NodeLookupHelper.BuildLookup(singleNode);
        Func<string, ITreeNode?> nodeLookup = id => lookup.TryGetValue(id, out var node) ? node : null;

        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);
        // Remove the RootNodeId property.
        JsonObject jsonObj = JsonNode.Parse(json)!.AsObject();
        jsonObj.Remove("RootNodeId");
        string modifiedJson = jsonObj.ToJsonString();

        // Act & Assert: Deserialization should throw a JsonException.
        Assert.That(() => TreeViewJsonSerializer.Deserialize(modifiedJson, payloadTypes, nodeLookup),
            Throws.TypeOf<JsonException>().With.Message.Contains("Missing RootNodeId"));
    }

    /// <summary>
    /// Test that deserialization fails when the NodeLookup delegate returns null.
    /// </summary>
    [Test]
    public void TreeView_Deserialize_NodeLookupFailure_ThrowsJsonException()
    {
        // Arrange: Create a valid treeView.
        var singleNode = new TreeNode<Space>(new Space { Name = "Solo", SquareFeet = 1000 });
        var treeView = new TreeView(singleNode);
        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        // Provide a NodeLookup delegate that always returns null.
        Func<string, ITreeNode?> nodeLookup = id => null;

        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);

        // Act & Assert: Deserialization should throw a JsonException because the root node cannot be found.
        Assert.That(() => TreeViewJsonSerializer.Deserialize(json, payloadTypes, nodeLookup),
            Throws.TypeOf<JsonException>().With.Message.Contains("No ITreeNode found"));
    }

    /// <summary>
    /// Test that IncludedProperties and ExcludedProperties are restored after deserialization.
    /// </summary>
    [Test]
    public void TreeView_Deserialize_RestoresIncludedExcludedProperties()
    {
        // Arrange: Create a sample tree.
        var property = TestHelpers.CreateTestSpaceTree();
        var treeView = new TreeView(property);
        treeView.IncludedProperties = new List<string> { "NodeId", "PayloadType", "IsExpanded", "ChildrenCount" };
        treeView.ExcludedProperties = new List<string> { "Payload.SquareFeet" };

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        var lookup = NodeLookupHelper.BuildLookup(property);
        Func<string, ITreeNode?> nodeLookup = id => lookup.TryGetValue(id, out var node) ? node : null;

        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);
        TreeView? deserialized = TreeViewJsonSerializer.Deserialize(json, payloadTypes, nodeLookup);

        // Assert: IncludedProperties and ExcludedProperties are restored.
        Assert.That(deserialized, Is.Not.Null);
        CollectionAssert.AreEqual(treeView.IncludedProperties, deserialized!.IncludedProperties);
        CollectionAssert.AreEqual(treeView.ExcludedProperties, deserialized.ExcludedProperties);
    }

    /// <summary>
    /// Test deserialization of a TreeView that contains mixed payload types (e.g. Space and Substance).
    /// </summary>
    [Test]
    public void TreeView_Deserialize_MixedPayloadTypes()
    {
        // Arrange: Create a mixed node tree.
        var mixedTree = TestHelpers.CreateMixedNodeTestSpaceTree();
        var treeView = new TreeView(mixedTree);
        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        var lookup = NodeLookupHelper.BuildLookup(mixedTree);
        Func<string, ITreeNode?> nodeLookup = id => lookup.TryGetValue(id, out var node) ? node : null;

        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);
        TreeView? deserialized = TreeViewJsonSerializer.Deserialize(json, payloadTypes, nodeLookup);

        // Assert: Verify that both Space and Substance nodes exist in the deserialized tree.
        bool foundSubstance = false;
        void Traverse(ITreeNode node)
        {
            if (node.PayloadObject is Substance)
                foundSubstance = true;
            foreach (var child in node.Children)
                Traverse(child);
        }
        Traverse(deserialized!.Root);
        Assert.That(foundSubstance, Is.True);
    }

    /// <summary>
    /// Test deserialization of a deeply nested tree with custom expanded states.
    /// </summary>
    [Test]
    public void TreeView_Deserialize_DeeplyNestedExpandedStates()
    {
        // Arrange: Create a test tree and set custom expanded states.
        var property = TestHelpers.CreateTestSpaceTree();
        var house = (TreeNode<Space>)property.Children
            .First(n => ((TreeNode<Space>)n).Payload.Name == "House");
        var basement = (TreeNode<Space>)property.Children
            .First(n => ((TreeNode<Space>)n).Payload.Name == "Basement");
        var bedroom = basement.Children.First();

        var treeView = new TreeView(property, defaultExpanded: true);
        treeView.SetIsExpanded(property, true);
        treeView.SetIsExpanded(house, false);
        treeView.SetIsExpanded(basement, true);
        treeView.SetIsExpanded(bedroom, false);

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        var lookup = NodeLookupHelper.BuildLookup(property);
        Func<string, ITreeNode?> nodeLookup = id => lookup.TryGetValue(id, out var node) ? node : null;

        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);
        TreeView? deserialized = TreeViewJsonSerializer.Deserialize(json, payloadTypes, nodeLookup);

        // Assert: Verify that the expanded states are restored.
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.GetIsExpanded(property), Is.True);
        Assert.That(deserialized.GetIsExpanded(house), Is.False);
        Assert.That(deserialized.GetIsExpanded(basement), Is.True);
        Assert.That(deserialized.GetIsExpanded(bedroom), Is.False);
    }

    /// <summary>
    /// Test that the projected collection is rebuilt correctly upon deserialization.
    /// </summary>
    [Test]
    public void TreeView_Deserialize_ProjectedCollectionIntegrity()
    {
        // Arrange: Create a test tree and update its projected collection.
        var property = TestHelpers.CreateTestSpaceTree();
        var treeView = new TreeView(property, defaultExpanded: true);
        treeView.UpdateProjectedCollection();

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        var lookup = NodeLookupHelper.BuildLookup(property);
        Func<string, ITreeNode?> nodeLookup = id => lookup.TryGetValue(id, out var node) ? node : null;

        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);
        TreeView? deserialized = TreeViewJsonSerializer.Deserialize(json, payloadTypes, nodeLookup);

        // Assert: The projected collection count should match between the original and deserialized TreeView.
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.ProjectedCollection.Count, Is.EqualTo(treeView.ProjectedCollection.Count));
    }

    /// <summary>
    /// This test verifies that when a limited set of properties is specified in the IncludedProperties list,
    /// the serialized JSON only includes those properties (e.g. "Name" is present but "SquareFeet" is not).
    /// After deserializing, clearing the IncludedProperties list (allowing all properties) and re-serializing
    /// should include all properties (so "SquareFeet" should now be present).
    /// </summary>
    [Test]
    public void TreeView_Serialize_WithClearedIncludedProperties_ReturnsAllProperties()
    {
        // Arrange: Create a full sample tree and use the "House" node as the TreeView root.
        var fullTree = TestHelpers.CreateTestSpaceTree();
        var house = (TreeNode<Space>)fullTree.Children
            .First(n => n is TreeNode<Space> && ((TreeNode<Space>)n).Payload.Name == "House");
        var treeView = new TreeView(house);

        // Limit included properties to a small set.
        treeView.IncludedProperties = new List<string>
        {
            "NodeId",
            "PayloadType",
            "Name",         // from the payload (Space.Name)
            "IsExpanded",
            "ChildrenCount"
        };
        // Note: With this limitation, properties like "SquareFeet" should not appear.

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        // Build lookup dictionary and lookup delegate for the subtree.
        var lookup = NodeLookupHelper.BuildLookup(house);
        Func<string, ITreeNode?> nodeLookup = id => lookup.TryGetValue(id, out var node) ? node : null;

        // Serialize with includeViewRoot = true.
        string jsonLimited = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);
        JsonObject jsonObjLimited = JsonNode.Parse(jsonLimited)!.AsObject();
        // The outer JSON object contains a "View" property holding the serialized node.
        JsonObject viewObjLimited = jsonObjLimited["View"]!.AsObject();

        // Assert: With limited IncludedProperties, the merged payload should contain "Name" but not "SquareFeet".
        Assert.That(viewObjLimited.ContainsKey("Name"), Is.True, "Expected 'Name' to be present in limited serialization.");
        Assert.That(viewObjLimited.ContainsKey("SquareFeet"), Is.False, "Expected 'SquareFeet' to be absent in limited serialization.");

        // Act: Deserialize the TreeView.
        TreeView? deserialized = TreeViewJsonSerializer.Deserialize(jsonLimited, payloadTypes, nodeLookup);
        Assert.That(deserialized, Is.Not.Null, "Deserialized TreeView should not be null.");

        // Now clear the IncludedProperties so that all properties will be included.
        deserialized!.IncludedProperties.Clear();

        // Re-serialize the TreeView.
        string jsonAll = TreeViewJsonSerializer.Serialize(deserialized, payloadTypes, includeViewRoot: true);
        JsonObject jsonObjAll = JsonNode.Parse(jsonAll)!.AsObject();
        JsonObject viewObjAll = jsonObjAll["View"]!.AsObject();

        // Assert: With no inclusion restrictions, the payload should include "SquareFeet".
        Assert.That(viewObjAll.ContainsKey("SquareFeet"), Is.True, "Expected 'SquareFeet' to be present when no inclusion restrictions are applied.");
    }

    /// <summary>
    /// This test ensures that when IncludeViewRoot is true during serialization,
    /// the outer JSON object always contains the view root properties:
    /// "RootNodeId", "IncludedProperties", "ExcludedProperties", "DefaultExpanded", and "View".
    /// </summary>
    [Test]
    public void Serialize_AlwaysIncludesViewRootProperties()
    {
        // Arrange: Create a sample tree using the test helper.
        var fullTree = TestHelpers.CreateTestSpaceTree();
        // Create a TreeView based on the full tree.
        var treeView = new TreeView(fullTree)
        {
            // Set IncludedProperties and ExcludedProperties to arbitrary values.
            IncludedProperties = new List<string> { "NodeId", "Children" },
            ExcludedProperties = new List<string> { "RootNodeId", "View" }
        };

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        // Act: Serialize with includeViewRoot = true.
        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes, includeViewRoot: true);
        JsonObject jsonObj = JsonNode.Parse(json)!.AsObject();

        // Assert: The outer JSON object must contain the view root properties.
        Assert.That(jsonObj.ContainsKey("RootNodeId"), Is.True, "Expected 'RootNodeId' to be present.");
        Assert.That(jsonObj.ContainsKey("IncludedProperties"), Is.True, "Expected 'IncludedProperties' to be present.");
        Assert.That(jsonObj.ContainsKey("ExcludedProperties"), Is.True, "Expected 'ExcludedProperties' to be present.");
        Assert.That(jsonObj.ContainsKey("DefaultExpanded"), Is.True, "Expected 'DefaultExpanded' to be present.");
        Assert.That(jsonObj.ContainsKey("View"), Is.True, "Expected 'View' to be present.");
    }
}
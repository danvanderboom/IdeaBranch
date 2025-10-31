using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

[TestFixture]
public class TreeViewJsonConverterTests
{
    // Helper: Returns a list of node names in pre-order from the serialized JSON using System.Text.Json.Nodes.
    private List<string> GetSerializedNodeNames(JsonObject jNode)
    {
        var names = new List<string>();

        foreach (var kv in jNode.Where(n => n.Key == nameof(Space.Name)))
        {
            var nameNode = kv.Value;
            string? name = nameNode?.GetValue<string>();
            names.Add(name ?? "null");
        }

        // Process children recursively.
        if (jNode.TryGetPropertyValue(nameof(ITreeNode.Children), out JsonNode? childrenNode) 
            && childrenNode is JsonArray childrenArray)
        {
            foreach (var child in childrenArray)
            {
                if (child is JsonObject childObj)
                    names.AddRange(GetSerializedNodeNames(childObj));
            }
        }

        return names;
    }

    [Test]
    public void SerializeFullTree_IncludesAllVisibleNodes()
    {
        // Create the full sample tree.
        var root = TestHelpers.CreateTestSpaceTree();
        var treeView = new TreeView(root);

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes);

        JsonObject jTree = JsonNode.Parse(json)!.AsObject();

        // Check that the root object has expected properties.
        Assert.That(jTree[nameof(ITreeNode.PayloadType)], Is.Not.Null);
        Assert.That(jTree[nameof(ITreeNode.Children)], Is.Not.Null);

        // For the full tree (all nodes expanded), the pre-order traversal of visible node names should be:
        // "Property", "House", "Kitchen", "Bathroom", "Shed", "Basement", "Bedroom"
        var expected = new List<string>
        {
            "Property", "House", "Kitchen", "Bathroom", "Shed", "Basement", "Bedroom"
        };

        List<string> actual = GetSerializedNodeNames(jTree);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void SerializePartialTree_ExcludesCollapsedSubtrees()
    {
        // Create the sample tree.
        var root = TestHelpers.CreateTestSpaceTree();
        var treeView = new TreeView(root);

        // Collapse the "House" node so its children become invisible.
        var house = root.Descendants.OfType<TreeNode<Space>>().First(n => n.Payload.Name == "House");
        treeView.SetIsExpanded(house, false);

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes);
        JsonObject jTree = JsonNode.Parse(json)!.AsObject();

        // Get serialized node names.
        // With "House" collapsed, its children ("Kitchen" and "Bathroom") should not appear.
        // Expected order: "Property", "House", "Shed", "Basement", "Bedroom"
        var expected = new List<string>
        {
            "Property", "House", "Shed", "Basement", "Bedroom"
        };

        List<string> actual = GetSerializedNodeNames(jTree);
        Assert.That(expected, Is.EqualTo(actual));

        // Also verify that the "House" node does not include a "Children" array (or that it is empty).
        // Locate the "House" node.
        JsonObject? jHouse = null;
        if (jTree.TryGetPropertyValue(nameof(ITreeNode.Children), out JsonNode? rootChildrenNode) && rootChildrenNode is JsonArray rootChildrenArray)
        {
            foreach (JsonNode? child in rootChildrenArray)
            {
                if (child is JsonObject childObj && childObj.TryGetPropertyValue(nameof(Space.Name), out JsonNode? nameNode))
                {
                    jHouse = childObj;
                    break;
                }
            }
        }

        Assert.That(jHouse, Is.Not.Null, "House node not found in JSON.");
        if (jHouse!.TryGetPropertyValue(nameof(ITreeNode.Children), out JsonNode? houseChildrenNode) && houseChildrenNode is JsonArray houseChildrenArray)
        {
            Assert.That(houseChildrenArray.Count, Is.EqualTo(0), "Collapsed node should not serialize its children.");
        }
    }

    [Test]
    public void TreeView_Created_BasedOnNonRootNode()
    {
        // Create the full sample tree.
        var root = TestHelpers.CreateTestSpaceTree();
        var house = (TreeNode<Space>)root.Children
            .Where(n => n is TreeNode<Space> && (n as TreeNode<Space>)?.Payload.Name == "House")
            .First();

        var treeView = new TreeView(house);

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        string json = TreeViewJsonSerializer.Serialize(treeView, payloadTypes);

        JsonObject jTree = JsonNode.Parse(json)!.AsObject();

        // Check that the root object has expected properties.
        Assert.That(jTree[nameof(ITreeNode.NodeId)]?.GetValue<string>(), Is.EqualTo(house.NodeId));
        Assert.That(jTree[nameof(Space.Name)]?.GetValue<string>(), Is.EqualTo(house.Payload.Name));
    }
}
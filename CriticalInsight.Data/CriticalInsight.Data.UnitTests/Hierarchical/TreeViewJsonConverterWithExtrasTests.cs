using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

[TestFixture]
public class TreeViewJsonConverterWithExtrasTests
{
    private class NodeInfo
    {
        public string Name { get; set; } = "";
        public bool IsExpanded { get; set; }
        public int ChildrenCount { get; set; }
        public IList<NodeInfo> Children { get; set; } = new List<NodeInfo>();
    }

    private NodeInfo ParseNode(JsonElement element)
    {
        var info = new NodeInfo();

        if (element.TryGetProperty(nameof(Space.Name), out JsonElement nameElement))
        {
            info.Name = nameElement.GetString() ?? "";
        }

        if (element.TryGetProperty("IsExpanded", out JsonElement isExpandedElement))
        {
            info.IsExpanded = isExpandedElement.GetBoolean();
        }

        if (element.TryGetProperty("ChildrenCount", out JsonElement childrenCountElement))
        {
            info.ChildrenCount = childrenCountElement.GetInt32();
        }

        if (element.TryGetProperty(nameof(ITreeNode.Children), out JsonElement childrenElement) && childrenElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in childrenElement.EnumerateArray())
            {
                info.Children.Add(ParseNode(child));
            }
        }

        return info;
    }

    private NodeInfo GetTreeInfo(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return ParseNode(doc.RootElement);
    }

    [Test]
    public void Serialize_FullTree_Includes_IsExpanded_And_ChildrenCount()
    {
        var root = TestHelpers.CreateTestSpaceTree();
        var treeView = new TreeView(root);

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        var json = treeView.Serialize(payloadTypes);
        NodeInfo info = GetTreeInfo(json);

        // Verify root node.
        Assert.That(info.Name, Is.EqualTo("Property"));
        Assert.That(info.IsExpanded, Is.True);
        Assert.That(info.ChildrenCount, Is.EqualTo(3));
        Assert.That(info.Children.Count, Is.EqualTo(3));

        // Verify House node.
        var house = info.Children.FirstOrDefault(n => n.Name == "House");
        Assert.That(house, Is.Not.Null, "House node not found");
        Assert.That(house.IsExpanded, Is.True);
        Assert.That(house.ChildrenCount, Is.EqualTo(2));
        Assert.That(house.Children.Count, Is.EqualTo(2));

        // Verify Kitchen and Bathroom.
        var kitchen = house.Children.FirstOrDefault(n => n.Name == "Kitchen");
        var bathroom = house.Children.FirstOrDefault(n => n.Name == "Bathroom");
        Assert.That(kitchen, Is.Not.Null, "Kitchen node not found");
        Assert.That(bathroom, Is.Not.Null, "Bathroom node not found");
        Assert.That(kitchen.IsExpanded, Is.True);
        Assert.That(bathroom.IsExpanded, Is.True);
        Assert.That(kitchen.ChildrenCount, Is.EqualTo(0));
        Assert.That(bathroom.ChildrenCount, Is.EqualTo(0));

        // Verify Shed node.
        var shed = info.Children.FirstOrDefault(n => n.Name == "Shed");
        Assert.That(shed, Is.Not.Null, "Shed node not found");
        Assert.That(shed.IsExpanded, Is.True);
        Assert.That(shed.ChildrenCount, Is.EqualTo(0));
        Assert.That(shed.Children.Count, Is.EqualTo(0));

        // Verify Basement node.
        var basement = info.Children.FirstOrDefault(n => n.Name == "Basement");
        Assert.That(basement, Is.Not.Null, "Basement node not found");
        Assert.That(basement.IsExpanded, Is.True);
        Assert.That(basement.ChildrenCount, Is.EqualTo(1));
        Assert.That(basement.Children.Count, Is.EqualTo(1));

        // Verify Bedroom node under Basement.
        var bedroom = basement.Children.FirstOrDefault(n => n.Name == "Bedroom");
        Assert.That(bedroom, Is.Not.Null, "Bedroom node not found");
        Assert.That(bedroom.IsExpanded, Is.True);
        Assert.That(bedroom.ChildrenCount, Is.EqualTo(0));
    }

    [Test]
    public void Serialize_PartialTree_Excludes_CollapsedChildren_But_Includes_Extra_Properties()
    {
        var root = TestHelpers.CreateTestSpaceTree();
        var treeView = new TreeView(root);

        var houseNode = root.Descendants.OfType<TreeNode<Space>>().First(n => n.Payload.Name == "House");
        treeView.SetIsExpanded(houseNode, false);

        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        var json = treeView.Serialize(payloadTypes);
        NodeInfo info = GetTreeInfo(json);

        // For the collapsed "House" node:
        var houseInfo = info.Children.FirstOrDefault(n => n.Name == "House");
        Assert.That(houseInfo, Is.Not.Null, "House node not found");
        Assert.That(houseInfo.IsExpanded, Is.False, "House should be collapsed");
        Assert.That(houseInfo.ChildrenCount, Is.EqualTo(2), "House should have 2 children");
        Assert.That(houseInfo.Children.Count, Is.EqualTo(0), "Collapsed House node should not serialize its children");

        // Other nodes remain fully expanded.
        var shed = info.Children.FirstOrDefault(n => n.Name == "Shed");
        Assert.That(shed, Is.Not.Null, "Shed node not found");
        Assert.That(shed.IsExpanded, Is.True);
        Assert.That(shed.ChildrenCount, Is.EqualTo(0));

        var basement = info.Children.FirstOrDefault(n => n.Name == "Basement");
        Assert.That(basement, Is.Not.Null, "Basement node not found");
        Assert.That(basement.IsExpanded, Is.True);
        Assert.That(basement.ChildrenCount, Is.EqualTo(1));
    }
}
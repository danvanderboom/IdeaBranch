using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using System.Text.Json.Nodes;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

public class TreeTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Tree_Serialize_Deserialize()
    {
        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        var firstTree = TestHelpers.CreateTestSpaceTree();

        // Serialize the tree.
        var json = firstTree.Serialize(payloadTypes);

        JsonObject? jObj = JsonNode.Parse(json)?.AsObject();

        // Helper function to check if a property exists at the root.
        bool HasProperty(string name) => jObj != null && jObj.ContainsKey(name);

        var propertiesToNotSerialize = new List<string>
        {
            nameof(firstTree.Root), nameof(firstTree.Parent), nameof(firstTree.Depth),
            nameof(firstTree.Subtree), nameof(firstTree.Ancestors), nameof(firstTree.Descendants)
        };

        bool hasPropertyItShouldnt = propertiesToNotSerialize.Any(p => HasProperty(p));
        Assert.That(hasPropertyItShouldnt, Is.False, "Some properties that should not be serialized were found.");

        var propertiesToSerialize = new List<string>
        {
            nameof(firstTree.PayloadType), nameof(firstTree.Payload), nameof(firstTree.Children)
        };

        bool missingRequiredProperty = !propertiesToSerialize.All(p => HasProperty(p));
        Assert.That(missingRequiredProperty, Is.False, "Some required properties are missing.");
    }

    [Test]
    public void Tree_MixedNodeTypes_Serialize_Deserialize()
    {
        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };

        var firstTree = TestHelpers.CreateMixedNodeTestSpaceTree();
        string firstJson = firstTree.Serialize(payloadTypes);

        var secondTree = TreeJsonSerializer.Deserialize<TreeNode<Space>>(firstJson, payloadTypes);
        string secondJson = secondTree?.Serialize(payloadTypes) ?? "null";

        Assert.That(firstJson, Is.EqualTo(secondJson));
    }

    [Test]
    public void Tree_InheritedNodeTypes_Serialize_Deserialize()
    {
        var payloadTypes = new Dictionary<string, Type>
        {
            { nameof(Forest), typeof(Forest) },
            { nameof(OakTree), typeof(OakTree) },
            { nameof(Branch), typeof(Branch) },
            { nameof(Leaf), typeof(Leaf) }
        };

        var forest1 = TestHelpers.CreateInheritedNodeTestSpaceTree();
        var json1 = forest1.Serialize(payloadTypes);

        var forest2 = TreeJsonSerializer.Deserialize<Forest>(json1, payloadTypes);
        var json2 = forest2?.Serialize(payloadTypes) ?? "null";

        Assert.That(json1, Is.EqualTo(json2));
    }
}
using CriticalInsight.Data.Hierarchical;

namespace CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;

// Helper for building a lookup dictionary from a tree (by NodeId).
internal static class NodeLookupHelper
{
    public static Dictionary<string, ITreeNode> BuildLookup(ITreeNode root)
    {
        var dict = new Dictionary<string, ITreeNode>();
        void Traverse(ITreeNode node)
        {
            dict[node.NodeId] = node;
            foreach (var child in node.Children)
                Traverse(child);
        }
        Traverse(root);
        return dict;
    }
}
using CriticalInsight.Data.Hierarchical;

namespace CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;

public abstract class BaseTreeNode<T> : TreeNode<T>
    where T : new()
{
    public string Color { get; set; } = "Transparent";
}
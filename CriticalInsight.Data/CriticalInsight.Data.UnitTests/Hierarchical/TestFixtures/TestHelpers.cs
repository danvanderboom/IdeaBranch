using CriticalInsight.Data.Hierarchical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;

public static class TestHelpers
{
    // Returns the list of Space names from the TreeView's projected collection.
    public static List<string> SpaceNames(TreeView treeView)
    {
        return treeView.ProjectedCollection
                       .Cast<TreeNode<Space>>()
                       .Select(n => n.Payload.Name)
                       .ToList();
    }

    // Creates a sample tree of Space nodes:
    //       Property
    //       /    |    \
    //   House  Shed  Basement
    //    /  \           |
    // Kitchen Bathroom Bedroom
    public static TreeNode<Space> CreateTestSpaceTree()
    {
        var property = new TreeNode<Space>(new Space { Name = "Property", SquareFeet = 20000 });

        var house = property.Children.Add(new TreeNode<Space>(new Space { Name = "House", SquareFeet = 2500 }));
        var shed = property.Children.Add(new TreeNode<Space>(new Space { Name = "Shed", SquareFeet = 240 }));
        var basement = property.Children.Add(new TreeNode<Space>(new Space { Name = "Basement", SquareFeet = 1500 }));

        house.Children.Add(new TreeNode<Space>(new Space { Name = "Kitchen", SquareFeet = 600 }));
        house.Children.Add(new TreeNode<Space>(new Space { Name = "Bathroom", SquareFeet = 300 }));
        basement.Children.Add(new TreeNode<Space>(new Space { Name = "Bedroom", SquareFeet = 400 }));

        return property;
    }

    public static ITreeNode<Space> CreateMixedNodeTestSpaceTree()
    {
        var property = new TreeNode<Space>(new Space { Name = "Property", SquareFeet = 20_000 });
        var shed = property.Children.Add(new TreeNode<Space>(new Space { Name = "Shed", SquareFeet = 240 }));
        var house = property.Children.Add(new TreeNode<Space>(new Space { Name = "House", SquareFeet = 2500 }));
        var kitchen = house.Children.Add(new TreeNode<Space>(new Space { Name = "Kitchen", SquareFeet = 600 }));
        var bathroom = house.Children.Add(new TreeNode<Space>(new Space { Name = "Bathroom", SquareFeet = 300 }));
        var basement = house.Children.Add(new TreeNode<Space>(new Space { Name = "Basement", SquareFeet = 1200 }));
        var basementBedroom = basement.Children.Add(new TreeNode<Space>(new Space { Name = "Bedroom", SquareFeet = 600 }));

        var lawnMower = property.Children.Add(new TreeNode<Substance>(new Substance { Name = "Lawnmower", Description = "Cut!" }));

        return property;
    }

    public static Forest CreateInheritedNodeTestSpaceTree()
    {
        var forest = new Forest { Color = "Green" };

        var tree1 = forest.Children.Add(new OakTree { Color = "Green" }) as OakTree;
        if (tree1 == null)
            throw new InvalidDataException(nameof(tree1));

        var branch1ontree1 = tree1.Children.Add(new Branch { Color = "Brown" });
        var branch2ontree1 = tree1.Children.Add(new Branch { Color = "Brown" });
        var branch3ontree1 = tree1.Children.Add(new Branch { Color = "Brown" });

        var tree2 = forest.Children.Add(new OakTree { Color = "Green" }) as OakTree;
        if (tree2 == null)
            throw new InvalidDataException(nameof(tree2));

        var branch1ontree2 = tree2.Children.Add(new Branch { Color = "Brown" });
        var branch2ontree2 = tree2.Children.Add(new Branch { Color = "Brown" });
        var branch3ontree2 = tree2.Children.Add(new Branch { Color = "Brown" });

        var branches = new List<OakTree> { tree1, tree2 }.SelectMany(t => t.Children).OfType<Branch>().ToList();

        foreach (var branch in branches)
        {
            _ = branch.Children.Add(new Leaf { Color = "Red" });
            _ = branch.Children.Add(new Leaf { Color = "Red" });
        }

        return forest;
    }
}
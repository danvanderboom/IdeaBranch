using FluentAssertions;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;
using IdeaBranch.App.ViewModels;
using IdeaBranch.Domain;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace IdeaBranch.UnitTests.ViewModels;

/// <summary>
/// Tests for TopicTreeViewModel.
/// </summary>
public class TopicTreeViewModelTests
{
    [Test]
    public async Task PromoteNodeToRoot_MovesNodeToTopLevel()
    {
        // Arrange
        var repository = new InMemoryTopicTreeRepository();
        var root = await repository.GetRootAsync();
        var parent = new TopicNode("Parent prompt", "Parent");
        var child = new TopicNode("Child prompt", "Child");
        root.AddChild(parent);
        parent.AddChild(child);
        await repository.SaveAsync(root);
        
        // Build tree structure manually to find child node
        var adapter = new TopicTreeAdapter();
        var reloadedRoot = await repository.GetRootAsync();
        var treeRoot = adapter.BuildTree(reloadedRoot);
        
        // Find the child node in the tree structure
        ITreeNode? childTreeNode = null;
        FindNodeInTree(treeRoot, child.Id, ref childTreeNode);
        
        childTreeNode.Should().NotBeNull("Child node should be found in tree structure");
        
        // Create ViewModel with the repository (initialization will happen asynchronously)
        var viewModel = new TopicTreeViewModel(repository);
        
        // Get initial root children count (before promotion)
        var initialRootChildrenCount = reloadedRoot.Children.Count;
        var childId = child.Id;
        var parentId = parent.Id;
        
        // Act - Promote using the manually built tree node
        await viewModel.PromoteNodeToRootAsync(childTreeNode!);
        
        // Reload to verify
        var finalRoot = await repository.GetRootAsync();
        
        // Assert
        finalRoot.Children.Should().Contain(c => c.Id == childId, "Child should be a direct child of root");
        finalRoot.Children.First(c => c.Id == childId).Parent.Should().Be(finalRoot, "Child's parent should be root");
        finalRoot.Children.Count.Should().Be(initialRootChildrenCount + 1, "Root should have one more child");
        
        // Verify parent no longer has the child
        var finalParent = finalRoot.Children.FirstOrDefault(c => c.Id == parentId);
        finalParent.Should().NotBeNull("Parent should still exist");
        if (finalParent != null)
        {
            finalParent.Children.Should().NotContain(c => c.Id == childId, "Parent should no longer have the child");
        }
    }
    
    /// <summary>
    /// Recursively searches for a node by domain ID in the tree structure.
    /// </summary>
    private void FindNodeInTree(ITreeNode node, System.Guid domainNodeId, ref ITreeNode? result)
    {
        var payload = TopicTreeViewModel.GetPayload(node);
        if (payload?.DomainNodeId == domainNodeId)
        {
            result = node;
            return;
        }
        
        foreach (var child in node.Children)
        {
            if (result == null)
            {
                FindNodeInTree(child, domainNodeId, ref result);
            }
        }
    }
}


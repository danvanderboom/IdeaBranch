using FluentAssertions;
using IdeaBranch.Domain;
using NUnit.Framework;

namespace IdeaBranch.UnitTests.Domain;

/// <summary>
/// Tests for topic tree manipulation logic.
/// Covers requirements: Hierarchical topic organization
/// Scenario: List responses create child nodes (Test ID: IB-UI-020)
/// </summary>
public class TopicTreeManipulationTests
{
    [Test]
    public void AddNode_ShouldCreateNewNode()
    {
        // Arrange
        var parent = new TopicNode("Parent prompt", "Parent");
        var child = new TopicNode("Child prompt", "Child");
        
        // Act
        parent.AddChild(child);
        
        // Assert
        parent.Children.Should().Contain(child);
        child.Parent.Should().Be(parent);
        parent.Children.Count.Should().Be(1);
    }

    [Test]
    public void MoveNode_ShouldChangeParent()
    {
        // Arrange
        var oldParent = new TopicNode("Old parent prompt", "Old Parent");
        var newParent = new TopicNode("New parent prompt", "New Parent");
        var child = new TopicNode("Child prompt", "Child");
        oldParent.AddChild(child);
        
        // Act
        oldParent.MoveChild(child, newParent);
        
        // Assert
        oldParent.Children.Should().NotContain(child);
        newParent.Children.Should().Contain(child);
        child.Parent.Should().Be(newParent);
        oldParent.Children.Count.Should().Be(0);
        newParent.Children.Count.Should().Be(1);
    }

    [Test]
    public void DeleteNode_ShouldRemoveFromTree()
    {
        // Arrange
        var parent = new TopicNode("Parent prompt", "Parent");
        var child = new TopicNode("Child prompt", "Child");
        parent.AddChild(child);
        
        // Act
        var removed = parent.RemoveChild(child);
        
        // Assert
        removed.Should().BeTrue();
        parent.Children.Should().NotContain(child);
        child.Parent.Should().BeNull();
        parent.Children.Count.Should().Be(0);
    }

    [Test]
    public void RemoveChild_WithNonChild_ShouldReturnFalse()
    {
        // Arrange
        var parent = new TopicNode("Parent prompt", "Parent");
        var child = new TopicNode("Child prompt", "Child");
        
        // Act
        var removed = parent.RemoveChild(child);
        
        // Assert
        removed.Should().BeFalse();
        parent.Children.Count.Should().Be(0);
    }

    [Test]
    [TestCase("1. First item\n2. Second item\n3. Third item")]
    [TestCase("- First item\n- Second item\n- Third item")]
    [TestCase("* First item\n* Second item\n* Third item")]
    public void ParseListResponse_ShouldCreateChildNodes(string listResponse)
    {
        // Arrange
        var node = new TopicNode("Parent prompt", "Parent");
        
        // Act
        node.SetResponse(listResponse, parseListItems: true);
        
        // Assert
        node.Children.Count.Should().Be(3);
        node.Children[0].Prompt.Should().Be("First item");
        node.Children[1].Prompt.Should().Be("Second item");
        node.Children[2].Prompt.Should().Be("Third item");
    }

    [Test]
    public void SetResponse_WithParseListItems_False_ShouldNotCreateChildren()
    {
        // Arrange
        var node = new TopicNode("Parent prompt", "Parent");
        var listResponse = "1. First item\n2. Second item";
        
        // Act
        node.SetResponse(listResponse, parseListItems: false);
        
        // Assert
        node.Children.Count.Should().Be(0);
        node.Response.Should().Be(listResponse);
    }

    [Test]
    public void AddChild_ShouldPreventCycles()
    {
        // Arrange
        var parent = new TopicNode("Parent prompt", "Parent");
        var child = new TopicNode("Child prompt", "Child");
        var grandchild = new TopicNode("Grandchild prompt", "Grandchild");
        parent.AddChild(child);
        child.AddChild(grandchild);
        
        // Act & Assert
        var act = () => grandchild.AddChild(parent);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cycle*");
    }

    [Test]
    public void AddChild_ShouldPreventSelfReference()
    {
        // Arrange
        var node = new TopicNode("Prompt", "Title");
        
        // Act & Assert
        var act = () => node.AddChild(node);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*own child*");
    }

    [Test]
    public void AddChild_ShouldPreventDuplicateParents()
    {
        // Arrange
        var parent1 = new TopicNode("Parent 1 prompt", "Parent 1");
        var parent2 = new TopicNode("Parent 2 prompt", "Parent 2");
        var child = new TopicNode("Child prompt", "Child");
        parent1.AddChild(child);
        
        // Act & Assert
        var act = () => parent2.AddChild(child);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already has a parent*");
    }

    [Test]
    public void ParseMultiListResponse_WithHeadings_ShouldCreateIntermediateNodesWithTitles()
    {
        // Arrange
        var node = new TopicNode("Parent prompt", "Parent");
        var response = @"First Category:
- First item
- Second item

Second Category:
1. Third item
2. Fourth item";

        // Act
        node.SetResponse(response, parseListItems: true);

        // Assert
        node.Children.Count.Should().Be(2);
        
        // First intermediate node with title from heading
        var firstIntermediate = node.Children[0];
        firstIntermediate.Prompt.Should().Be("First Category");
        firstIntermediate.Title.Should().Be("First Category");
        firstIntermediate.Children.Count.Should().Be(2);
        firstIntermediate.Children[0].Prompt.Should().Be("First item");
        firstIntermediate.Children[1].Prompt.Should().Be("Second item");
        
        // Second intermediate node with title from heading
        var secondIntermediate = node.Children[1];
        secondIntermediate.Prompt.Should().Be("Second Category");
        secondIntermediate.Title.Should().Be("Second Category");
        secondIntermediate.Children.Count.Should().Be(2);
        secondIntermediate.Children[0].Prompt.Should().Be("Third item");
        secondIntermediate.Children[1].Prompt.Should().Be("Fourth item");
    }

    [Test]
    public void ParseMultiListResponse_WithoutHeadings_ShouldUseListNAsTitles()
    {
        // Arrange
        var node = new TopicNode("Parent prompt", "Parent");
        var response = @"- First item
- Second item

1. Third item
2. Fourth item

* Fifth item
* Sixth item";

        // Act
        node.SetResponse(response, parseListItems: true);

        // Assert
        node.Children.Count.Should().Be(3);
        
        // First intermediate node with default title
        var firstIntermediate = node.Children[0];
        firstIntermediate.Prompt.Should().Be("List 1");
        firstIntermediate.Title.Should().Be("List 1");
        firstIntermediate.Children.Count.Should().Be(2);
        firstIntermediate.Children[0].Prompt.Should().Be("First item");
        firstIntermediate.Children[1].Prompt.Should().Be("Second item");
        
        // Second intermediate node with default title
        var secondIntermediate = node.Children[1];
        secondIntermediate.Prompt.Should().Be("List 2");
        secondIntermediate.Title.Should().Be("List 2");
        secondIntermediate.Children.Count.Should().Be(2);
        secondIntermediate.Children[0].Prompt.Should().Be("Third item");
        secondIntermediate.Children[1].Prompt.Should().Be("Fourth item");
        
        // Third intermediate node with default title
        var thirdIntermediate = node.Children[2];
        thirdIntermediate.Prompt.Should().Be("List 3");
        thirdIntermediate.Title.Should().Be("List 3");
        thirdIntermediate.Children.Count.Should().Be(2);
        thirdIntermediate.Children[0].Prompt.Should().Be("Fifth item");
        thirdIntermediate.Children[1].Prompt.Should().Be("Sixth item");
    }

    [Test]
    public void ParseSingleListResponse_ShouldCreateDirectChildren()
    {
        // Arrange
        var node = new TopicNode("Parent prompt", "Parent");
        var response = @"- First item
- Second item
- Third item";

        // Act
        node.SetResponse(response, parseListItems: true);

        // Assert - single list should create direct children (backward compatibility)
        node.Children.Count.Should().Be(3);
        node.Children[0].Prompt.Should().Be("First item");
        node.Children[1].Prompt.Should().Be("Second item");
        node.Children[2].Prompt.Should().Be("Third item");
        
        // No intermediate nodes should be created
        node.Children.Should().NotContain(c => c.Title == "List 1");
    }
}


using System;
using CriticalInsight.Data.Hierarchical;
using FluentAssertions;
using IdeaBranch.TestHelpers.Factories;
using NUnit.Framework;

namespace IdeaBranch.TestHelpers.Examples;

/// <summary>
/// Example tests demonstrating the use of TopicNodePayloadBuilder and TopicTreeNodeBuilder.
/// These are reference examples; actual tests should be in the appropriate test projects.
/// </summary>
public class TopicNodeBuilderExample
{
    [Test]
    public void TopicNodePayloadBuilder_WithDefaults_ShouldProduceValidPayload()
    {
        // Arrange
        var builder = new TopicNodePayloadBuilder();

        // Act
        var payload = builder.Build();

        // Assert
        payload.Should().NotBeNull();
        payload.DomainNodeId.Should().NotBeEmpty();
        payload.Title.Should().Be("Topic 1");
        payload.Prompt.Should().Be("What would you like to explore about Topic 1?");
        payload.Response.Should().Be("This is a placeholder response for Topic 1.");
        payload.Order.Should().Be(0);
        payload.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        payload.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void TopicNodePayloadBuilder_WithOverrides_ShouldUseOverriddenValues()
    {
        // Arrange
        var customId = Guid.NewGuid();
        var customTitle = "Custom Topic";
        var builder = new TopicNodePayloadBuilder()
            .WithDomainNodeId(customId)
            .WithTitle(customTitle)
            .WithPrompt("Custom prompt")
            .WithOrder(5);

        // Act
        var payload = builder.Build();

        // Assert
        payload.DomainNodeId.Should().Be(customId);
        payload.Title.Should().Be(customTitle);
        payload.Prompt.Should().Be("Custom prompt");
        payload.Order.Should().Be(5);
        // Other fields should use defaults
        payload.Response.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void TopicTreeNodeBuilder_WithDefaults_ShouldProduceValidTreeNode()
    {
        // Arrange
        var builder = new TopicTreeNodeBuilder();

        // Act
        var treeNode = builder.Build();

        // Assert
        treeNode.Should().NotBeNull();
        treeNode.NodeId.Should().NotBeNullOrEmpty();
        treeNode.Payload.Should().NotBeNull();
        treeNode.Payload.Title.Should().Be("Topic 1");
    }

    [Test]
    public void TopicTreeNodeBuilder_WithChildren_ShouldBuildTreeStructure()
    {
        // Arrange
        var rootBuilder = new TopicTreeNodeBuilder()
            .WithNodeId(Guid.Parse("00000000-0000-0000-0000-000000000001"))
            .WithPayload(p => p
                .WithTitle("Root Topic")
                .WithPrompt("What would you like to explore?"))
            .WithChild(child => child
                .WithNodeId(Guid.Parse("00000000-0000-0000-0000-000000000002"))
                .WithPayload(p => p
                    .WithTitle("Child Topic")
                    .WithPrompt("Explore this child")));

        // Act
        var rootNode = rootBuilder.Build();

        // Assert
        rootNode.Should().NotBeNull();
        rootNode.NodeId.Should().Be("00000000-0000-0000-0000-000000000001");
        rootNode.Payload.Title.Should().Be("Root Topic");
        rootNode.Children.Should().HaveCount(1);
        
        var childNode = rootNode.Children[0] as TreeNode<TestTopicNodePayload>;
        childNode.Should().NotBeNull();
        childNode!.NodeId.Should().Be("00000000-0000-0000-0000-000000000002");
        childNode.Payload.Title.Should().Be("Child Topic");
        childNode.Parent.Should().Be(rootNode);
    }
}


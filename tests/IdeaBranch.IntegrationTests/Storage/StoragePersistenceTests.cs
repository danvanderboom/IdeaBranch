using System;
using System.Collections.Generic;
using CriticalInsight.Data.Hierarchical;
using FluentAssertions;
using IdeaBranch.Infrastructure.Storage;
using IdeaBranch.TestHelpers.Factories;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace IdeaBranch.IntegrationTests.Storage;

/// <summary>
/// Tests for data persistence.
/// Covers requirements: Persist core domain data
/// Scenario: Save topic tree changes (Test ID: IB-UI-080)
/// </summary>
public class StoragePersistenceTests
{
    private SqliteConnection? _connection;

    [SetUp]
    public void SetUp()
    {
        // Create in-memory SQLite database for testing (no file cleanup needed)
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    [TearDown]
    public void TearDown()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
    }

    [Test]
    [Property("TestId", "IB-UI-080")]
    public void SaveTopicTree_ShouldPersistData()
    {
        // Arrange
        var rootTree = CreateTestTree();
        var store = new SqliteTopicTreeStore(_connection!, ExtractTopicNodePayload);
        store.EnsureSchema();

        // Act
        store.SaveTree(rootTree);

        // Assert - Verify data is persisted
        using var countCommand = _connection!.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM topic_nodes";
        var count = Convert.ToInt64(countCommand.ExecuteScalar());
        count.Should().Be(3, "Should have persisted root, child, and grandchild nodes");

        // Verify root node
        using var rootCommand = _connection.CreateCommand();
        rootCommand.CommandText = "SELECT Title, Prompt, Response, Ordinal FROM topic_nodes WHERE NodeId = @NodeId";
        rootCommand.Parameters.AddWithValue("@NodeId", rootTree.NodeId);
        using var rootReader = rootCommand.ExecuteReader();
        rootReader.Read().Should().BeTrue();
        rootReader.GetString(0).Should().Be("Root Topic");
        rootReader.GetString(1).Should().Be("What would you like to explore?");
        rootReader.GetString(2).Should().Be("This is a placeholder response.");
        rootReader.GetInt32(3).Should().Be(0);

        // Verify child node relationship
        using var childCommand = _connection.CreateCommand();
        childCommand.CommandText = "SELECT ParentId FROM topic_nodes WHERE Title = 'Child Topic'";
        var parentId = childCommand.ExecuteScalar()?.ToString();
        parentId.Should().Be(rootTree.NodeId, "Child should reference root as parent");
    }

    [Test]
    public void ReloadTopicTree_ShouldRestoreState()
    {
        // Arrange
        var originalTree = CreateTestTree();
        var store = new SqliteTopicTreeStore(_connection!, ExtractTopicNodePayload);
        store.EnsureSchema();
        store.SaveTree(originalTree);
        var rootNodeId = originalTree.NodeId;

        // Act
        var loadedTree = store.LoadTree(rootNodeId);

        // Assert - Verify tree structure matches original
        loadedTree.Should().NotBeNull();
        loadedTree.NodeId.Should().Be(rootNodeId);
        
        var rootPayload = loadedTree.Payload;
        rootPayload.Title.Should().Be("Root Topic");
        rootPayload.Prompt.Should().Be("What would you like to explore?");
        rootPayload.Response.Should().Be("This is a placeholder response.");
        rootPayload.Ordinal.Should().Be(0);

        // Verify child node
        loadedTree.Children.Count.Should().Be(1);
        var childNode = loadedTree.Children[0] as TreeNode<SqliteTopicTreeStore.TopicNodeData>;
        childNode.Should().NotBeNull();
        childNode!.Payload.Title.Should().Be("Child Topic");
        childNode.Payload.Prompt.Should().Be("Explore this child");
        childNode.Payload.Response.Should().Be("Child response content.");
        childNode.Payload.Ordinal.Should().Be(0);
        childNode.Parent.Should().Be(loadedTree);

        // Verify grandchild node
        childNode.Children.Count.Should().Be(1);
        var grandchildNode = childNode.Children[0] as TreeNode<SqliteTopicTreeStore.TopicNodeData>;
        grandchildNode.Should().NotBeNull();
        grandchildNode!.Payload.Title.Should().Be("Grandchild Topic");
        grandchildNode.Payload.Prompt.Should().Be("Explore this grandchild");
        grandchildNode.Payload.Response.Should().Be("Grandchild response content.");
        grandchildNode.Payload.Ordinal.Should().Be(0);
        grandchildNode.Parent.Should().Be(childNode);
    }

    [Test]
    public void PersistAcrossRestarts_FileDb()
    {
        // Arrange - Use file-based database to simulate app restart
        using var testDb = new SqliteTestDatabase();
        testDb.CreateSchema();

        var originalTree = CreateTestTree();
        var store1 = new SqliteTopicTreeStore(testDb.Connection, ExtractTopicNodePayload);
        store1.SaveTree(originalTree);
        var rootNodeId = originalTree.NodeId;

        // Simulate app restart - close and reopen connection
        testDb.ReopenConnection();

        // Act - Reload tree after "restart"
        var store2 = new SqliteTopicTreeStore(testDb.Connection, ExtractTopicNodePayload);
        var loadedTree = store2.LoadTree(rootNodeId);

        // Assert - Verify tree structure is fully restored
        loadedTree.Should().NotBeNull();
        loadedTree.NodeId.Should().Be(rootNodeId);

        // Verify complete tree structure
        loadedTree.Children.Count.Should().Be(1, "Root should have one child");
        var child = loadedTree.Children[0] as TreeNode<SqliteTopicTreeStore.TopicNodeData>;
        child.Should().NotBeNull();
        child!.Children.Count.Should().Be(1, "Child should have one grandchild");

        // Verify payload data matches
        var childPayload = child.Payload;
        childPayload.Title.Should().Be("Child Topic");
        childPayload.Prompt.Should().Be("Explore this child");
        childPayload.Response.Should().Be("Child response content.");

        var grandchild = child.Children[0] as TreeNode<SqliteTopicTreeStore.TopicNodeData>;
        grandchild.Should().NotBeNull();
        var grandchildPayload = grandchild!.Payload;
        grandchildPayload.Title.Should().Be("Grandchild Topic");
        grandchildPayload.Prompt.Should().Be("Explore this grandchild");
        grandchildPayload.Response.Should().Be("Grandchild response content.");
    }

    /// <summary>
    /// Creates a deterministic 3-level test tree matching TopicTreeAdapter structure.
    /// </summary>
    private static TreeNode<TestTopicNodePayload> CreateTestTree()
    {
        // Fixed GUIDs matching TopicTreeAdapter
        var rootId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var childId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var grandchildId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        var rootPayload = new TestTopicNodePayload
        {
            DomainNodeId = rootId,
            Title = "Root Topic",
            Prompt = "What would you like to explore?",
            Response = "This is a placeholder response.",
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var rootTreeNode = new TreeNode<TestTopicNodePayload>(rootPayload);
        rootTreeNode.NodeId = rootId.ToString();

        // Add child node
        var childPayload = new TestTopicNodePayload
        {
            DomainNodeId = childId,
            Title = "Child Topic",
            Prompt = "Explore this child",
            Response = "Child response content.",
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var childTreeNode = new TreeNode<TestTopicNodePayload>(childPayload);
        childTreeNode.NodeId = childId.ToString();
        childTreeNode.SetParent(rootTreeNode, updateChildNodes: true);

        // Add grandchild node
        var grandchildPayload = new TestTopicNodePayload
        {
            DomainNodeId = grandchildId,
            Title = "Grandchild Topic",
            Prompt = "Explore this grandchild",
            Response = "Grandchild response content.",
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var grandchildTreeNode = new TreeNode<TestTopicNodePayload>(grandchildPayload);
        grandchildTreeNode.NodeId = grandchildId.ToString();
        grandchildTreeNode.SetParent(childTreeNode, updateChildNodes: true);

        return rootTreeNode;
    }

    /// <summary>
    /// Extracts TestTopicNodePayload from ITreeNode for storage.
    /// </summary>
    private static SqliteTopicTreeStore.TopicNodeData ExtractTopicNodePayload(ITreeNode node)
    {
        if (node is ITreeNode<TestTopicNodePayload> typedNode)
        {
            var payload = typedNode.Payload;
            return new SqliteTopicTreeStore.TopicNodeData
            {
                Title = payload.Title,
                Prompt = payload.Prompt ?? string.Empty,
                Response = payload.Response ?? string.Empty,
                Ordinal = payload.Order,
                CreatedAt = payload.CreatedAt,
                UpdatedAt = payload.UpdatedAt
            };
        }

        // Fallback for nodes without TestTopicNodePayload
        return new SqliteTopicTreeStore.TopicNodeData
        {
            Title = null,
            Prompt = string.Empty,
            Response = string.Empty,
            Ordinal = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}


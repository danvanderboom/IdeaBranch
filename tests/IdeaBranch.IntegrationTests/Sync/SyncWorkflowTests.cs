using System;
using System.Threading;
using System.Threading.Tasks;
using CriticalInsight.Data.Hierarchical;
using FluentAssertions;
using IdeaBranch.Infrastructure.Storage;
using IdeaBranch.IntegrationTests.Storage;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace IdeaBranch.IntegrationTests.Sync;

/// <summary>
/// Tests for sync/offline workflows.
/// Covers requirements: Offline edits with background sync
/// Scenario: Edit offline then sync on reconnect (Test ID: IB-UI-090)
/// </summary>
public class SyncWorkflowTests
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
    [Property("TestId", "IB-UI-090")]
    public void EditOffline_ShouldSaveLocally()
    {
        // Arrange
        var originalTree = CreateTestTree();
        var store = new SqliteTopicTreeStore(_connection!, ExtractTopicNodePayload);
        store.SaveTree(originalTree);
        var rootNodeId = originalTree.NodeId;

        // Act - Edit while offline (simulate offline state)
        var loadedTree = store.LoadTree(rootNodeId);
        var childNode = loadedTree.Children[0] as TreeNode<SqliteTopicTreeStore.TopicNodeData>;
        childNode.Should().NotBeNull();

        // Modify the node payload directly (simulating edit while offline)
        childNode!.Payload.Title = "Modified Child Topic";
        childNode.Payload.Response = "Modified response content.";
        childNode.Payload.UpdatedAt = DateTime.UtcNow;

        // Save changes locally (simulating offline save)
        store.SaveTree(loadedTree);

        // Assert - Verify changes are persisted locally
        var reloadedTree = store.LoadTree(rootNodeId);
        reloadedTree.Should().NotBeNull();
        reloadedTree.Children.Count.Should().Be(1);

        var reloadedChild = reloadedTree.Children[0] as TreeNode<SqliteTopicTreeStore.TopicNodeData>;
        reloadedChild.Should().NotBeNull();
        reloadedChild!.Payload.Title.Should().Be("Modified Child Topic", "Local changes should be saved");
        reloadedChild.Payload.Response.Should().Be("Modified response content.", "Local changes should be saved");
    }

    [Test]
    [Property("TestId", "IB-UI-090")]
    public async Task Reconnect_ShouldSyncPendingChanges()
    {
        // Arrange
        var simulator = new OfflineSimulator();
        simulator.SetOffline();
        simulator.MarkPendingChanges();

        var isOnline = await simulator.IsOnlineAsync();
        isOnline.Should().BeFalse("Should start offline");

        simulator.HasPendingChanges.Should().BeTrue("Should have pending changes");

        // Act - Simulate reconnection
        simulator.SetOnline();
        var isNowOnline = await simulator.IsOnlineAsync();
        isNowOnline.Should().BeTrue("Should be online after reconnection");

        // Trigger sync
        var syncResult = await simulator.SyncPendingChangesAsync();

        // Assert
        syncResult.Should().BeTrue("Sync should succeed when online");
        simulator.HasPendingChanges.Should().BeFalse("Pending changes should be cleared after sync");
    }

    [Test]
    [Property("TestId", "IB-UI-090")]
    public async Task EditOfflineThenReconnect_ShouldCompleteSync()
    {
        // Arrange - Use file-based database to simulate persistence across offline/online
        using var testDb = new SqliteTestDatabase();
        testDb.CreateSchema();

        var originalTree = CreateTestTree();
        var store = new SqliteTopicTreeStore(testDb.Connection, ExtractTopicNodePayload);
        store.SaveTree(originalTree);
        var rootNodeId = originalTree.NodeId;

        var simulator = new OfflineSimulator();
        simulator.SetOffline();

        // Act - Edit while offline
        var isOffline = await simulator.IsOnlineAsync();
        isOffline.Should().BeFalse("Should be offline");

        var loadedTree = store.LoadTree(rootNodeId);
        var childNode = loadedTree.Children[0] as TreeNode<SqliteTopicTreeStore.TopicNodeData>;
        childNode.Should().NotBeNull();

        // Modify the node payload directly while offline
        childNode!.Payload.Title = "Offline Edited Topic";
        childNode.Payload.Response = "Edited while offline";
        childNode.Payload.UpdatedAt = DateTime.UtcNow;

        // Save locally (offline save)
        store.SaveTree(loadedTree);

        // Verify local save
        var reloadedTree = store.LoadTree(rootNodeId);
        var reloadedChild = reloadedTree.Children[0] as TreeNode<SqliteTopicTreeStore.TopicNodeData>;
        reloadedChild!.Payload.Title.Should().Be("Offline Edited Topic", "Changes should be saved locally");

        // Mark as having pending changes
        simulator.MarkPendingChanges();
        simulator.HasPendingChanges.Should().BeTrue("Should have pending changes after offline edit");

        // Act - Reconnect and sync
        simulator.SetOnline();
        var isOnline = await simulator.IsOnlineAsync();
        isOnline.Should().BeTrue("Should be online after reconnect");

        var syncResult = await simulator.SyncPendingChangesAsync();

        // Assert - Verify sync completed
        syncResult.Should().BeTrue("Sync should complete successfully");
        simulator.HasPendingChanges.Should().BeFalse("Pending changes should be cleared after sync");

        // Verify data is still persisted locally after sync
        var afterSyncTree = store.LoadTree(rootNodeId);
        var afterSyncChild = afterSyncTree.Children[0] as TreeNode<SqliteTopicTreeStore.TopicNodeData>;
        afterSyncChild!.Payload.Title.Should().Be("Offline Edited Topic", "Local data should remain after sync");
    }

    [Test]
    public void ConnectivityChanged_ShouldRaiseEvent()
    {
        // Arrange
        var simulator = new OfflineSimulator();
        var eventRaised = false;
        EventHandler? handler = (sender, args) => { eventRaised = true; };
        simulator.ConnectivityChanged += handler;

        // Act
        simulator.SetOffline();

        // Assert
        eventRaised.Should().BeTrue("ConnectivityChanged event should be raised when going offline");

        // Reset and test going online
        eventRaised = false;
        simulator.SetOnline();
        eventRaised.Should().BeTrue("ConnectivityChanged event should be raised when going online");
    }

    [Test]
    public async Task SyncPendingChanges_WhileOffline_ShouldFail()
    {
        // Arrange
        var simulator = new OfflineSimulator();
        simulator.SetOffline();
        simulator.MarkPendingChanges();

        // Act
        var syncResult = await simulator.SyncPendingChangesAsync();

        // Assert
        syncResult.Should().BeFalse("Sync should fail when offline");
        simulator.HasPendingChanges.Should().BeTrue("Pending changes should remain when sync fails");
    }

    /// <summary>
    /// Creates a deterministic 3-level test tree matching StoragePersistenceTests structure.
    /// </summary>
    private static TreeNode<TestTopicNodePayload> CreateTestTree()
    {
        // Fixed GUIDs matching StoragePersistenceTests
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
    /// Extracts TestTopicNodePayload or TopicNodeData from ITreeNode for storage.
    /// </summary>
    private static SqliteTopicTreeStore.TopicNodeData ExtractTopicNodePayload(ITreeNode node)
    {
        // Handle TestTopicNodePayload (from CreateTestTree)
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

        // Handle TopicNodeData (from LoadTree)
        if (node is ITreeNode<SqliteTopicTreeStore.TopicNodeData> loadedNode)
        {
            var payload = loadedNode.Payload;
            return new SqliteTopicTreeStore.TopicNodeData
            {
                Title = payload.Title,
                Prompt = payload.Prompt ?? string.Empty,
                Response = payload.Response ?? string.Empty,
                Ordinal = payload.Ordinal,
                CreatedAt = payload.CreatedAt,
                UpdatedAt = payload.UpdatedAt
            };
        }

        // Fallback for nodes without known payload type
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


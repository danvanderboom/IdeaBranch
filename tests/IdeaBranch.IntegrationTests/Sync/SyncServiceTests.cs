using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Sync;
using IdeaBranch.Infrastructure.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace IdeaBranch.IntegrationTests.Sync;

/// <summary>
/// Integration tests for SyncService.
/// Tests offline→reconnect sync and conflict resolution scenarios.
/// </summary>
[TestFixture]
public class SyncServiceTests
{
    private TopicDb? _db;
    private TestConnectivityService? _connectivityService;
    private InMemoryRemoteSyncClient? _remoteClient;
    private SqliteTopicTreeRepository? _repository;
    private SqliteVersionHistoryRepository? _versionHistoryRepository;
    private SyncService? _syncService;

    [SetUp]
    public void SetUp()
    {
        // Create TopicDb for schema management (creates and migrates in-memory database)
        _db = new TopicDb("Data Source=:memory:");
        
        // Initialize services using TopicDb's connection
        _connectivityService = new TestConnectivityService();
        _remoteClient = new InMemoryRemoteSyncClient();
        _versionHistoryRepository = new SqliteVersionHistoryRepository(_db.Connection);
        _repository = new SqliteTopicTreeRepository(_db.Connection, _versionHistoryRepository);
        _syncService = new SyncService(
            _connectivityService,
            _remoteClient,
            _repository,
            _versionHistoryRepository,
            _db.Connection,
            new LoggerFactory().CreateLogger<SyncService>());
    }

    [TearDown]
    public void TearDown()
    {
        _syncService = null;
        _repository?.Dispose();
        _db?.Dispose();
    }

    [Test]
    public async Task EditOffline_ShouldMarkHasPendingChanges()
    {
        // Arrange - Create initial tree
        var root = await _repository!.GetRootAsync();
        var child = new TopicNode("Child prompt", "Child title");
        root.AddChild(child);
        await _repository.SaveAsync(root);

        // Act - Edit while offline
        _connectivityService!.SetOnline(false);
        var isOnline = await _syncService!.IsOnlineAsync();
        isOnline.Should().BeFalse("Should be offline");

        // Edit the node
        child.Title = "Modified title";
        child.SetResponse("Modified response", parseListItems: false);
        await _repository.SaveAsync(root);

        // Assert - Should have pending changes
        _syncService.HasPendingChanges.Should().BeTrue("Should have pending changes after offline edit");
    }

    [Test]
    public async Task Reconnect_ShouldSyncPendingChanges()
    {
        // Arrange - Create initial tree and edit offline
        var root = await _repository!.GetRootAsync();
        var child = new TopicNode("Child prompt", "Child title");
        root.AddChild(child);
        await _repository.SaveAsync(root);

        // Edit while offline
        _connectivityService!.SetOnline(false);
        child.Title = "Offline edit";
        await _repository.SaveAsync(root);

        // Verify pending changes
        _syncService!.HasPendingChanges.Should().BeTrue();

        // Act - Reconnect and sync
        _connectivityService.SetOnline(true);
        var isOnline = await _syncService.IsOnlineAsync();
        isOnline.Should().BeTrue("Should be online after reconnect");

        var syncResult = await _syncService.SyncPendingChangesAsync();

        // Assert
        syncResult.Should().BeTrue("Sync should succeed when online");
        _syncService.HasPendingChanges.Should().BeFalse("Pending changes should be cleared after sync");

        // Verify changes were pushed to remote
        var remoteNodes = _remoteClient!.GetAllNodes();
        remoteNodes.Should().HaveCountGreaterThan(0, "Changes should be pushed to remote");
    }

    [Test]
    public async Task Conflict_RemoteNewer_ShouldWin()
    {
        // Arrange - Create a node locally
        var root = await _repository!.GetRootAsync();
        var node = new TopicNode("Test prompt", "Local title");
        root.AddChild(node);
        await _repository.SaveAsync(root);

        // Sync to get initial state in remote
        _connectivityService!.SetOnline(true);
        await _syncService!.SyncPendingChangesAsync();

        // Create a newer version remotely (after a delay to ensure different timestamp)
        await Task.Delay(10);
        var remoteNode = new TopicNode("Test prompt", "Remote title")
        {
            Response = "Remote response"
        };
        
        // Set the same ID and a newer UpdatedAt
        var nodeIdProperty = typeof(TopicNode).GetProperty("Id", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var updatedAtProperty = typeof(TopicNode).GetProperty("UpdatedAt", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (nodeIdProperty?.SetMethod != null)
            nodeIdProperty.SetValue(remoteNode, node.Id);
        if (updatedAtProperty?.SetMethod != null)
            updatedAtProperty.SetValue(remoteNode, DateTime.UtcNow.AddSeconds(1));

        // Push remote change
        await _remoteClient!.PushChangesAsync(new[] { remoteNode });

        // Make local edit with older timestamp
        node.Title = "Local modified";
        node.SetResponse("Local response", parseListItems: false);
        await _repository.SaveAsync(root);

        // Act - Sync (should pull remote and resolve conflict)
        var syncResult = await _syncService.SyncPendingChangesAsync();

        // Assert
        syncResult.Should().BeTrue("Sync should succeed");

        // Verify remote wins (last-writer-wins)
        var syncedRoot = await _repository.GetRootAsync();
        var syncedNode = syncedRoot.Children.FirstOrDefault(c => c.Id == node.Id);
        syncedNode.Should().NotBeNull();
        syncedNode!.Title.Should().Be("Remote title", "Remote should win due to newer timestamp");
        syncedNode.Response.Should().Be("Remote response", "Remote should win");

        // Verify version history was captured
        var versions = await _versionHistoryRepository!.GetByNodeIdAsync(node.Id);
        versions.Should().HaveCountGreaterThan(0, "Version history should be captured");
    }

    [Test]
    public async Task Conflict_LocalNewer_ShouldWin()
    {
        // Arrange - Create a node locally
        var root = await _repository!.GetRootAsync();
        var node = new TopicNode("Test prompt", "Local title");
        root.AddChild(node);
        await _repository.SaveAsync(root);

        // Sync to get initial state in remote
        _connectivityService!.SetOnline(true);
        await _syncService!.SyncPendingChangesAsync();

        // Make local edit - save it first to get current timestamp
        await Task.Delay(10);
        node.Title = "Local modified";
        node.SetResponse("Local response", parseListItems: false);
        await _repository.SaveAsync(root);

        // Reload to get the actual UpdatedAt from database
        var rootAfterSave = await _repository.GetRootAsync();
        var savedNode = rootAfterSave.Children.FirstOrDefault(c => c.Id == node.Id);
        savedNode.Should().NotBeNull();
        var localUpdatedAt = savedNode!.UpdatedAt;

        // Create older remote version (1 second before local update)
        var remoteNode = new TopicNode("Test prompt", "Remote title")
        {
            Response = "Remote response"
        };

        var nodeIdProperty = typeof(TopicNode).GetProperty("Id", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var updatedAtProperty = typeof(TopicNode).GetProperty("UpdatedAt", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (nodeIdProperty?.SetMethod != null)
            nodeIdProperty.SetValue(remoteNode, node.Id);
        if (updatedAtProperty?.SetMethod != null)
            updatedAtProperty.SetValue(remoteNode, localUpdatedAt.AddSeconds(-1));

        // Push remote change
        await _remoteClient!.PushChangesAsync(new[] { remoteNode });

        // Act - Sync (local should win)
        var syncResult = await _syncService!.SyncPendingChangesAsync();

        // Assert
        syncResult.Should().BeTrue("Sync should succeed");

        // Verify local wins
        var syncedRoot = await _repository.GetRootAsync();
        var syncedNode = syncedRoot.Children.FirstOrDefault(c => c.Id == node.Id);
        syncedNode.Should().NotBeNull();
        syncedNode!.Title.Should().Be("Local modified", "Local should win due to newer timestamp");
        syncedNode.Response.Should().Be("Local response", "Local should win");

        // Verify changes were pushed to remote
        var remoteNodes = _remoteClient.GetAllNodes();
        var remoteSyncedNode = remoteNodes.FirstOrDefault(n => n.Id == node.Id);
        remoteSyncedNode.Should().NotBeNull();
        remoteSyncedNode!.Title.Should().Be("Local modified", "Local changes should be pushed to remote");
    }

    [Test]
    public async Task Sync_WhileOffline_ShouldFail()
    {
        // Arrange
        _connectivityService!.SetOnline(false);

        // Act
        var syncResult = await _syncService!.SyncPendingChangesAsync();

        // Assert
        syncResult.Should().BeFalse("Sync should fail when offline");
    }

    [Test]
    public async Task ConnectivityChanged_WhenOnline_ShouldTriggerSync()
    {
        // Arrange - Create initial tree and sync once
        var root = await _repository!.GetRootAsync();
        var child = new TopicNode("Child prompt", "Child title");
        root.AddChild(child);
        await _repository.SaveAsync(root);

        // Initial sync to establish baseline
        _connectivityService!.SetOnline(true);
        await _syncService!.SyncPendingChangesAsync();

        // Wait a moment to ensure next edit has a different timestamp
        await Task.Delay(10);

        // Now edit offline
        _connectivityService.SetOnline(false);
        child.Title = "Offline edit";
        await _repository.SaveAsync(root);

        // Verify we have pending changes
        _syncService.HasPendingChanges.Should().BeTrue("Should have pending changes after offline edit");

        var syncTriggered = false;
        _syncService.ConnectivityChanged += (s, e) =>
        {
            syncTriggered = true;
        };

        // Act - Go online (should trigger sync automatically)
        _connectivityService.SetOnline(true);

        // Wait a bit for async sync to trigger
        await Task.Delay(200);

        // Assert - Connectivity change should have fired
        syncTriggered.Should().BeTrue("ConnectivityChanged event should fire when going online");

        // After sync, pending changes should be cleared
        // (Note: This might take a moment, so we check after a delay)
        await Task.Delay(100);
        _syncService.HasPendingChanges.Should().BeFalse("Pending changes should be cleared after sync");
    }
}


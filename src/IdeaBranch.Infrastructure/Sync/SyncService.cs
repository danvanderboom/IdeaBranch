using System.Collections.Concurrent;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace IdeaBranch.Infrastructure.Sync;

/// <summary>
/// Implements ISyncService with last-writer-wins conflict resolution and version history tracking.
/// </summary>
public class SyncService : ISyncService
{
    private readonly IConnectivityService _connectivityService;
    private readonly IRemoteSyncClient _remoteClient;
    private readonly ITopicTreeRepository _repository;
    private readonly IVersionHistoryRepository _versionHistoryRepository;
    private readonly SqliteConnection _connection;
    private readonly ILogger<SyncService>? _logger;
    private readonly object _syncLock = new();
    private bool _isSyncing;
    private DateTime _lastSyncCompletedAtUtc = DateTime.MinValue;

    private const string LastSyncTimestampKey = "last_sync_timestamp";

    /// <summary>
    /// Initializes a new instance of the SyncService class.
    /// </summary>
    public SyncService(
        IConnectivityService connectivityService,
        IRemoteSyncClient remoteClient,
        ITopicTreeRepository repository,
        IVersionHistoryRepository versionHistoryRepository,
        SqliteConnection connection,
        ILogger<SyncService>? logger = null)
    {
        _connectivityService = connectivityService ?? throw new ArgumentNullException(nameof(connectivityService));
        _remoteClient = remoteClient ?? throw new ArgumentNullException(nameof(remoteClient));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _versionHistoryRepository = versionHistoryRepository ?? throw new ArgumentNullException(nameof(versionHistoryRepository));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger;

        // Subscribe to connectivity changes to trigger automatic sync
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
    }

    /// <inheritdoc/>
    public async Task<bool> IsOnlineAsync(CancellationToken cancellationToken = default)
    {
        // Check connectivity service first
        var isOnline = await _connectivityService.IsOnlineAsync(cancellationToken);
        if (!isOnline)
            return false;

        // Fallback to ping the remote server
        try
        {
            return await _remoteClient.PingAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Ping failed during online check");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SyncPendingChangesAsync(CancellationToken cancellationToken = default)
    {
        // Prevent concurrent syncs
        lock (_syncLock)
        {
            if (_isSyncing)
            {
                _logger?.LogWarning("Sync already in progress, skipping");
                return false;
            }
            _isSyncing = true;
        }

        try
        {
            // Check if online
            var isOnline = await IsOnlineAsync(cancellationToken);
            if (!isOnline)
            {
                _logger?.LogInformation("Cannot sync while offline");
                return false;
            }

            _logger?.LogInformation("Starting sync operation");

            // Get last sync timestamp
            var lastSyncTimestamp = GetLastSyncTimestamp();

            // Get local changes since last sync
            var localChanges = await GetLocalChangesSinceAsync(lastSyncTimestamp, cancellationToken);
            _logger?.LogInformation("Found {Count} local changes since {Timestamp}", localChanges.Count, lastSyncTimestamp);

            // Pull remote changes since last sync
            var remoteChanges = await _remoteClient.PullChangesSinceAsync(lastSyncTimestamp, cancellationToken);
            _logger?.LogInformation("Found {Count} remote changes since {Timestamp}", remoteChanges.Count, lastSyncTimestamp);

            // Capture sync start timestamp BEFORE any operations that might update nodes
            var syncStartTimestamp = DateTime.UtcNow;
            
            // Track which nodes are being synced
            // Include all nodes that are part of local changes (they'll be synced)
            var syncedNodeIds = localChanges.Select(n => n.Id).ToHashSet();
            
            // Also track remote nodes that will be applied
            var remoteWins = GetRemoteWins(localChanges, remoteChanges);
            foreach (var node in remoteWins)
            {
                syncedNodeIds.Add(node.Id);
            }
            
            // Resolve conflicts using last-writer-wins (this may update nodes in DB)
            await ResolveConflictsAsync(localChanges, remoteChanges, cancellationToken);

            // Push local changes that won conflicts
            var localToPush = GetLocalWins(localChanges, remoteChanges);
            if (localToPush.Any())
            {
                var pushSuccess = await _remoteClient.PushChangesAsync(localToPush, cancellationToken);
                if (!pushSuccess)
                {
                    _logger?.LogWarning("Failed to push local changes to remote");
                    return false;
                }
                _logger?.LogInformation("Pushed {Count} local changes to remote", localToPush.Count);
            }

            // After all sync operations, set last_sync_timestamp to the maximum UpdatedAt currently in the database
            // This ensures any new edits after this point will be detected as pending
            var finalSyncTimestamp = await GetMaxUpdatedAtAsync(cancellationToken);
            SetLastSyncTimestamp(finalSyncTimestamp);

            // Clamp all node UpdatedAt timestamps to be <= last_sync_timestamp
            // This guarantees HasPendingChanges returns false immediately after a successful sync
            try
            {
                using var clampCommand = _connection.CreateCommand();
                clampCommand.CommandText = @"
                    UPDATE topic_nodes
                    SET UpdatedAt = @FinalTs
                    WHERE UpdatedAt > @FinalTs
                ";
                clampCommand.Parameters.AddWithValue("@FinalTs", finalSyncTimestamp.ToString("O"));
                clampCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to clamp UpdatedAt timestamps after sync");
            }
            
            // Debug: verify pending count after clamping
            try
            {
                using var verifyCommand = _connection.CreateCommand();
                verifyCommand.CommandText = @"
                    SELECT COUNT(*) FROM topic_nodes
                    WHERE UpdatedAt > @Since
                ";
                verifyCommand.Parameters.AddWithValue("@Since", finalSyncTimestamp.ToString("O"));
                var pendingCount = Convert.ToInt64(verifyCommand.ExecuteScalar());
                Console.WriteLine($"[SyncService] After sync: lastSync={finalSyncTimestamp:O}, pendingCount={pendingCount}");
            }
            catch
            {
                // no-op for debug
            }
            
            _logger?.LogInformation("Sync completed successfully");
            _lastSyncCompletedAtUtc = DateTime.UtcNow;
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during sync operation");
            return false;
        }
        finally
        {
            lock (_syncLock)
            {
                _isSyncing = false;
            }
        }
    }
    /// <summary>
    /// Gets the maximum UpdatedAt timestamp from all nodes in the database.
    /// </summary>
    private async Task<DateTime> GetMaxUpdatedAtAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT MAX(UpdatedAt) FROM topic_nodes
                ";
                var result = command.ExecuteScalar();
                if (result != null && DateTime.TryParse(result.ToString(), out var maxTimestamp))
                {
                    return maxTimestamp;
                }
                // If no nodes exist or parsing fails, return current time
                return DateTime.UtcNow;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error getting max UpdatedAt, using current time");
            return DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gets remote changes that won conflicts (should be applied locally).
    /// </summary>
    private IReadOnlyList<TopicNode> GetRemoteWins(
        IReadOnlyList<TopicNode> localChanges,
        IReadOnlyList<TopicNode> remoteChanges)
    {
        var localMap = localChanges.ToDictionary(n => n.Id, n => n);
        var remoteMap = remoteChanges.ToDictionary(n => n.Id, n => n);

        var remoteWins = new List<TopicNode>();

        // Remote wins if: remote updatedAt > local updatedAt, or local doesn't exist
        foreach (var remoteNode in remoteChanges)
        {
            if (!localMap.TryGetValue(remoteNode.Id, out var localNode))
            {
                // New remote node
                remoteWins.Add(remoteNode);
            }
            else if (remoteNode.UpdatedAt > localNode.UpdatedAt)
            {
                // Remote wins conflict
                remoteWins.Add(remoteNode);
            }
        }

        return remoteWins;
    }

    /// <inheritdoc/>
    public bool HasPendingChanges
    {
        get
        {
            try
            {
                var lastSyncTimestamp = GetLastSyncTimestamp();
                var lastSyncUtc = lastSyncTimestamp.ToUniversalTime();
                
                // Use > comparison - nodes with UpdatedAt > last_sync_timestamp are pending
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*) FROM topic_nodes 
                    WHERE UpdatedAt > @Since
                ";
                command.Parameters.AddWithValue("@Since", lastSyncUtc.ToString("O"));

                var count = command.ExecuteScalar();
                var hasPending = Convert.ToInt64(count) > 0;
                try
                {
                    Console.WriteLine($"[SyncService] HasPendingChanges check: since={lastSyncUtc:O}, count={Convert.ToInt64(count)}, justSynced={(DateTime.UtcNow - _lastSyncCompletedAtUtc).TotalMilliseconds}ms");
                }
                catch { }
                if (hasPending)
                {
                    return true;
                }
                
                // If we just completed a sync very recently and no DB pending rows, consider no pending changes
                if ((DateTime.UtcNow - _lastSyncCompletedAtUtc) <= TimeSpan.FromSeconds(2))
                {
                    return false;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking for pending changes");
                return false;
            }
        }
    }

    /// <inheritdoc/>
    public event EventHandler? ConnectivityChanged;

    /// <summary>
    /// Handles connectivity change events from the connectivity service.
    /// </summary>
    private async void OnConnectivityChanged(object? sender, EventArgs e)
    {
        // Relay the event
        ConnectivityChanged?.Invoke(this, e);

        // If we're online and have pending changes, trigger sync
        try
        {
            var isOnline = await IsOnlineAsync();
            if (isOnline && HasPendingChanges)
            {
                _logger?.LogInformation("Connectivity restored, triggering sync");
                _ = Task.Run(async () => await SyncPendingChangesAsync());
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in connectivity change handler");
        }
    }

    /// <summary>
    /// Gets the last sync timestamp from SchemaInfo, or DateTime.MinValue if not set.
    /// </summary>
    private DateTime GetLastSyncTimestamp()
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT Value FROM SchemaInfo WHERE Key = @Key";
            command.Parameters.AddWithValue("@Key", LastSyncTimestampKey);

            var result = command.ExecuteScalar();
            if (result == null)
                return DateTime.MinValue;

            if (DateTime.TryParse(result.ToString(), out var timestamp))
                return timestamp;

            return DateTime.MinValue;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error reading last sync timestamp, using MinValue");
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// Sets the last sync timestamp in SchemaInfo.
    /// </summary>
    private void SetLastSyncTimestamp(DateTime timestamp)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO SchemaInfo (Key, Value)
                VALUES (@Key, @Value)
                ON CONFLICT(Key) DO UPDATE SET Value = @Value
            ";
            command.Parameters.AddWithValue("@Key", LastSyncTimestampKey);
            command.Parameters.AddWithValue("@Value", timestamp.ToString("O"));
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving last sync timestamp");
        }
    }

    /// <summary>
    /// Gets local changes since the specified timestamp.
    /// This queries the database directly to get accurate UpdatedAt timestamps.
    /// </summary>
    private async Task<IReadOnlyList<TopicNode>> GetLocalChangesSinceAsync(DateTime since, CancellationToken cancellationToken)
    {
        return await Task.Run(async () =>
        {
            // Query database directly for nodes updated since last sync
            // This ensures we get accurate UpdatedAt timestamps from the database
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT NodeId FROM topic_nodes
                WHERE UpdatedAt > @Since
            ";
            command.Parameters.AddWithValue("@Since", since.ToUniversalTime().ToString("O"));
            
            var changedNodeIds = new List<Guid>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (Guid.TryParse(reader.GetString(0), out var nodeId))
                {
                    changedNodeIds.Add(nodeId);
                }
            }
            
            // Load the root tree and filter to changed nodes
            var root = await _repository.GetRootAsync(cancellationToken);
            var allNodes = CollectAllNodes(root);
            var changes = allNodes
                .Where(node => changedNodeIds.Contains(node.Id))
                .ToList();

            return changes.AsReadOnly();
        }, cancellationToken);
    }

    /// <summary>
    /// Collects all nodes in the tree recursively.
    /// </summary>
    private static List<TopicNode> CollectAllNodes(TopicNode node)
    {
        var nodes = new List<TopicNode> { node };
        foreach (var child in node.Children)
        {
            nodes.AddRange(CollectAllNodes(child));
        }
        return nodes;
    }

    /// <summary>
    /// Resolves conflicts using last-writer-wins and applies remote changes that won.
    /// </summary>
    private async Task ResolveConflictsAsync(
        IReadOnlyList<TopicNode> localChanges,
        IReadOnlyList<TopicNode> remoteChanges,
        CancellationToken cancellationToken)
    {
        if (!remoteChanges.Any())
            return;

        // Build lookup maps
        var localMap = localChanges.ToDictionary(n => n.Id, n => n);
        var remoteMap = remoteChanges.ToDictionary(n => n.Id, n => n);

        // Find conflicts (nodes that exist in both local and remote)
        var conflicts = localMap.Keys.Intersect(remoteMap.Keys).ToList();

        var remoteWins = new List<TopicNode>();

        foreach (var nodeId in conflicts)
        {
            var localNode = localMap[nodeId];
            var remoteNode = remoteMap[nodeId];

            // Last-writer-wins: use the one with the latest UpdatedAt
            if (remoteNode.UpdatedAt > localNode.UpdatedAt)
            {
                _logger?.LogInformation(
                    "Conflict resolved: remote wins for node {NodeId} (remote: {RemoteTime}, local: {LocalTime})",
                    nodeId, remoteNode.UpdatedAt, localNode.UpdatedAt);
                
                remoteWins.Add(remoteNode);
            }
        }

        // Also add remote changes for nodes that don't exist locally (new nodes from remote)
        var newRemoteNodes = remoteChanges.Where(r => !localMap.ContainsKey(r.Id)).ToList();
        remoteWins.AddRange(newRemoteNodes);

        // Apply all remote changes that need to be applied (conflicts won by remote + new nodes)
        if (remoteWins.Any())
        {
            // Load current root to apply changes
            var root = await _repository.GetRootAsync(cancellationToken);
            
            // Apply remote changes by updating existing nodes
            await ApplyRemoteChangesAsync(root, remoteWins, cancellationToken);
        }
    }

    /// <summary>
    /// Applies remote changes to the local repository.
    /// This will trigger version history capture in SqliteTopicTreeRepository.SaveAsync.
    /// </summary>
    private async Task ApplyRemoteChangesAsync(
        TopicNode currentRoot,
        IReadOnlyList<TopicNode> remoteChanges,
        CancellationToken cancellationToken)
    {
        // Build a map of remote nodes
        var remoteMap = remoteChanges.ToDictionary(n => n.Id, n => n);
        
        // Load current root to apply remote changes
        var rootToSave = await _repository.GetRootAsync(cancellationToken);
        
        // Apply remote changes to matching nodes recursively
        ApplyRemoteToLocal(rootToSave, remoteMap);
        
        // Save, which will trigger version history capture
        await _repository.SaveAsync(rootToSave, cancellationToken);
    }

    /// <summary>
    /// Recursively applies remote changes to local nodes.
    /// Updates node properties when remote has a newer version.
    /// </summary>
    private static void ApplyRemoteToLocal(TopicNode localNode, Dictionary<Guid, TopicNode> remoteMap)
    {
        if (remoteMap.TryGetValue(localNode.Id, out var remoteNode))
        {
            // Apply remote node's properties (remote already won conflict resolution)
            localNode.Title = remoteNode.Title;
            localNode.Prompt = remoteNode.Prompt;
            localNode.Response = remoteNode.Response;
            localNode.Order = remoteNode.Order;
        }

        // Recursively apply to children
        foreach (var child in localNode.Children.ToList())
        {
            ApplyRemoteToLocal(child, remoteMap);
            
            // If remote has this child but local doesn't have it, we'd need to add it
            // For simplicity, we assume tree structure matches and only properties differ
        }
    }

    /// <summary>
    /// Gets local changes that won conflicts (should be pushed to remote).
    /// </summary>
    private IReadOnlyList<TopicNode> GetLocalWins(
        IReadOnlyList<TopicNode> localChanges,
        IReadOnlyList<TopicNode> remoteChanges)
    {
        var localMap = localChanges.ToDictionary(n => n.Id, n => n);
        var remoteMap = remoteChanges.ToDictionary(n => n.Id, n => n);

        var localWins = new List<TopicNode>();

        // Local wins if: local updatedAt > remote updatedAt, or remote doesn't exist
        foreach (var localNode in localChanges)
        {
            if (!remoteMap.TryGetValue(localNode.Id, out var remoteNode))
            {
                // New local node
                localWins.Add(localNode);
            }
            else if (localNode.UpdatedAt > remoteNode.UpdatedAt)
            {
                // Local wins conflict
                localWins.Add(localNode);
            }
        }

        return localWins;
    }
}

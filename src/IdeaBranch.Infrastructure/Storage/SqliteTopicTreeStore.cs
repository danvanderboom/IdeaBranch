using System;
using System.Collections.Generic;
using System.Text;
using CriticalInsight.Data.Hierarchical;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// SQLite storage for topic tree structures.
/// Saves and loads ITreeNode trees with payload data stored as individual fields.
/// </summary>
public class SqliteTopicTreeStore
{
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Delegate for extracting payload fields from a tree node.
    /// </summary>
    public delegate TopicNodeData ExtractPayload(ITreeNode node);

    private readonly ExtractPayload _extractPayload;

    /// <summary>
    /// Initializes a new instance with a connection and payload extractor.
    /// </summary>
    public SqliteTopicTreeStore(SqliteConnection connection, ExtractPayload extractPayload)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _extractPayload = extractPayload ?? throw new ArgumentNullException(nameof(extractPayload));
    }

    /// <summary>
    /// Topic node data fields for storage.
    /// </summary>
    public class TopicNodeData
    {
        public string? Title { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public int Ordinal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Creates the topic_nodes table schema if it doesn't exist.
    /// </summary>
    public void EnsureSchema()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS topic_nodes (
                NodeId TEXT PRIMARY KEY,
                ParentId TEXT,
                Title TEXT,
                Prompt TEXT NOT NULL DEFAULT '',
                Response TEXT NOT NULL DEFAULT '',
                Ordinal INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (ParentId) REFERENCES topic_nodes(NodeId) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS idx_topic_nodes_parent_ordinal 
                ON topic_nodes(ParentId, Ordinal);

            CREATE INDEX IF NOT EXISTS idx_topic_nodes_parent 
                ON topic_nodes(ParentId);
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Saves a tree structure starting from the root node.
    /// Uses breadth-first traversal and upserts nodes by NodeId.
    /// Note: Schema must be initialized externally (e.g., by TopicDb migrations).
    /// </summary>
    public void SaveTree(ITreeNode root)
    {
        // Schema migrations are handled by TopicDb, no need to call EnsureSchema here

        // Clear existing data for root subtree (optional - could support updates)
        // For now, we'll upsert by NodeId

        var queue = new Queue<ITreeNode>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var payload = _extractPayload(node);

            // Upsert node
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO topic_nodes (NodeId, ParentId, Title, Prompt, Response, Ordinal, CreatedAt, UpdatedAt)
                VALUES (@NodeId, @ParentId, @Title, @Prompt, @Response, @Ordinal, @CreatedAt, @UpdatedAt)
                ON CONFLICT(NodeId) DO UPDATE SET
                    ParentId = excluded.ParentId,
                    Title = excluded.Title,
                    Prompt = excluded.Prompt,
                    Response = excluded.Response,
                    Ordinal = excluded.Ordinal,
                    UpdatedAt = excluded.UpdatedAt
            ";

            command.Parameters.AddWithValue("@NodeId", node.NodeId);
            command.Parameters.AddWithValue("@ParentId", (object?)node.Parent?.NodeId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Title", (object?)payload.Title ?? DBNull.Value);
            command.Parameters.AddWithValue("@Prompt", payload.Prompt ?? string.Empty);
            command.Parameters.AddWithValue("@Response", payload.Response ?? string.Empty);
            command.Parameters.AddWithValue("@Ordinal", payload.Ordinal);
            command.Parameters.AddWithValue("@CreatedAt", payload.CreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@UpdatedAt", payload.UpdatedAt.ToString("O"));

            command.ExecuteNonQuery();

            // Enqueue children for breadth-first traversal
            foreach (var child in node.Children)
            {
                queue.Enqueue(child);
            }
        }
    }

    /// <summary>
    /// Loads a tree structure starting from the specified root node ID.
    /// </summary>
    public TreeNode<TopicNodeData> LoadTree(string rootNodeId)
    {
        // Load all nodes, then build tree structure starting from root
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            SELECT NodeId, ParentId, Title, Prompt, Response, Ordinal, CreatedAt, UpdatedAt
            FROM topic_nodes
            ORDER BY CASE WHEN ParentId IS NULL THEN 0 ELSE 1 END, ParentId, Ordinal, NodeId
        ";

        var nodeMap = new Dictionary<string, (TreeNode<TopicNodeData> node, string? parentId)>();
        TreeNode<TopicNodeData>? root = null;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var nodeId = reader.GetString(0);
            var parentId = reader.IsDBNull(1) ? null : reader.GetString(1);
            var title = reader.IsDBNull(2) ? null : reader.GetString(2);
            var prompt = reader.GetString(3);
            var response = reader.GetString(4);
            var ordinal = reader.GetInt32(5);
            var createdAt = DateTime.Parse(reader.GetString(6));
            var updatedAt = DateTime.Parse(reader.GetString(7));

            var payload = new TopicNodeData
            {
                Title = title,
                Prompt = prompt,
                Response = response,
                Ordinal = ordinal,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            var node = new TreeNode<TopicNodeData>(payload);
            node.NodeId = nodeId;
            nodeMap[nodeId] = (node, parentId);

            if (nodeId == rootNodeId)
            {
                root = node;
            }
        }

        if (root == null)
        {
            throw new InvalidOperationException($"Root node with ID '{rootNodeId}' not found in database.");
        }

        // Build parent-child relationships
        foreach (var (nodeId, (node, parentId)) in nodeMap)
        {
            if (parentId != null && nodeMap.TryGetValue(parentId, out var parentInfo))
            {
                node.SetParent(parentInfo.node, updateChildNodes: true);
            }
        }

        return root;
    }
}


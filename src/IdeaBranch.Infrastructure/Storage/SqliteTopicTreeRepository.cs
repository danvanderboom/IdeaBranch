using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.Domain;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// SQLite implementation of ITopicTreeRepository.
/// Uses TopicDb for connection and migration management, and SqliteTopicTreeStore for tree operations.
/// </summary>
public class SqliteTopicTreeRepository : ITopicTreeRepository, IDisposable
{
    private readonly TopicDb? _db;
    private readonly SqliteConnection? _connection; // For backward compatibility
    private readonly SqliteTopicTreeStore _store;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance with a TopicDb instance.
    /// </summary>
    public SqliteTopicTreeRepository(TopicDb db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _connection = null;
        _store = new SqliteTopicTreeStore(_db.Connection, ExtractTopicNodePayload);
    }

    /// <summary>
    /// Initializes a new instance with a SQLite connection (backward compatibility).
    /// </summary>
    public SqliteTopicTreeRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _db = null;
        _store = new SqliteTopicTreeStore(_connection, ExtractTopicNodePayload);
    }

    /// <summary>
    /// Gets the SQLite connection, either from TopicDb or directly.
    /// </summary>
    private SqliteConnection GetConnection()
    {
        if (_db != null)
            return _db.Connection;
        if (_connection != null)
            return _connection;
        throw new InvalidOperationException("Database connection not available.");
    }

    /// <inheritdoc/>
    public async Task<TopicNode> GetRootAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            // TopicDb handles schema migrations, no need to call EnsureSchema
            
            // Check if root node exists
            var connection = GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT NodeId FROM topic_nodes 
                WHERE ParentId IS NULL
                LIMIT 1
            ";
            
            var result = command.ExecuteScalar();
            
            if (result != null)
            {
                var rootNodeId = result.ToString()!;
                var treeNode = _store.LoadTree(rootNodeId);
                // Convert TreeNode back to TopicNode
                return ConvertToTopicNode(treeNode);
            }
            
            // No root exists - create default root
            return CreateDefaultRoot();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(TopicNode root, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            // Convert TopicNode to ITreeNode
            var rootTreeNode = ConvertToTreeNode(root);
            _store.SaveTree(rootTreeNode);
        }, cancellationToken);
    }

    /// <summary>
    /// Creates a default root topic node if none exists.
    /// </summary>
    private TopicNode CreateDefaultRoot()
    {
        var root = new TopicNode(
            "What would you like to explore?",
            "Root Topic"
        );
        root.SetResponse("This is a placeholder response.", parseListItems: false);
        
        // Save it immediately
        var rootTreeNode = ConvertToTreeNode(root);
        _store.SaveTree(rootTreeNode);
        
        return root;
    }

    /// <summary>
    /// Converts a TopicNode to a TreeNode with TopicNodeData for storage.
    /// </summary>
    private TreeNode<SqliteTopicTreeStore.TopicNodeData> ConvertToTreeNode(TopicNode domainNode)
    {
        var payload = new SqliteTopicTreeStore.TopicNodeData
        {
            Title = domainNode.Title,
            Prompt = domainNode.Prompt,
            Response = domainNode.Response,
            Ordinal = domainNode.Order,
            CreatedAt = domainNode.CreatedAt,
            UpdatedAt = domainNode.UpdatedAt
        };

        var treeNode = new TreeNode<SqliteTopicTreeStore.TopicNodeData>(payload);
        treeNode.NodeId = domainNode.Id.ToString();

        // Recursively add children
        foreach (var child in domainNode.Children)
        {
            var childTreeNode = ConvertToTreeNode(child);
            treeNode.Children.Add(childTreeNode);
        }

        return treeNode;
    }

    /// <summary>
    /// Converts a TreeNode back to a TopicNode.
    /// </summary>
    private TopicNode ConvertToTopicNode(TreeNode<SqliteTopicTreeStore.TopicNodeData> treeNode)
    {
        var payload = treeNode.Payload;
        var nodeId = Guid.Parse(treeNode.NodeId);
        
        // Use reflection to invoke internal constructor that accepts existing ID and timestamps
        var internalConstructor = typeof(TopicNode).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(Guid), typeof(string), typeof(DateTime), typeof(DateTime), typeof(string) },
            null);
        
        TopicNode domainNode;
        if (internalConstructor != null)
        {
            // Use internal constructor with existing ID and timestamps
            domainNode = (TopicNode)internalConstructor.Invoke(new object[]
            {
                nodeId,
                payload.Prompt,
                payload.CreatedAt,
                payload.UpdatedAt,
                payload.Title ?? (object)string.Empty
            });
            
            domainNode.Response = payload.Response;
            domainNode.Order = payload.Ordinal;
        }
        else
        {
            // Fallback: create normally and set properties via reflection
            domainNode = new TopicNode(payload.Prompt, payload.Title)
            {
                Response = payload.Response,
                Order = payload.Ordinal
            };
            
            var idProperty = typeof(TopicNode).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            var createdAtProperty = typeof(TopicNode).GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance);
            var updatedAtProperty = typeof(TopicNode).GetProperty("UpdatedAt", BindingFlags.Public | BindingFlags.Instance);
            
            if (idProperty?.SetMethod != null)
                idProperty.SetValue(domainNode, nodeId);
            
            if (createdAtProperty?.SetMethod != null)
                createdAtProperty.SetValue(domainNode, payload.CreatedAt);
                
            if (updatedAtProperty?.SetMethod != null)
                updatedAtProperty.SetValue(domainNode, payload.UpdatedAt);
        }

        // Recursively add children
        foreach (var childTreeNode in treeNode.Children.OfType<TreeNode<SqliteTopicTreeStore.TopicNodeData>>())
        {
            var childNode = ConvertToTopicNode(childTreeNode);
            domainNode.AddChild(childNode);
        }

        return domainNode;
    }

    /// <summary>
    /// Extracts payload from an ITreeNode for use with SqliteTopicTreeStore.
    /// </summary>
    private static SqliteTopicTreeStore.TopicNodeData ExtractTopicNodePayload(ITreeNode node)
    {
        // If it's a TreeNode with TopicNodeData, extract from that
        if (node is ITreeNode<SqliteTopicTreeStore.TopicNodeData> typedNode)
        {
            return typedNode.Payload;
        }

        // Fallback - shouldn't happen in normal flow
        return new SqliteTopicTreeStore.TopicNodeData
        {
            Prompt = string.Empty,
            Response = string.Empty
        };
    }

    /// <summary>
    /// Disposes the repository and underlying database connection (if managed by TopicDb).
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _db?.Dispose();
                // _connection is not disposed here as it may be managed externally
            }
            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

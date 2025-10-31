using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.Domain;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// SQLite implementation of ITopicTreeRepository.
/// Uses SqliteTopicTreeStore internally and converts between TopicNode and ITreeNode.
/// </summary>
public class SqliteTopicTreeRepository : ITopicTreeRepository
{
    private readonly SqliteConnection _connection;
    private readonly SqliteTopicTreeStore _store;

    /// <summary>
    /// Initializes a new instance with a SQLite connection.
    /// </summary>
    public SqliteTopicTreeRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _store = new SqliteTopicTreeStore(_connection, ExtractTopicNodePayload);
    }

    /// <inheritdoc/>
    public async Task<TopicNode> GetRootAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            _store.EnsureSchema();
            
            // Check if root node exists
            using var command = _connection.CreateCommand();
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
        
        var domainNode = new TopicNode(
            nodeId,
            payload.Prompt,
            payload.CreatedAt,
            payload.UpdatedAt,
            payload.Title
        )
        {
            Response = payload.Response,
            Order = payload.Ordinal
        };

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
}

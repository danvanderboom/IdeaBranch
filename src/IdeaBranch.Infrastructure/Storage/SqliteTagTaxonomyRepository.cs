using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// SQLite implementation of ITagTaxonomyRepository.
/// Handles hierarchical tag taxonomy storage and retrieval.
/// </summary>
public class SqliteTagTaxonomyRepository : ITagTaxonomyRepository
{
    private readonly SqliteConnection _connection;
    private const string DefaultRootName = "Root";

    /// <summary>
    /// Initializes a new instance with a SQLite connection.
    /// </summary>
    public SqliteTagTaxonomyRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public async Task<TagTaxonomyNode> GetRootAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, ParentId, Name, ""Order"", CreatedAt, UpdatedAt
                FROM tag_taxonomy_nodes
                WHERE ParentId IS NULL
                ORDER BY ""Order"", Name
                LIMIT 1
            ";

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return ReadNode(reader);
            }

            // Create default root if none exists
            var root = new TagTaxonomyNode(DefaultRootName, null);
            SaveNode(root);
            return root;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TagTaxonomyNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, ParentId, Name, ""Order"", CreatedAt, UpdatedAt
                FROM tag_taxonomy_nodes
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@Id", id.ToString());

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return ReadNode(reader);
            }

            return null;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TagTaxonomyNode>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            if (parentId.HasValue)
            {
                command.CommandText = @"
                    SELECT Id, ParentId, Name, ""Order"", CreatedAt, UpdatedAt
                    FROM tag_taxonomy_nodes
                    WHERE ParentId = @ParentId
                    ORDER BY ""Order"", Name
                ";
                command.Parameters.AddWithValue("@ParentId", parentId.Value.ToString());
            }
            else
            {
                command.CommandText = @"
                    SELECT Id, ParentId, Name, ""Order"", CreatedAt, UpdatedAt
                    FROM tag_taxonomy_nodes
                    WHERE ParentId IS NULL
                    ORDER BY ""Order"", Name
                ";
            }

            var nodes = new List<TagTaxonomyNode>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var node = ReadNode(reader);
                nodes.Add(node);
            }

            return nodes.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(TagTaxonomyNode node, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            SaveNode(node);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM tag_taxonomy_nodes
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@Id", id.ToString());

            return command.ExecuteNonQuery() > 0;
        }, cancellationToken);
    }

    /// <summary>
    /// Saves a tag taxonomy node (upsert by ID).
    /// </summary>
    private void SaveNode(TagTaxonomyNode node)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO tag_taxonomy_nodes (Id, ParentId, Name, ""Order"", CreatedAt, UpdatedAt)
            VALUES (@Id, @ParentId, @Name, @Order, @CreatedAt, @UpdatedAt)
            ON CONFLICT(Id) DO UPDATE SET
                ParentId = excluded.ParentId,
                Name = excluded.Name,
                ""Order"" = excluded.""Order"",
                UpdatedAt = excluded.UpdatedAt
        ";

        command.Parameters.AddWithValue("@Id", node.Id.ToString());
        command.Parameters.AddWithValue("@ParentId", (object?)node.ParentId?.ToString() ?? DBNull.Value);
        command.Parameters.AddWithValue("@Name", node.Name);
        command.Parameters.AddWithValue("@Order", node.Order);
        command.Parameters.AddWithValue("@CreatedAt", node.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", node.UpdatedAt.ToString("O"));

        command.ExecuteNonQuery();

        // Save children recursively
        foreach (var child in node.Children)
        {
            SaveNode(child);
        }
    }

    /// <summary>
    /// Reads a TagTaxonomyNode from a data reader.
    /// </summary>
    private static TagTaxonomyNode ReadNode(SqliteDataReader reader)
    {
        var id = Guid.Parse(reader.GetString(0));
        var parentId = reader.IsDBNull(1) ? null : (Guid?)Guid.Parse(reader.GetString(1));
        var name = reader.GetString(2);
        var order = reader.GetInt32(3);
        var createdAt = DateTime.Parse(reader.GetString(4));
        var updatedAt = DateTime.Parse(reader.GetString(5));

        return new TagTaxonomyNode(id, parentId, name, order, createdAt, updatedAt);
    }
}


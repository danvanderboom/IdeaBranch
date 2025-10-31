using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// SQLite implementation of IPromptTemplateRepository.
/// Handles hierarchical prompt template storage and retrieval.
/// </summary>
public class SqlitePromptTemplateRepository : IPromptTemplateRepository
{
    private readonly SqliteConnection _connection;
    private const string DefaultRootName = "Root";

    /// <summary>
    /// Initializes a new instance with a SQLite connection.
    /// </summary>
    public SqlitePromptTemplateRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public async Task<PromptTemplate> GetRootAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, ParentId, Name, Body, ""Order"", CreatedAt, UpdatedAt
                FROM prompt_templates
                WHERE ParentId IS NULL
                ORDER BY ""Order"", Name
                LIMIT 1
            ";

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return ReadTemplate(reader);
            }

            // Create default root if none exists
            var root = new PromptTemplate(DefaultRootName);
            SaveTemplate(root);
            return root;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, ParentId, Name, Body, ""Order"", CreatedAt, UpdatedAt
                FROM prompt_templates
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@Id", id.ToString());

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return ReadTemplate(reader);
            }

            return null;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PromptTemplate>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            if (parentId.HasValue)
            {
                command.CommandText = @"
                    SELECT Id, ParentId, Name, Body, ""Order"", CreatedAt, UpdatedAt
                    FROM prompt_templates
                    WHERE ParentId = @ParentId
                    ORDER BY ""Order"", Name
                ";
                command.Parameters.AddWithValue("@ParentId", parentId.Value.ToString());
            }
            else
            {
                command.CommandText = @"
                    SELECT Id, ParentId, Name, Body, ""Order"", CreatedAt, UpdatedAt
                    FROM prompt_templates
                    WHERE ParentId IS NULL
                    ORDER BY ""Order"", Name
                ";
            }

            var templates = new List<PromptTemplate>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var template = ReadTemplate(reader);
                templates.Add(template);
            }

            return templates.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PromptTemplate?> GetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        return await Task.Run(() =>
        {
            var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length == 0)
                return null;

            Guid? currentId = null;

            foreach (var part in pathParts)
            {
                using var command = _connection.CreateCommand();
                if (currentId.HasValue)
                {
                    command.CommandText = @"
                        SELECT Id, ParentId, Name, Body, ""Order"", CreatedAt, UpdatedAt
                        FROM prompt_templates
                        WHERE ParentId = @ParentId AND Name = @Name
                        LIMIT 1
                    ";
                    command.Parameters.AddWithValue("@ParentId", currentId.Value.ToString());
                }
                else
                {
                    command.CommandText = @"
                        SELECT Id, ParentId, Name, Body, ""Order"", CreatedAt, UpdatedAt
                        FROM prompt_templates
                        WHERE ParentId IS NULL AND Name = @Name
                        LIMIT 1
                    ";
                }

                command.Parameters.AddWithValue("@Name", part);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                    return null;

                currentId = Guid.Parse(reader.GetString(0));
            }

            if (currentId.HasValue)
            {
                return GetByIdAsync(currentId.Value, cancellationToken).Result;
            }

            return null;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PromptTemplate>> GetSubtreeAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            // Recursively get all templates (not categories) in the subtree
            var allTemplates = new List<PromptTemplate>();
            GetSubtreeRecursive(parentId, allTemplates);
            return allTemplates.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(PromptTemplate template, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            SaveTemplate(template);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM prompt_templates
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@Id", id.ToString());

            return command.ExecuteNonQuery() > 0;
        }, cancellationToken);
    }

    /// <summary>
    /// Saves a prompt template (upsert by ID).
    /// </summary>
    private void SaveTemplate(PromptTemplate template)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO prompt_templates (Id, ParentId, Name, Body, ""Order"", CreatedAt, UpdatedAt)
            VALUES (@Id, @ParentId, @Name, @Body, @Order, @CreatedAt, @UpdatedAt)
            ON CONFLICT(Id) DO UPDATE SET
                ParentId = excluded.ParentId,
                Name = excluded.Name,
                Body = excluded.Body,
                ""Order"" = excluded.""Order"",
                UpdatedAt = excluded.UpdatedAt
        ";

        command.Parameters.AddWithValue("@Id", template.Id.ToString());
        command.Parameters.AddWithValue("@ParentId", (object?)template.ParentId?.ToString() ?? DBNull.Value);
        command.Parameters.AddWithValue("@Name", template.Name);
        command.Parameters.AddWithValue("@Body", (object?)template.Body ?? DBNull.Value);
        command.Parameters.AddWithValue("@Order", template.Order);
        command.Parameters.AddWithValue("@CreatedAt", template.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", template.UpdatedAt.ToString("O"));

        command.ExecuteNonQuery();

        // Save children recursively
        foreach (var child in template.Children)
        {
            SaveTemplate(child);
        }
    }

    /// <summary>
    /// Recursively gets all templates (not categories) in a subtree.
    /// </summary>
    private void GetSubtreeRecursive(Guid? parentId, List<PromptTemplate> result)
    {
        using var command = _connection.CreateCommand();
        if (parentId.HasValue)
        {
                command.CommandText = @"
                    SELECT Id, ParentId, Name, Body, ""Order"", CreatedAt, UpdatedAt
                    FROM prompt_templates
                    WHERE ParentId = @ParentId
                    ORDER BY ""Order"", Name
                ";
            command.Parameters.AddWithValue("@ParentId", parentId.Value.ToString());
        }
        else
        {
                command.CommandText = @"
                    SELECT Id, ParentId, Name, Body, ""Order"", CreatedAt, UpdatedAt
                    FROM prompt_templates
                    WHERE ParentId IS NULL
                    ORDER BY ""Order"", Name
                ";
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var template = ReadTemplate(reader);
            if (template.IsCategory)
            {
                // Recursively get children of categories
                GetSubtreeRecursive(template.Id, result);
            }
            else
            {
                // Add templates (not categories) to result
                result.Add(template);
            }
        }
    }

    /// <summary>
    /// Reads a PromptTemplate from a data reader.
    /// </summary>
    private static PromptTemplate ReadTemplate(SqliteDataReader reader)
    {
        var id = Guid.Parse(reader.GetString(0));
        var parentId = reader.IsDBNull(1) ? null : (Guid?)Guid.Parse(reader.GetString(1));
        var name = reader.GetString(2);
        var body = reader.IsDBNull(3) ? null : reader.GetString(3);
        var order = reader.GetInt32(4);
        var createdAt = DateTime.Parse(reader.GetString(5));
        var updatedAt = DateTime.Parse(reader.GetString(6));

        return new PromptTemplate(id, parentId, name, body, order, createdAt, updatedAt);
    }
}


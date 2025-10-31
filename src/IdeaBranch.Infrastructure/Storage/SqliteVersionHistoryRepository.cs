using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// SQLite implementation of IVersionHistoryRepository.
/// </summary>
public class SqliteVersionHistoryRepository : IVersionHistoryRepository
{
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Initializes a new instance with a SQLite connection.
    /// </summary>
    public SqliteVersionHistoryRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public async Task SaveAsync(TopicNodeVersion version, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO version_history (
                    VersionId, NodeId, Title, Prompt, Response, Ordinal, VersionTimestamp, AuthorId, AuthorName
                )
                VALUES (
                    @VersionId, @NodeId, @Title, @Prompt, @Response, @Ordinal, @VersionTimestamp, @AuthorId, @AuthorName
                )
            ";

            command.Parameters.AddWithValue("@VersionId", version.VersionId.ToString());
            command.Parameters.AddWithValue("@NodeId", version.NodeId.ToString());
            command.Parameters.AddWithValue("@Title", (object?)version.Title ?? DBNull.Value);
            command.Parameters.AddWithValue("@Prompt", version.Prompt ?? string.Empty);
            command.Parameters.AddWithValue("@Response", version.Response ?? string.Empty);
            command.Parameters.AddWithValue("@Ordinal", version.Order);
            command.Parameters.AddWithValue("@VersionTimestamp", version.VersionTimestamp.ToString("O"));
            command.Parameters.AddWithValue("@AuthorId", (object?)version.AuthorId ?? DBNull.Value);
            command.Parameters.AddWithValue("@AuthorName", (object?)version.AuthorName ?? DBNull.Value);

            command.ExecuteNonQuery();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TopicNodeVersion>> GetByNodeIdAsync(Guid nodeId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT VersionId, NodeId, Title, Prompt, Response, Ordinal, VersionTimestamp, AuthorId, AuthorName
                FROM version_history
                WHERE NodeId = @NodeId
                ORDER BY VersionTimestamp DESC
            ";

            command.Parameters.AddWithValue("@NodeId", nodeId.ToString());

            var versions = new List<TopicNodeVersion>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var version = ReadVersion(reader);
                versions.Add(version);
            }

            return versions.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TopicNodeVersion?> GetLatestAsync(Guid nodeId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT VersionId, NodeId, Title, Prompt, Response, Ordinal, VersionTimestamp, AuthorId, AuthorName
                FROM version_history
                WHERE NodeId = @NodeId
                ORDER BY VersionTimestamp DESC
                LIMIT 1
            ";

            command.Parameters.AddWithValue("@NodeId", nodeId.ToString());

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return ReadVersion(reader);
            }

            return null;
        }, cancellationToken);
    }

    /// <summary>
    /// Reads a TopicNodeVersion from a data reader.
    /// </summary>
    private static TopicNodeVersion ReadVersion(SqliteDataReader reader)
    {
        var versionId = Guid.Parse(reader.GetString(0));
        var nodeId = Guid.Parse(reader.GetString(1));
        var title = reader.IsDBNull(2) ? null : reader.GetString(2);
        var prompt = reader.GetString(3);
        var response = reader.GetString(4);
        var ordinal = reader.GetInt32(5);
        var versionTimestamp = DateTime.Parse(reader.GetString(6));
        var authorId = reader.IsDBNull(7) ? null : reader.GetString(7);
        var authorName = reader.IsDBNull(8) ? null : reader.GetString(8);

        return new TopicNodeVersion(
            versionId,
            nodeId,
            title,
            prompt,
            response,
            ordinal,
            versionTimestamp,
            authorId,
            authorName);
    }
}


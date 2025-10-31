using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// SQLite implementation of IAnnotationsRepository.
/// </summary>
public class SqliteAnnotationsRepository : IAnnotationsRepository
{
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Initializes a new instance with a SQLite connection.
    /// </summary>
    public SqliteAnnotationsRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public async Task SaveAsync(Annotation annotation, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO annotations (Id, NodeId, StartOffset, EndOffset, Comment, CreatedAt, UpdatedAt)
                VALUES (@Id, @NodeId, @StartOffset, @EndOffset, @Comment, @CreatedAt, @UpdatedAt)
                ON CONFLICT(Id) DO UPDATE SET
                    NodeId = @NodeId,
                    StartOffset = @StartOffset,
                    EndOffset = @EndOffset,
                    Comment = @Comment,
                    UpdatedAt = @UpdatedAt
            ";

            command.Parameters.AddWithValue("@Id", annotation.Id.ToString());
            command.Parameters.AddWithValue("@NodeId", annotation.NodeId.ToString());
            command.Parameters.AddWithValue("@StartOffset", annotation.StartOffset);
            command.Parameters.AddWithValue("@EndOffset", annotation.EndOffset);
            command.Parameters.AddWithValue("@Comment", (object?)annotation.Comment ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", annotation.CreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@UpdatedAt", annotation.UpdatedAt.ToString("O"));

            command.ExecuteNonQuery();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Annotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, NodeId, StartOffset, EndOffset, Comment, CreatedAt, UpdatedAt
                FROM annotations
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@Id", id.ToString());

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return ReadAnnotation(reader);
            }

            return null;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Annotation>> GetByNodeIdAsync(Guid nodeId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, NodeId, StartOffset, EndOffset, Comment, CreatedAt, UpdatedAt
                FROM annotations
                WHERE NodeId = @NodeId
                ORDER BY StartOffset, EndOffset
            ";

            command.Parameters.AddWithValue("@NodeId", nodeId.ToString());

            var annotations = new List<Annotation>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var annotation = ReadAnnotation(reader);
                annotations.Add(annotation);
            }

            return annotations.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Annotation>> GetByNodeIdAndTagsAsync(Guid nodeId, IReadOnlyList<Guid> tagIds, CancellationToken cancellationToken = default)
    {
        if (tagIds == null || tagIds.Count == 0)
            return await GetByNodeIdAsync(nodeId, cancellationToken);

        return await Task.Run(() =>
        {
            // Build the query with IN clause for tag IDs
            var tagIdStrings = tagIds.Select(id => id.ToString()).ToList();
            var placeholders = string.Join(",", tagIdStrings.Select((_, i) => $"@TagId{i}"));

            using var command = _connection.CreateCommand();
            command.CommandText = $@"
                SELECT DISTINCT a.Id, a.NodeId, a.StartOffset, a.EndOffset, a.Comment, a.CreatedAt, a.UpdatedAt
                FROM annotations a
                INNER JOIN annotation_tags at ON a.Id = at.AnnotationId
                WHERE a.NodeId = @NodeId
                  AND at.TagId IN ({placeholders})
                ORDER BY a.StartOffset, a.EndOffset
            ";

            command.Parameters.AddWithValue("@NodeId", nodeId.ToString());
            for (int i = 0; i < tagIdStrings.Count; i++)
            {
                command.Parameters.AddWithValue($"@TagId{i}", tagIdStrings[i]);
            }

            var annotations = new List<Annotation>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var annotation = ReadAnnotation(reader);
                annotations.Add(annotation);
            }

            return annotations.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM annotations
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@Id", id.ToString());

            return command.ExecuteNonQuery() > 0;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddTagAsync(Guid annotationId, Guid tagId, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT OR IGNORE INTO annotation_tags (AnnotationId, TagId)
                VALUES (@AnnotationId, @TagId)
            ";

            command.Parameters.AddWithValue("@AnnotationId", annotationId.ToString());
            command.Parameters.AddWithValue("@TagId", tagId.ToString());

            command.ExecuteNonQuery();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RemoveTagAsync(Guid annotationId, Guid tagId, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM annotation_tags
                WHERE AnnotationId = @AnnotationId AND TagId = @TagId
            ";

            command.Parameters.AddWithValue("@AnnotationId", annotationId.ToString());
            command.Parameters.AddWithValue("@TagId", tagId.ToString());

            command.ExecuteNonQuery();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Guid>> GetTagIdsAsync(Guid annotationId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT TagId
                FROM annotation_tags
                WHERE AnnotationId = @AnnotationId
            ";

            command.Parameters.AddWithValue("@AnnotationId", annotationId.ToString());

            var tagIds = new List<Guid>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var tagId = Guid.Parse(reader.GetString(0));
                tagIds.Add(tagId);
            }

            return tagIds.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveValueAsync(AnnotationValue value, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO annotation_values (Id, AnnotationId, ValueType, NumericValue, GeospatialValue, TemporalValue)
                VALUES (@Id, @AnnotationId, @ValueType, @NumericValue, @GeospatialValue, @TemporalValue)
                ON CONFLICT(Id) DO UPDATE SET
                    AnnotationId = @AnnotationId,
                    ValueType = @ValueType,
                    NumericValue = @NumericValue,
                    GeospatialValue = @GeospatialValue,
                    TemporalValue = @TemporalValue
            ";

            command.Parameters.AddWithValue("@Id", value.Id.ToString());
            command.Parameters.AddWithValue("@AnnotationId", value.AnnotationId.ToString());
            command.Parameters.AddWithValue("@ValueType", value.ValueType);
            command.Parameters.AddWithValue("@NumericValue", (object?)value.NumericValue ?? DBNull.Value);
            command.Parameters.AddWithValue("@GeospatialValue", (object?)value.GeospatialValue ?? DBNull.Value);
            command.Parameters.AddWithValue("@TemporalValue", (object?)value.TemporalValue ?? DBNull.Value);

            command.ExecuteNonQuery();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AnnotationValue>> GetValuesAsync(Guid annotationId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, AnnotationId, ValueType, NumericValue, GeospatialValue, TemporalValue
                FROM annotation_values
                WHERE AnnotationId = @AnnotationId
            ";

            command.Parameters.AddWithValue("@AnnotationId", annotationId.ToString());

            var values = new List<AnnotationValue>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var value = ReadAnnotationValue(reader);
                values.Add(value);
            }

            return values.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteValueAsync(Guid valueId, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM annotation_values
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@Id", valueId.ToString());

            return command.ExecuteNonQuery() > 0;
        }, cancellationToken);
    }

    /// <summary>
    /// Reads an Annotation from a data reader.
    /// </summary>
    private static Annotation ReadAnnotation(SqliteDataReader reader)
    {
        var id = Guid.Parse(reader.GetString(0));
        var nodeId = Guid.Parse(reader.GetString(1));
        var startOffset = reader.GetInt32(2);
        var endOffset = reader.GetInt32(3);
        var comment = reader.IsDBNull(4) ? null : reader.GetString(4);
        var createdAt = DateTime.Parse(reader.GetString(5));
        var updatedAt = DateTime.Parse(reader.GetString(6));

        return new Annotation(id, nodeId, startOffset, endOffset, createdAt, updatedAt, comment);
    }

    /// <summary>
    /// Reads an AnnotationValue from a data reader.
    /// </summary>
    private static AnnotationValue ReadAnnotationValue(SqliteDataReader reader)
    {
        var id = Guid.Parse(reader.GetString(0));
        var annotationId = Guid.Parse(reader.GetString(1));
        var valueType = reader.GetString(2);
        var numericValue = reader.IsDBNull(3) ? null : (double?)reader.GetDouble(3);
        var geospatialValue = reader.IsDBNull(4) ? null : reader.GetString(4);
        var temporalValue = reader.IsDBNull(5) ? null : reader.GetString(5);

        return new AnnotationValue(id, annotationId, valueType, numericValue, geospatialValue, temporalValue);
    }
}


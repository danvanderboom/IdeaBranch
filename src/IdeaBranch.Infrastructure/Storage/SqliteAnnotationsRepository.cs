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

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Annotation>> QueryAsync(
        Guid nodeId,
        IReadOnlyList<Guid>? tagIds = null,
        double? numericMin = null,
        double? numericMax = null,
        DateTime? temporalStart = null,
        DateTime? temporalEnd = null,
        (double minLat, double minLon, double maxLat, double maxLon)? geoBbox = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var hasTagFilter = tagIds != null && tagIds.Count > 0;
            var hasNumericFilter = numericMin.HasValue || numericMax.HasValue;
            var hasTemporalFilter = temporalStart.HasValue || temporalEnd.HasValue;
            var hasGeoFilter = geoBbox.HasValue;

            // Build the base query with necessary joins
            var queryParts = new List<string>();
            var joinParts = new List<string>();
            var whereParts = new List<string> { "a.NodeId = @NodeId" };

            queryParts.Add("SELECT DISTINCT a.Id, a.NodeId, a.StartOffset, a.EndOffset, a.Comment, a.CreatedAt, a.UpdatedAt");
            queryParts.Add("FROM annotations a");

            // Add tag filter join if needed
            if (hasTagFilter)
            {
                var tagIdStrings = tagIds!.Select(id => id.ToString()).ToList();
                var placeholders = string.Join(",", tagIdStrings.Select((_, i) => $"@TagId{i}"));
                joinParts.Add("INNER JOIN annotation_tags at ON a.Id = at.AnnotationId");
                whereParts.Add($"at.TagId IN ({placeholders})");
            }

            // Add numeric filter join if needed
            if (hasNumericFilter)
            {
                joinParts.Add("INNER JOIN annotation_values av_numeric ON a.Id = av_numeric.AnnotationId AND av_numeric.ValueType = 'numeric'");
                if (numericMin.HasValue)
                {
                    whereParts.Add("av_numeric.NumericValue >= @NumericMin");
                }
                if (numericMax.HasValue)
                {
                    whereParts.Add("av_numeric.NumericValue <= @NumericMax");
                }
            }

            // Add temporal filter join if needed
            if (hasTemporalFilter)
            {
                joinParts.Add("INNER JOIN annotation_values av_temporal ON a.Id = av_temporal.AnnotationId AND av_temporal.ValueType = 'temporal'");
                if (temporalStart.HasValue)
                {
                    whereParts.Add("av_temporal.TemporalValue >= @TemporalStart");
                }
                if (temporalEnd.HasValue)
                {
                    whereParts.Add("av_temporal.TemporalValue <= @TemporalEnd");
                }
            }

            // Add geospatial filter join if needed
            if (hasGeoFilter)
            {
                joinParts.Add("INNER JOIN annotation_values av_geo ON a.Id = av_geo.AnnotationId AND av_geo.ValueType = 'geospatial'");
                // Use json_extract if JSON1 extension is available, otherwise skip geo filter
                // SQLite's json_extract can handle JSON strings
                whereParts.Add("json_extract(av_geo.GeospatialValue, '$.lat') >= @GeoMinLat");
                whereParts.Add("json_extract(av_geo.GeospatialValue, '$.lat') <= @GeoMaxLat");
                whereParts.Add("json_extract(av_geo.GeospatialValue, '$.lon') >= @GeoMinLon");
                whereParts.Add("json_extract(av_geo.GeospatialValue, '$.lon') <= @GeoMaxLon");
            }

            // Combine query parts
            if (joinParts.Count > 0)
            {
                queryParts.Add(string.Join(" ", joinParts));
            }

            queryParts.Add("WHERE " + string.Join(" AND ", whereParts));
            queryParts.Add("ORDER BY a.StartOffset, a.EndOffset");

            using var command = _connection.CreateCommand();
            command.CommandText = string.Join("\n", queryParts);
            command.Parameters.AddWithValue("@NodeId", nodeId.ToString());

            // Add parameters for tag filter
            if (hasTagFilter)
            {
                var tagIdStrings = tagIds!.Select(id => id.ToString()).ToList();
                for (int i = 0; i < tagIdStrings.Count; i++)
                {
                    command.Parameters.AddWithValue($"@TagId{i}", tagIdStrings[i]);
                }
            }

            // Add parameters for numeric filter
            if (numericMin.HasValue)
            {
                command.Parameters.AddWithValue("@NumericMin", numericMin.Value);
            }
            if (numericMax.HasValue)
            {
                command.Parameters.AddWithValue("@NumericMax", numericMax.Value);
            }

            // Add parameters for temporal filter
            if (temporalStart.HasValue)
            {
                command.Parameters.AddWithValue("@TemporalStart", temporalStart.Value.ToString("O"));
            }
            if (temporalEnd.HasValue)
            {
                command.Parameters.AddWithValue("@TemporalEnd", temporalEnd.Value.ToString("O"));
            }

            // Add parameters for geospatial filter
            if (hasGeoFilter)
            {
                var (minLat, minLon, maxLat, maxLon) = geoBbox!.Value;
                command.Parameters.AddWithValue("@GeoMinLat", minLat);
                command.Parameters.AddWithValue("@GeoMaxLat", maxLat);
                command.Parameters.AddWithValue("@GeoMinLon", minLon);
                command.Parameters.AddWithValue("@GeoMaxLon", maxLon);
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


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

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Annotation>> SearchAsync(
        Guid nodeId,
        AnnotationsSearchOptions options,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var queryParts = new List<string>();
            var joinParts = new List<string>();
            var whereParts = new List<string> { "a.NodeId = @NodeId" };
            var havingParts = new List<string>();

            queryParts.Add("SELECT a.Id, a.NodeId, a.StartOffset, a.EndOffset, a.Comment, a.CreatedAt, a.UpdatedAt");
            queryParts.Add("FROM annotations a");

            var hasIncludeTags = options.IncludeTags != null && options.IncludeTags.Count > 0;
            var hasExcludeTags = options.ExcludeTags != null && options.ExcludeTags.Count > 0;
            var hasWeightFilters = options.TagWeightFilters != null && options.TagWeightFilters.Count > 0;
            var hasCommentSearch = !string.IsNullOrWhiteSpace(options.CommentContains);
            var hasUpdatedAtFilter = options.UpdatedAtFrom.HasValue || options.UpdatedAtTo.HasValue;
            var hasTemporalFilter = options.TemporalStart.HasValue || options.TemporalEnd.HasValue;

            // Track if we need GROUP BY (only if we have include tags)
            var needsGroupBy = false;

            // Include tags filter (AND logic via GROUP BY/HAVING)
            if (hasIncludeTags)
            {
                var tagIdStrings = options.IncludeTags.Select(id => id.ToString()).ToList();
                var placeholders = string.Join(",", tagIdStrings.Select((_, i) => $"@IncludeTagId{i}"));
                joinParts.Add("INNER JOIN annotation_tags at_include ON a.Id = at_include.AnnotationId");
                whereParts.Add($"at_include.TagId IN ({placeholders})");
                needsGroupBy = true;
                havingParts.Add($"COUNT(DISTINCT at_include.TagId) = @IncludeTagCount");
            }

            // Exclude tags filter (NOT EXISTS)
            if (hasExcludeTags)
            {
                var tagIdStrings = options.ExcludeTags.Select(id => id.ToString()).ToList();
                var placeholders = string.Join(",", tagIdStrings.Select((_, i) => $"@ExcludeTagId{i}"));
                whereParts.Add($@"NOT EXISTS (
                    SELECT 1 FROM annotation_tags at_exclude
                    WHERE at_exclude.AnnotationId = a.Id AND at_exclude.TagId IN ({placeholders})
                )");
            }

            // Tag weight filters
            if (hasWeightFilters)
            {
                var weightConditions = new List<string>();
                int weightIndex = 0;
                foreach (var filter in options.TagWeightFilters!)
                {
                    var alias = $"atw{weightIndex}";
                    joinParts.Add($"LEFT JOIN annotation_tags {alias} ON a.Id = {alias}.AnnotationId AND {alias}.TagId = @WeightTagId{weightIndex}");
                    
                    var condition = filter.Op.ToLowerInvariant() switch
                    {
                        "gt" => $"{alias}.Weight > @WeightValue{weightIndex}",
                        "lt" => $"{alias}.Weight < @WeightValue{weightIndex}",
                        "between" => $"{alias}.Weight >= @WeightValue{weightIndex} AND {alias}.Weight <= @WeightValue2{weightIndex}",
                        _ => throw new ArgumentException($"Unsupported weight filter operator: {filter.Op}", nameof(options))
                    };
                    weightConditions.Add(condition);
                    weightIndex++;
                }
                // For AND logic, all weight conditions must be true
                // If we have GROUP BY, add to HAVING instead of WHERE
                var weightCondition = "(" + string.Join(" AND ", weightConditions) + ")";
                if (needsGroupBy)
                {
                    havingParts.Add(weightCondition);
                }
                else
                {
                    whereParts.Add(weightCondition);
                }
            }

            // Comment text search
            if (hasCommentSearch)
            {
                whereParts.Add("a.Comment LIKE @CommentContains");
            }

            // UpdatedAt range filter
            if (hasUpdatedAtFilter)
            {
                if (options.UpdatedAtFrom.HasValue && options.UpdatedAtTo.HasValue)
                {
                    whereParts.Add("a.UpdatedAt BETWEEN @UpdatedAtFrom AND @UpdatedAtTo");
                }
                else if (options.UpdatedAtFrom.HasValue)
                {
                    whereParts.Add("a.UpdatedAt >= @UpdatedAtFrom");
                }
                else if (options.UpdatedAtTo.HasValue)
                {
                    whereParts.Add("a.UpdatedAt <= @UpdatedAtTo");
                }
            }

            // Temporal/historical time range filter
            if (hasTemporalFilter)
            {
                joinParts.Add("INNER JOIN annotation_values av_temporal ON a.Id = av_temporal.AnnotationId AND av_temporal.ValueType = 'temporal'");
                if (options.TemporalStart.HasValue && options.TemporalEnd.HasValue)
                {
                    whereParts.Add("av_temporal.TemporalValue BETWEEN @TemporalStart AND @TemporalEnd");
                }
                else if (options.TemporalStart.HasValue)
                {
                    whereParts.Add("av_temporal.TemporalValue >= @TemporalStart");
                }
                else if (options.TemporalEnd.HasValue)
                {
                    whereParts.Add("av_temporal.TemporalValue <= @TemporalEnd");
                }
            }

            // Combine query parts
            if (joinParts.Count > 0)
            {
                queryParts.Insert(2, string.Join(" ", joinParts));
            }

            queryParts.Add("WHERE " + string.Join(" AND ", whereParts));

            // Add GROUP BY if needed (for include tags AND logic)
            if (needsGroupBy)
            {
                queryParts.Add("GROUP BY a.Id, a.NodeId, a.StartOffset, a.EndOffset, a.Comment, a.CreatedAt, a.UpdatedAt");
            }

            if (havingParts.Count > 0)
            {
                queryParts.Add("HAVING " + string.Join(" AND ", havingParts));
            }

            queryParts.Add("ORDER BY a.StartOffset, a.EndOffset");

            // Add pagination
            if (options.PageSize.HasValue)
            {
                queryParts.Add($"LIMIT @PageSize");
                if (options.PageOffset.HasValue)
                {
                    queryParts.Add($"OFFSET @PageOffset");
                }
            }

            using var command = _connection.CreateCommand();
            command.CommandText = string.Join("\n", queryParts);
            command.Parameters.AddWithValue("@NodeId", nodeId.ToString());

            // Add parameters for include tags
            if (hasIncludeTags)
            {
                var tagIdStrings = options.IncludeTags.Select(id => id.ToString()).ToList();
                for (int i = 0; i < tagIdStrings.Count; i++)
                {
                    command.Parameters.AddWithValue($"@IncludeTagId{i}", tagIdStrings[i]);
                }
                command.Parameters.AddWithValue("@IncludeTagCount", tagIdStrings.Count);
            }

            // Add parameters for exclude tags
            if (hasExcludeTags)
            {
                var tagIdStrings = options.ExcludeTags.Select(id => id.ToString()).ToList();
                for (int i = 0; i < tagIdStrings.Count; i++)
                {
                    command.Parameters.AddWithValue($"@ExcludeTagId{i}", tagIdStrings[i]);
                }
            }

            // Add parameters for weight filters
            if (hasWeightFilters)
            {
                int weightIndex = 0;
                foreach (var filter in options.TagWeightFilters!)
                {
                    command.Parameters.AddWithValue($"@WeightTagId{weightIndex}", filter.TagId.ToString());
                    command.Parameters.AddWithValue($"@WeightValue{weightIndex}", filter.Value);
                    if (filter.Op.ToLowerInvariant() == "between" && filter.Value2.HasValue)
                    {
                        command.Parameters.AddWithValue($"@WeightValue2{weightIndex}", filter.Value2.Value);
                    }
                    weightIndex++;
                }
            }

            // Add parameters for comment search
            if (hasCommentSearch)
            {
                command.Parameters.AddWithValue("@CommentContains", $"%{options.CommentContains}%");
            }

            // Add parameters for UpdatedAt filter
            if (hasUpdatedAtFilter)
            {
                if (options.UpdatedAtFrom.HasValue)
                {
                    command.Parameters.AddWithValue("@UpdatedAtFrom", options.UpdatedAtFrom.Value.ToString("O"));
                }
                if (options.UpdatedAtTo.HasValue)
                {
                    command.Parameters.AddWithValue("@UpdatedAtTo", options.UpdatedAtTo.Value.ToString("O"));
                }
            }

            // Add parameters for temporal filter
            if (hasTemporalFilter)
            {
                if (options.TemporalStart.HasValue)
                {
                    command.Parameters.AddWithValue("@TemporalStart", options.TemporalStart.Value.ToString("O"));
                }
                if (options.TemporalEnd.HasValue)
                {
                    command.Parameters.AddWithValue("@TemporalEnd", options.TemporalEnd.Value.ToString("O"));
                }
            }

            // Add parameters for pagination
            if (options.PageSize.HasValue)
            {
                command.Parameters.AddWithValue("@PageSize", options.PageSize.Value);
                if (options.PageOffset.HasValue)
                {
                    command.Parameters.AddWithValue("@PageOffset", options.PageOffset.Value);
                }
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
    public async Task<IDictionary<Guid, IReadOnlyList<Guid>>> GetTagIdsForAnnotationsAsync(
        IEnumerable<Guid> annotationIds,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var annotationIdList = annotationIds.ToList();
            if (annotationIdList.Count == 0)
                return new Dictionary<Guid, IReadOnlyList<Guid>>();

            var annotationIdStrings = annotationIdList.Select(id => id.ToString()).ToList();
            var placeholders = string.Join(",", annotationIdStrings.Select((_, i) => $"@AnnotationId{i}"));

            using var command = _connection.CreateCommand();
            command.CommandText = $@"
                SELECT AnnotationId, TagId
                FROM annotation_tags
                WHERE AnnotationId IN ({placeholders})
                ORDER BY AnnotationId, TagId
            ";

            for (int i = 0; i < annotationIdStrings.Count; i++)
            {
                command.Parameters.AddWithValue($"@AnnotationId{i}", annotationIdStrings[i]);
            }

            var result = new Dictionary<Guid, List<Guid>>();
            
            // Initialize all annotation IDs with empty lists
            foreach (var id in annotationIdList)
            {
                result[id] = new List<Guid>();
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var annotationId = Guid.Parse(reader.GetString(0));
                var tagId = Guid.Parse(reader.GetString(1));

                if (result.TryGetValue(annotationId, out var tagList))
                {
                    tagList.Add(tagId);
                }
                else
                {
                    result[annotationId] = new List<Guid> { tagId };
                }
            }

            // Convert to IReadOnlyList
            return result.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<Guid>)kvp.Value.AsReadOnly());
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SetTagWeightAsync(
        Guid annotationId,
        Guid tagId,
        double? weight,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            // First, ensure the tag association exists
            using var ensureCommand = _connection.CreateCommand();
            ensureCommand.CommandText = @"
                INSERT OR IGNORE INTO annotation_tags (AnnotationId, TagId, Weight)
                VALUES (@AnnotationId, @TagId, NULL)
            ";
            ensureCommand.Parameters.AddWithValue("@AnnotationId", annotationId.ToString());
            ensureCommand.Parameters.AddWithValue("@TagId", tagId.ToString());
            ensureCommand.ExecuteNonQuery();

            // Then update the weight
            using var updateCommand = _connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE annotation_tags
                SET Weight = @Weight
                WHERE AnnotationId = @AnnotationId AND TagId = @TagId
            ";
            updateCommand.Parameters.AddWithValue("@AnnotationId", annotationId.ToString());
            updateCommand.Parameters.AddWithValue("@TagId", tagId.ToString());
            updateCommand.Parameters.AddWithValue("@Weight", (object?)weight ?? DBNull.Value);
            updateCommand.ExecuteNonQuery();
        }, cancellationToken);
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


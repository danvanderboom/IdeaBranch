using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.Domain;

/// <summary>
/// Repository interface for persisting and loading annotations.
/// </summary>
public interface IAnnotationsRepository
{
    /// <summary>
    /// Saves an annotation.
    /// </summary>
    /// <param name="annotation">The annotation to save.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task SaveAsync(Annotation annotation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an annotation by its ID.
    /// </summary>
    /// <param name="id">The annotation ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The annotation, or null if not found.</returns>
    Task<Annotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all annotations for a topic node.
    /// </summary>
    /// <param name="nodeId">The topic node ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of annotations for the node.</returns>
    Task<IReadOnlyList<Annotation>> GetByNodeIdAsync(Guid nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets annotations for a node filtered by tag IDs.
    /// </summary>
    /// <param name="nodeId">The topic node ID.</param>
    /// <param name="tagIds">The tag IDs to filter by.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of annotations matching the filter.</returns>
    Task<IReadOnlyList<Annotation>> GetByNodeIdAndTagsAsync(Guid nodeId, IReadOnlyList<Guid> tagIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an annotation.
    /// </summary>
    /// <param name="id">The annotation ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the annotation was found and deleted; false otherwise.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a tag to an annotation.
    /// </summary>
    /// <param name="annotationId">The annotation ID.</param>
    /// <param name="tagId">The tag taxonomy node ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddTagAsync(Guid annotationId, Guid tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a tag from an annotation.
    /// </summary>
    /// <param name="annotationId">The annotation ID.</param>
    /// <param name="tagId">The tag taxonomy node ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task RemoveTagAsync(Guid annotationId, Guid tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tag IDs for an annotation.
    /// </summary>
    /// <param name="annotationId">The annotation ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of tag IDs.</returns>
    Task<IReadOnlyList<Guid>> GetTagIdsAsync(Guid annotationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an annotation value (numeric, geospatial, or temporal).
    /// </summary>
    /// <param name="value">The annotation value to save.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task SaveValueAsync(AnnotationValue value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all values for an annotation.
    /// </summary>
    /// <param name="annotationId">The annotation ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of annotation values.</returns>
    Task<IReadOnlyList<AnnotationValue>> GetValuesAsync(Guid annotationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an annotation value.
    /// </summary>
    /// <param name="valueId">The annotation value ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<bool> DeleteValueAsync(Guid valueId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries annotations for a node with optional filters for tags, numeric ranges, temporal ranges, and geospatial bounding box.
    /// </summary>
    /// <param name="nodeId">The topic node ID.</param>
    /// <param name="tagIds">Optional tag IDs to filter by.</param>
    /// <param name="numericMin">Optional minimum numeric value filter.</param>
    /// <param name="numericMax">Optional maximum numeric value filter.</param>
    /// <param name="temporalStart">Optional start date/time filter (ISO 8601).</param>
    /// <param name="temporalEnd">Optional end date/time filter (ISO 8601).</param>
    /// <param name="geoBbox">Optional geospatial bounding box filter (minLat, minLon, maxLat, maxLon).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of annotations matching all specified filters.</returns>
    Task<IReadOnlyList<Annotation>> QueryAsync(
        Guid nodeId,
        IReadOnlyList<Guid>? tagIds = null,
        double? numericMin = null,
        double? numericMax = null,
        DateTime? temporalStart = null,
        DateTime? temporalEnd = null,
        (double minLat, double minLon, double maxLat, double maxLon)? geoBbox = null,
        CancellationToken cancellationToken = default);
}


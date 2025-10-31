using System;

namespace IdeaBranch.Domain;

/// <summary>
/// Represents an optional value associated with an annotation.
/// Supports numeric, geospatial, and temporal values.
/// </summary>
public class AnnotationValue
{
    /// <summary>
    /// Gets the unique identifier for this annotation value.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the annotation this value belongs to.
    /// </summary>
    public Guid AnnotationId { get; private set; }

    /// <summary>
    /// Gets the type of value: "numeric", "geospatial", or "temporal".
    /// </summary>
    public string ValueType { get; private set; }

    /// <summary>
    /// Gets or sets the numeric value (if ValueType is "numeric").
    /// </summary>
    public double? NumericValue { get; set; }

    /// <summary>
    /// Gets or sets the geospatial value as JSON (latitude, longitude, region, etc.).
    /// </summary>
    public string? GeospatialValue { get; set; }

    /// <summary>
    /// Gets or sets the temporal value as ISO 8601 string (point in time, date, or timespan).
    /// </summary>
    public string? TemporalValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the AnnotationValue class.
    /// </summary>
    public AnnotationValue(Guid annotationId, string valueType)
    {
        if (valueType != "numeric" && valueType != "geospatial" && valueType != "temporal")
            throw new ArgumentException("Value type must be 'numeric', 'geospatial', or 'temporal'.", nameof(valueType));

        Id = Guid.NewGuid();
        AnnotationId = annotationId;
        ValueType = valueType;
    }

    /// <summary>
    /// Initializes a new instance with an existing ID (for loading from storage).
    /// </summary>
    public AnnotationValue(
        Guid id,
        Guid annotationId,
        string valueType,
        double? numericValue = null,
        string? geospatialValue = null,
        string? temporalValue = null)
    {
        if (valueType != "numeric" && valueType != "geospatial" && valueType != "temporal")
            throw new ArgumentException("Value type must be 'numeric', 'geospatial', or 'temporal'.", nameof(valueType));

        Id = id;
        AnnotationId = annotationId;
        ValueType = valueType;
        NumericValue = numericValue;
        GeospatialValue = geospatialValue;
        TemporalValue = temporalValue;
    }
}


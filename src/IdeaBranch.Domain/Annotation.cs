using System;

namespace IdeaBranch.Domain;

/// <summary>
/// Represents an annotation attached to a topic node's response text.
/// Supports text span references, tags, optional values (numeric/geospatial/temporal), and comments.
/// </summary>
public class Annotation
{
    /// <summary>
    /// Gets the unique identifier for this annotation.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the topic node this annotation is attached to.
    /// </summary>
    public Guid NodeId { get; private set; }

    /// <summary>
    /// Gets the start offset (character position) of the annotated text span.
    /// </summary>
    public int StartOffset { get; private set; }

    /// <summary>
    /// Gets the end offset (character position) of the annotated text span.
    /// </summary>
    public int EndOffset { get; private set; }

    /// <summary>
    /// Gets or sets the optional comment text for this annotation.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets the timestamp when this annotation was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when this annotation was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Annotation class.
    /// </summary>
    public Annotation(Guid nodeId, int startOffset, int endOffset, string? comment = null)
    {
        if (startOffset < 0)
            throw new ArgumentException("Start offset cannot be negative.", nameof(startOffset));
        if (endOffset < startOffset)
            throw new ArgumentException("End offset must be greater than or equal to start offset.", nameof(endOffset));

        Id = Guid.NewGuid();
        NodeId = nodeId;
        StartOffset = startOffset;
        EndOffset = endOffset;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance with an existing ID (for loading from storage).
    /// </summary>
    public Annotation(
        Guid id,
        Guid nodeId,
        int startOffset,
        int endOffset,
        DateTime createdAt,
        DateTime updatedAt,
        string? comment = null)
    {
        if (startOffset < 0)
            throw new ArgumentException("Start offset cannot be negative.", nameof(startOffset));
        if (endOffset < startOffset)
            throw new ArgumentException("End offset must be greater than or equal to start offset.", nameof(endOffset));

        Id = id;
        NodeId = nodeId;
        StartOffset = startOffset;
        EndOffset = endOffset;
        Comment = comment;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Updates the text span for this annotation.
    /// </summary>
    public void UpdateSpan(int startOffset, int endOffset)
    {
        if (startOffset < 0)
            throw new ArgumentException("Start offset cannot be negative.", nameof(startOffset));
        if (endOffset < startOffset)
            throw new ArgumentException("End offset must be greater than or equal to start offset.", nameof(endOffset));

        StartOffset = startOffset;
        EndOffset = endOffset;
        UpdatedAt = DateTime.UtcNow;
    }
}


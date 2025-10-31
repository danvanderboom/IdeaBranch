using System;

namespace IdeaBranch.Domain;

/// <summary>
/// Represents a historical version of a topic node.
/// Captures the state of a topic node at a specific point in time.
/// </summary>
public class TopicNodeVersion
{
    /// <summary>
    /// Gets the unique identifier for this version entry.
    /// </summary>
    public Guid VersionId { get; private set; }

    /// <summary>
    /// Gets the identifier of the topic node this version belongs to.
    /// </summary>
    public Guid NodeId { get; private set; }

    /// <summary>
    /// Gets the optional title of the topic node at this version.
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// Gets the prompt text at this version.
    /// </summary>
    public string Prompt { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the response text at this version.
    /// </summary>
    public string Response { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the order/position at this version.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Gets the timestamp when this version was created (when the change occurred).
    /// </summary>
    public DateTime VersionTimestamp { get; private set; }

    /// <summary>
    /// Gets the identifier of the user who made this change, or null for system changes.
    /// </summary>
    public string? AuthorId { get; private set; }

    /// <summary>
    /// Gets the display name of the user who made this change, or null if unknown.
    /// </summary>
    public string? AuthorName { get; private set; }

    /// <summary>
    /// Initializes a new instance of the TopicNodeVersion class.
    /// </summary>
    public TopicNodeVersion(
        Guid versionId,
        Guid nodeId,
        string? title,
        string prompt,
        string response,
        int order,
        DateTime versionTimestamp,
        string? authorId = null,
        string? authorName = null)
    {
        VersionId = versionId;
        NodeId = nodeId;
        Title = title;
        Prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
        Response = response ?? string.Empty;
        Order = order;
        VersionTimestamp = versionTimestamp;
        AuthorId = authorId;
        AuthorName = authorName;
    }

    /// <summary>
    /// Creates a version from the current state of a topic node.
    /// </summary>
    public static TopicNodeVersion FromTopicNode(
        TopicNode node,
        string? authorId = null,
        string? authorName = null)
    {
        return new TopicNodeVersion(
            versionId: Guid.NewGuid(),
            nodeId: node.Id,
            title: node.Title,
            prompt: node.Prompt,
            response: node.Response,
            order: node.Order,
            versionTimestamp: DateTime.UtcNow,
            authorId: authorId,
            authorName: authorName);
    }
}


using System;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// Data transfer objects for topic storage.
/// </summary>
public static class TopicEntities
{
    /// <summary>
    /// Represents a row in the Topics table.
    /// </summary>
    public class TopicRow
    {
        /// <summary>
        /// Gets or sets the unique identifier for the topic node.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parent node identifier, or null if this is the root.
        /// </summary>
        public string? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the optional title of the topic.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the prompt text for this topic.
        /// </summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the response text for this topic.
        /// </summary>
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the order/position of this node among its siblings.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this node was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this node was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}


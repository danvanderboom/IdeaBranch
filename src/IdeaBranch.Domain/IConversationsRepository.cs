using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.Domain;

/// <summary>
/// Repository interface for persisting and loading conversation messages (prompts and responses).
/// </summary>
public interface IConversationsRepository
{
    /// <summary>
    /// Gets all conversation messages for a topic node.
    /// </summary>
    /// <param name="nodeId">The topic node ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of conversation messages.</returns>
    Task<IReadOnlyList<ConversationMessage>> GetMessagesByNodeIdAsync(
        Guid nodeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets conversation messages filtered by tag IDs.
    /// </summary>
    /// <param name="tagIds">The tag IDs to filter by.</param>
    /// <param name="includeDescendants">If true, includes messages from nodes tagged with descendant tags.</param>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of conversation messages matching the filters.</returns>
    Task<IReadOnlyList<ConversationMessage>> GetMessagesByTagsAsync(
        IReadOnlyList<Guid> tagIds,
        bool includeDescendants = false,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets conversation messages for specific node IDs.
    /// </summary>
    /// <param name="nodeIds">The node IDs to filter by.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of conversation messages.</returns>
    Task<IReadOnlyList<ConversationMessage>> GetMessagesByNodeIdsAsync(
        IReadOnlyList<Guid> nodeIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a conversation message.
    /// </summary>
    /// <param name="message">The conversation message to save.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task SaveMessageAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a conversation message (prompt or response).
/// </summary>
public class ConversationMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for this message.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the topic node this message belongs to.
    /// </summary>
    public Guid NodeId { get; set; }

    /// <summary>
    /// Gets or sets the message type (prompt or response).
    /// </summary>
    public ConversationMessageType MessageType { get; set; }

    /// <summary>
    /// Gets or sets the message text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when this message was created.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Conversation message types.
/// </summary>
public enum ConversationMessageType
{
    Prompt,
    Response
}


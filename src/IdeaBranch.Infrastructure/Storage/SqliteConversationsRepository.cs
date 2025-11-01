using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// SQLite implementation of IConversationsRepository.
/// Reads conversation messages from topic_nodes (prompts and responses).
/// </summary>
public class SqliteConversationsRepository : IConversationsRepository
{
    private readonly SqliteConnection _connection;
    private readonly IAnnotationsRepository _annotationsRepository;
    private readonly ITagTaxonomyRepository _tagTaxonomyRepository;

    /// <summary>
    /// Initializes a new instance with a SQLite connection.
    /// </summary>
    public SqliteConversationsRepository(
        SqliteConnection connection,
        IAnnotationsRepository annotationsRepository,
        ITagTaxonomyRepository tagTaxonomyRepository)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _annotationsRepository = annotationsRepository ?? throw new ArgumentNullException(nameof(annotationsRepository));
        _tagTaxonomyRepository = tagTaxonomyRepository ?? throw new ArgumentNullException(nameof(tagTaxonomyRepository));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ConversationMessage>> GetMessagesByNodeIdAsync(
        Guid nodeId,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var messages = new List<ConversationMessage>();

            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT NodeId, Prompt, Response, CreatedAt, UpdatedAt
                FROM topic_nodes
                WHERE NodeId = @NodeId
            ";
            command.Parameters.AddWithValue("@NodeId", nodeId.ToString());

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var nodeIdStr = reader.GetString(0);
                var prompt = reader.GetString(1);
                var response = reader.GetString(2);
                var createdAtStr = reader.GetString(3);
                var updatedAtStr = reader.GetString(4);

                if (DateTime.TryParse(createdAtStr, out var createdAt) &&
                    DateTime.TryParse(updatedAtStr, out var updatedAt))
                {
                    // Add prompt message if not empty
                    if (!string.IsNullOrWhiteSpace(prompt))
                    {
                        messages.Add(new ConversationMessage
                        {
                            Id = Guid.NewGuid(), // Generate ID for message
                            NodeId = Guid.Parse(nodeIdStr),
                            MessageType = ConversationMessageType.Prompt,
                            Text = prompt,
                            Timestamp = createdAt
                        });
                    }

                    // Add response message if not empty
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        messages.Add(new ConversationMessage
                        {
                            Id = Guid.NewGuid(), // Generate ID for message
                            NodeId = Guid.Parse(nodeIdStr),
                            MessageType = ConversationMessageType.Response,
                            Text = response,
                            Timestamp = updatedAt
                        });
                    }
                }
            }

            return messages.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ConversationMessage>> GetMessagesByTagsAsync(
        IReadOnlyList<Guid> tagIds,
        bool includeDescendants = false,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            // Resolve tag IDs with descendants if needed
            var resolvedTagIds = new HashSet<Guid>(tagIds);
            if (includeDescendants)
            {
                foreach (var tagId in tagIds)
                {
                    var descendants = await GetTagDescendantsAsync(tagId, cancellationToken);
                    foreach (var descendant in descendants)
                    {
                        resolvedTagIds.Add(descendant);
                    }
                }
            }

            // Find node IDs that have annotations with these tags
            var nodeIds = await GetNodeIdsByTagsAsync(resolvedTagIds.ToList(), cancellationToken);

            if (nodeIds.Count == 0)
            {
                return Array.Empty<ConversationMessage>().AsReadOnly();
            }

            // Get messages for these nodes with date filtering
            return await GetMessagesByNodeIdsWithDateFilterAsync(nodeIds, startDate, endDate, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ConversationMessage>> GetMessagesByNodeIdsAsync(
        IReadOnlyList<Guid> nodeIds,
        CancellationToken cancellationToken = default)
    {
        return await GetMessagesByNodeIdsWithDateFilterAsync(nodeIds, null, null, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveMessageAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            // Since conversations are stored in topic_nodes, we update the appropriate field
            using var command = _connection.CreateCommand();
            
            if (message.MessageType == ConversationMessageType.Prompt)
            {
                command.CommandText = @"
                    UPDATE topic_nodes
                    SET Prompt = @Text, UpdatedAt = @Timestamp
                    WHERE NodeId = @NodeId
                ";
            }
            else // Response
            {
                command.CommandText = @"
                    UPDATE topic_nodes
                    SET Response = @Text, UpdatedAt = @Timestamp
                    WHERE NodeId = @NodeId
                ";
            }

            command.Parameters.AddWithValue("@NodeId", message.NodeId.ToString());
            command.Parameters.AddWithValue("@Text", message.Text);
            command.Parameters.AddWithValue("@Timestamp", message.Timestamp.ToUniversalTime().ToString("O"));

            command.ExecuteNonQuery();
        }, cancellationToken);
    }

    /// <summary>
    /// Gets messages for specific node IDs with optional date filtering.
    /// </summary>
    private async Task<IReadOnlyList<ConversationMessage>> GetMessagesByNodeIdsWithDateFilterAsync(
        IReadOnlyList<Guid> nodeIds,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var messages = new List<ConversationMessage>();

            if (nodeIds.Count == 0)
            {
                return messages.AsReadOnly();
            }

            // Build parameter placeholders for IN clause
            var nodeIdParams = string.Join(",", nodeIds.Select((_, i) => $"@NodeId{i}"));
            var dateFilter = "";
            if (startDate.HasValue || endDate.HasValue)
            {
                var conditions = new List<string>();
                if (startDate.HasValue)
                {
                    conditions.Add("(CreatedAt >= @StartDate OR UpdatedAt >= @StartDate)");
                }
                if (endDate.HasValue)
                {
                    conditions.Add("(CreatedAt <= @EndDate OR UpdatedAt <= @EndDate)");
                }
                dateFilter = "AND " + string.Join(" AND ", conditions);
            }

            using var command = _connection.CreateCommand();
            command.CommandText = $@"
                SELECT NodeId, Prompt, Response, CreatedAt, UpdatedAt
                FROM topic_nodes
                WHERE NodeId IN ({nodeIdParams})
                {dateFilter}
            ";

            // Add node ID parameters
            for (int i = 0; i < nodeIds.Count; i++)
            {
                command.Parameters.AddWithValue($"@NodeId{i}", nodeIds[i].ToString());
            }

            // Add date parameters
            if (startDate.HasValue)
            {
                command.Parameters.AddWithValue("@StartDate", startDate.Value.ToUniversalTime().ToString("O"));
            }
            if (endDate.HasValue)
            {
                command.Parameters.AddWithValue("@EndDate", endDate.Value.ToUniversalTime().ToString("O"));
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var nodeIdStr = reader.GetString(0);
                var prompt = reader.GetString(1);
                var response = reader.GetString(2);
                var createdAtStr = reader.GetString(3);
                var updatedAtStr = reader.GetString(4);

                if (DateTime.TryParse(createdAtStr, out var createdAt) &&
                    DateTime.TryParse(updatedAtStr, out var updatedAt))
                {
                    // Add prompt message if not empty
                    if (!string.IsNullOrWhiteSpace(prompt))
                    {
                        messages.Add(new ConversationMessage
                        {
                            Id = Guid.NewGuid(),
                            NodeId = Guid.Parse(nodeIdStr),
                            MessageType = ConversationMessageType.Prompt,
                            Text = prompt,
                            Timestamp = createdAt
                        });
                    }

                    // Add response message if not empty
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        messages.Add(new ConversationMessage
                        {
                            Id = Guid.NewGuid(),
                            NodeId = Guid.Parse(nodeIdStr),
                            MessageType = ConversationMessageType.Response,
                            Text = response,
                            Timestamp = updatedAt
                        });
                    }
                }
            }

            return messages.AsReadOnly();
        }, cancellationToken);
    }

    /// <summary>
    /// Gets node IDs that have annotations with the specified tags.
    /// </summary>
    private async Task<IReadOnlyList<Guid>> GetNodeIdsByTagsAsync(
        IReadOnlyList<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            if (tagIds.Count == 0)
            {
                return Array.Empty<Guid>().AsReadOnly();
            }

            var tagIdParams = string.Join(",", tagIds.Select((_, i) => $"@TagId{i}"));
            var nodeIds = new HashSet<Guid>();

            using var command = _connection.CreateCommand();
            command.CommandText = $@"
                SELECT DISTINCT a.NodeId
                FROM annotations a
                INNER JOIN annotation_tags at ON a.Id = at.AnnotationId
                WHERE at.TagId IN ({tagIdParams})
            ";

            for (int i = 0; i < tagIds.Count; i++)
            {
                command.Parameters.AddWithValue($"@TagId{i}", tagIds[i].ToString());
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (Guid.TryParse(reader.GetString(0), out var nodeId))
                {
                    nodeIds.Add(nodeId);
                }
            }

            return nodeIds.ToList().AsReadOnly();
        }, cancellationToken);
    }

    /// <summary>
    /// Gets all descendant tag IDs for a given tag ID.
    /// </summary>
    private async Task<IReadOnlyList<Guid>> GetTagDescendantsAsync(Guid tagId, CancellationToken cancellationToken)
    {
        var descendants = new List<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(tagId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = await _tagTaxonomyRepository.GetChildrenAsync(currentId, cancellationToken);
            
            foreach (var child in children)
            {
                descendants.Add(child.Id);
                queue.Enqueue(child.Id);
            }
        }

        return descendants.AsReadOnly();
    }
}


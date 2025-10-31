using System;

namespace IdeaBranch.App.Adapters;

/// <summary>
/// Payload class for TreeNode&lt;TopicNodePayload&gt; to bind in MAUI CollectionView.
/// Represents domain TopicNode data without coupling to CriticalInsight.Data in Domain layer.
/// </summary>
public class TopicNodePayload
{
    public Guid DomainNodeId { get; set; }
    public string? Title { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


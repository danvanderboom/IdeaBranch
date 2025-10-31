using System;
using CriticalInsight.Data.Hierarchical;

namespace IdeaBranch.IntegrationTests.Storage;

/// <summary>
/// Test-specific payload class matching TopicNodePayload structure.
/// Used for creating test trees without referencing the App project.
/// </summary>
public class TestTopicNodePayload
{
    public Guid DomainNodeId { get; set; }
    public string? Title { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


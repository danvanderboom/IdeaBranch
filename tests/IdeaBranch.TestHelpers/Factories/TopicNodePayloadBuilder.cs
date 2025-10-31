using System;

namespace IdeaBranch.TestHelpers.Factories;

/// <summary>
/// Builder for creating TestTopicNodePayload instances with sensible defaults.
/// Supports fluent API for overriding specific fields.
/// </summary>
public class TopicNodePayloadBuilder : EntityBuilder<TestTopicNodePayload>
{
    private Guid? _domainNodeId;
    private string? _title;
    private string? _prompt;
    private string? _response;
    private int? _order;
    private DateTime? _createdAt;
    private DateTime? _updatedAt;

    /// <summary>
    /// Initializes a new instance with an optional seed for deterministic generation.
    /// </summary>
    /// <param name="seed">The seed value. Defaults to 1.</param>
    public TopicNodePayloadBuilder(int? seed = null)
        : base(seed)
    {
    }

    /// <summary>
    /// Sets the domain node ID.
    /// </summary>
    public TopicNodePayloadBuilder WithDomainNodeId(Guid domainNodeId)
    {
        _domainNodeId = domainNodeId;
        return this;
    }

    /// <summary>
    /// Sets the title.
    /// </summary>
    public TopicNodePayloadBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the prompt.
    /// </summary>
    public TopicNodePayloadBuilder WithPrompt(string prompt)
    {
        _prompt = prompt;
        return this;
    }

    /// <summary>
    /// Sets the response.
    /// </summary>
    public TopicNodePayloadBuilder WithResponse(string response)
    {
        _response = response;
        return this;
    }

    /// <summary>
    /// Sets the order/ordinal.
    /// </summary>
    public TopicNodePayloadBuilder WithOrder(int order)
    {
        _order = order;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp.
    /// </summary>
    public TopicNodePayloadBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets the update timestamp.
    /// </summary>
    public TopicNodePayloadBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    /// <summary>
    /// Builds and returns a TestTopicNodePayload with sensible defaults or overridden values.
    /// </summary>
    public override TestTopicNodePayload Build()
    {
        var now = DateTime.UtcNow;
        return new TestTopicNodePayload
        {
            DomainNodeId = _domainNodeId ?? Guid.NewGuid(),
            Title = _title ?? $"Topic {Seed}",
            Prompt = _prompt ?? $"What would you like to explore about Topic {Seed}?",
            Response = _response ?? $"This is a placeholder response for Topic {Seed}.",
            Order = _order ?? 0,
            CreatedAt = _createdAt ?? now,
            UpdatedAt = _updatedAt ?? now
        };
    }
}


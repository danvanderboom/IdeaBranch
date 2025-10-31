using System;
using System.Collections.Generic;
using System.Linq;

namespace IdeaBranch.Domain;

/// <summary>
/// Represents a prompt template in a hierarchical collection.
/// Supports parent/child relationships (categories and templates), name/title, body text with placeholders, and display order.
/// </summary>
public class PromptTemplate
{
    private readonly List<PromptTemplate> _children = new();

    /// <summary>
    /// Gets the unique identifier for this prompt template.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets or sets the parent node ID (category), or null if this is the root.
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// Gets or sets the name/title of this template or category.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the template body text (with optional placeholders like {keyword} or {phrase}).
    /// Null for category nodes, non-null for template nodes.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the display order among siblings.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets the timestamp when this template was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when this template was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Gets the parent node, or null if this is the root.
    /// </summary>
    public PromptTemplate? Parent { get; private set; }

    /// <summary>
    /// Gets a read-only list of child nodes (categories or templates).
    /// </summary>
    public IReadOnlyList<PromptTemplate> Children => _children.AsReadOnly();

    /// <summary>
    /// Gets whether this is a category (has children or no body) or a template (has body).
    /// </summary>
    public bool IsCategory => Body == null;

    /// <summary>
    /// Initializes a new instance of the PromptTemplate class (for categories).
    /// </summary>
    public PromptTemplate(string name, Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        Id = Guid.NewGuid();
        ParentId = parentId;
        Name = name;
        Body = null;
        Order = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the PromptTemplate class (for templates with body).
    /// </summary>
    public PromptTemplate(string name, string body, Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be null or empty for template nodes.", nameof(body));

        Id = Guid.NewGuid();
        ParentId = parentId;
        Name = name;
        Body = body;
        Order = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance with an existing ID (for loading from storage).
    /// </summary>
    public PromptTemplate(
        Guid id,
        Guid? parentId,
        string name,
        string? body,
        int order,
        DateTime createdAt,
        DateTime updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        Id = id;
        ParentId = parentId;
        Name = name;
        Body = body;
        Order = order;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Adds a child node to this node (category or template).
    /// </summary>
    public void AddChild(PromptTemplate child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        if (child.ParentId != null && child.ParentId != Id)
            throw new InvalidOperationException("Child node already has a different parent.");

        if (child == this)
            throw new InvalidOperationException("Cannot add node as its own child.");

        // Check for cycles
        if (child.IsAncestorOf(this))
            throw new InvalidOperationException("Cannot add ancestor as child (would create cycle).");

        if (_children.Any(c => c.Name.Equals(child.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"A sibling with name '{child.Name}' already exists.");

        _children.Add(child);
        child.ParentId = Id;
        child.Parent = this;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a child node from this node.
    /// </summary>
    public bool RemoveChild(PromptTemplate child)
    {
        if (child == null || child.ParentId != Id)
            return false;

        if (_children.Remove(child))
        {
            child.ParentId = null;
            child.Parent = null;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if this node is an ancestor of the specified node.
    /// </summary>
    private bool IsAncestorOf(PromptTemplate node)
    {
        var current = node.Parent;
        while (current != null)
        {
            if (current == this)
                return true;
            current = current.Parent;
        }
        return false;
    }
}


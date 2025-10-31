using System;
using System.Collections.Generic;
using System.Linq;

namespace IdeaBranch.Domain;

/// <summary>
/// Represents a node in a hierarchical tag taxonomy.
/// Supports parent/child relationships, display order, and uniqueness among siblings.
/// </summary>
public class TagTaxonomyNode
{
    private readonly List<TagTaxonomyNode> _children = new();

    /// <summary>
    /// Gets the unique identifier for this tag taxonomy node.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets or sets the parent node ID, or null if this is the root.
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// Gets or sets the name/title of this tag category or tag.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the display order among siblings.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets the timestamp when this node was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when this node was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Gets the parent node, or null if this is the root.
    /// </summary>
    public TagTaxonomyNode? Parent { get; private set; }

    /// <summary>
    /// Gets a read-only list of child nodes.
    /// </summary>
    public IReadOnlyList<TagTaxonomyNode> Children => _children.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the TagTaxonomyNode class.
    /// </summary>
    public TagTaxonomyNode(string name, Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        Id = Guid.NewGuid();
        ParentId = parentId;
        Name = name;
        Order = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance with an existing ID (for loading from storage).
    /// </summary>
    public TagTaxonomyNode(
        Guid id,
        Guid? parentId,
        string name,
        int order,
        DateTime createdAt,
        DateTime updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        Id = id;
        ParentId = parentId;
        Name = name;
        Order = order;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    public void AddChild(TagTaxonomyNode child)
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
    public bool RemoveChild(TagTaxonomyNode child)
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
    private bool IsAncestorOf(TagTaxonomyNode node)
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


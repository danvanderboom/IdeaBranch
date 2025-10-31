using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IdeaBranch.Domain;

/// <summary>
/// Aggregate root for a topic node in a hierarchical topic tree.
/// Each node pairs a prompt with a response and can have child nodes.
/// </summary>
public class TopicNode
{
    private readonly List<TopicNode> _children = new();

    /// <summary>
    /// Gets the unique identifier for this node.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets or sets the optional title of this topic.
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
    public TopicNode? Parent { get; private set; }

    /// <summary>
    /// Gets a read-only list of child nodes.
    /// </summary>
    public IReadOnlyList<TopicNode> Children => _children.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the TopicNode class.
    /// </summary>
    public TopicNode(string prompt, string? title = null)
    {
        Id = Guid.NewGuid();
        Prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
        Title = title;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance with an existing ID (for loading from storage).
    /// </summary>
    internal TopicNode(Guid id, string prompt, DateTime createdAt, DateTime updatedAt, string? title = null)
    {
        Id = id;
        Prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
        Title = title;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    /// <param name="child">The child node to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when child is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when child already has a parent or is this node.</exception>
    public void AddChild(TopicNode child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        if (child.Parent != null)
            throw new InvalidOperationException("Child node already has a parent.");

        if (child == this)
            throw new InvalidOperationException("Cannot add node as its own child.");

        // Check for cycles
        if (IsAncestorOf(child))
            throw new InvalidOperationException("Cannot add ancestor as child (would create cycle).");

        _children.Add(child);
        child.Parent = this;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a child node from this node.
    /// </summary>
    /// <param name="child">The child node to remove.</param>
    /// <returns>True if the child was removed; false if it was not a child of this node.</returns>
    public bool RemoveChild(TopicNode child)
    {
        if (child == null || child.Parent != this)
            return false;

        if (_children.Remove(child))
        {
            child.Parent = null;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Moves a child node to a new parent.
    /// </summary>
    /// <param name="child">The child node to move.</param>
    /// <param name="newParent">The new parent node.</param>
    /// <exception cref="ArgumentNullException">Thrown when child or newParent is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when child is not a child of this node.</exception>
    public void MoveChild(TopicNode child, TopicNode newParent)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        if (newParent == null)
            throw new ArgumentNullException(nameof(newParent));

        if (child.Parent != this)
            throw new InvalidOperationException("Child is not a child of this node.");

        if (!RemoveChild(child))
            throw new InvalidOperationException("Failed to remove child from current parent.");

        newParent.AddChild(child);
    }

    /// <summary>
    /// Sets the response text and optionally parses it for list items to create child nodes.
    /// </summary>
    /// <param name="response">The response text to set.</param>
    /// <param name="parseListItems">If true, parses the response for list items and creates child nodes.</param>
    public void SetResponse(string response, bool parseListItems = false)
    {
        Response = response ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;

        if (parseListItems && !string.IsNullOrWhiteSpace(response))
        {
            var listItems = ParseListItems(response);
            foreach (var item in listItems)
            {
                // Only add if we don't already have a child with this prompt
                if (!_children.Any(c => c.Prompt.Equals(item, StringComparison.OrdinalIgnoreCase)))
                {
                    var childNode = new TopicNode(item.Trim());
                    AddChild(childNode);
                }
            }
        }
    }

    /// <summary>
    /// Checks if this node is an ancestor of the specified node.
    /// </summary>
    private bool IsAncestorOf(TopicNode node)
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

    /// <summary>
    /// Parses list items from response text.
    /// Supports numbered lists (1., 2., etc.), bullet points (-, *, •), and line-separated items.
    /// </summary>
    private static List<string> ParseListItems(string response)
    {
        var items = new List<string>();
        var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            // Match numbered lists: "1. Item", "2) Item", etc.
            var numberedMatch = Regex.Match(trimmed, @"^\d+[.)]\s*(.+)$");
            if (numberedMatch.Success)
            {
                items.Add(numberedMatch.Groups[1].Value.Trim());
                continue;
            }

            // Match bullet points: "- Item", "* Item", "• Item", etc.
            var bulletMatch = Regex.Match(trimmed, @"^[-*•]\s*(.+)$");
            if (bulletMatch.Success)
            {
                items.Add(bulletMatch.Groups[1].Value.Trim());
                continue;
            }

            // If no list marker, treat as a standalone item if it's not too long
            // (to avoid treating paragraphs as list items)
            if (trimmed.Length < 200)
            {
                items.Add(trimmed);
            }
        }

        return items;
    }
}

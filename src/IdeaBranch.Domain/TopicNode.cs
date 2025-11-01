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

        // Check for cycles - if the child being added is an ancestor of this node, adding it would create a cycle
        if (child.IsAncestorOf(this))
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
            var listBlocks = ParseListBlocks(response);
            
            // If there's only one list block, use existing behavior (direct children)
            if (listBlocks.Count == 1)
            {
                var block = listBlocks[0];
                foreach (var item in block.Items)
                {
                    // Only add if we don't already have a child with this prompt
                    if (!_children.Any(c => c.Prompt.Equals(item, StringComparison.OrdinalIgnoreCase)))
                    {
                        var childNode = new TopicNode(item.Trim());
                        AddChild(childNode);
                    }
                }
            }
            else if (listBlocks.Count > 1)
            {
                // Multiple lists: create intermediate nodes
                foreach (var block in listBlocks)
                {
                    var title = block.Title ?? $"List {block.Index}";
                    var titleTrimmed = title.TrimEnd(':', '.', ' ').Trim();
                    
                    // Only create intermediate node if we don't already have a child with this title
                    var existingIntermediate = _children.FirstOrDefault(c => 
                        (c.Title?.Equals(titleTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (c.Prompt?.Equals(titleTrimmed, StringComparison.OrdinalIgnoreCase) ?? false));
                    
                    TopicNode intermediateNode;
                    if (existingIntermediate != null)
                    {
                        intermediateNode = existingIntermediate;
                    }
                    else
                    {
                        intermediateNode = new TopicNode(titleTrimmed, titleTrimmed);
                        AddChild(intermediateNode);
                    }
                    
                    // Add list items as children of the intermediate node
                    foreach (var item in block.Items)
                    {
                        // Only add if we don't already have a child with this prompt
                        if (!intermediateNode.Children.Any(c => c.Prompt.Equals(item, StringComparison.OrdinalIgnoreCase)))
                        {
                            var childNode = new TopicNode(item.Trim());
                            intermediateNode.AddChild(childNode);
                        }
                    }
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
    /// Represents a block of contiguous list items with an optional title.
    /// </summary>
    private class ListBlock
    {
        public string? Title { get; set; }
        public List<string> Items { get; set; } = new();
        public int Index { get; set; }
    }

    /// <summary>
    /// Parses response text into blocks of list items, detecting intermediate titles.
    /// Supports numbered lists (1., 2., etc.), bullet points (-, *, •), and line-separated items.
    /// </summary>
    private static List<ListBlock> ParseListBlocks(string response)
    {
        var blocks = new List<ListBlock>();
        // Normalize line endings first, then split to avoid double empty strings from \r\n
        var normalized = response.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = normalized.Split('\n', StringSplitOptions.None);
        
        ListBlock? currentBlock = null;
        string? pendingTitle = null;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();

            // Empty lines close the current block (but preserve pending title for next block)
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                if (currentBlock != null && currentBlock.Items.Count > 0)
                {
                    // Current block is complete
                    currentBlock = null;
                }
                // Don't clear pendingTitle here - it should be available for the next list block
                continue;
            }

            // Check if this line is a list item
            bool isListItem = false;
            string? itemText = null;

            // Match numbered lists: "1. Item", "2) Item", etc.
            var numberedMatch = Regex.Match(trimmed, @"^\d+[.)]\s*(.+)$");
            if (numberedMatch.Success)
            {
                isListItem = true;
                itemText = numberedMatch.Groups[1].Value.Trim();
            }

            // Match bullet points: "- Item", "* Item", "• Item", etc.
            if (!isListItem)
            {
                var bulletMatch = Regex.Match(trimmed, @"^[-*•]\s*(.+)$");
                if (bulletMatch.Success)
                {
                    isListItem = true;
                    itemText = bulletMatch.Groups[1].Value.Trim();
                }
            }

            // If no list marker, treat as a standalone item if it's not too long
            // (to avoid treating paragraphs as list items)
            // Don't treat title-like lines (ending with colons or very short) as list items
            if (!isListItem && trimmed.Length < 200 && !trimmed.EndsWith(':') && trimmed.Length > 10)
            {
                isListItem = true;
                itemText = trimmed;
            }

            if (isListItem && !string.IsNullOrWhiteSpace(itemText))
            {
                // Start a new block if we don't have one (transition from non-list to list)
                if (currentBlock == null)
                {
                    currentBlock = new ListBlock
                    {
                        Title = pendingTitle,
                        Index = blocks.Count + 1
                    };
                    blocks.Add(currentBlock);
                    pendingTitle = null;
                }

                // Only add if we don't already have this item
                if (!currentBlock.Items.Any(item => item.Equals(itemText, StringComparison.OrdinalIgnoreCase)))
                {
                    currentBlock.Items.Add(itemText);
                }
            }
            else
            {
                // Non-list line: this ends the current block and becomes a potential title for the next list block
                if (currentBlock != null && currentBlock.Items.Count > 0)
                {
                    // Current block is complete
                    currentBlock = null;
                }

                // This line becomes a potential title for the next list block
                // Lines ending with colons are strong candidates for titles (headings)
                if (trimmed.EndsWith(':'))
                {
                    // Remove the colon for the title
                    pendingTitle = trimmed.Substring(0, trimmed.Length - 1).Trim();
                }
                else if (trimmed.Length <= 100 && !trimmed.EndsWith('.') && !trimmed.EndsWith('!') && !trimmed.EndsWith('?'))
                {
                    // Use it if it's not too long and doesn't look like a paragraph
                    pendingTitle = trimmed;
                }
                else if (trimmed.Length <= 80)
                {
                    // Allow slightly longer titles even with punctuation
                    pendingTitle = trimmed;
                }
                else
                {
                    // Clear pending title if it looks like a paragraph
                    pendingTitle = null;
                }
            }
        }

        // Filter out empty blocks (blocks with no items)
        blocks = blocks.Where(b => b.Items.Count > 0).ToList();
        
        // Ensure all blocks have titles (fallback to "List N")
        for (int i = 0; i < blocks.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(blocks[i].Title))
            {
                blocks[i].Title = $"List {i + 1}";
            }
            else
            {
                // Trim any trailing colons from titles (shouldn't be needed but just in case)
                // Title is guaranteed to be non-null here because IsNullOrWhiteSpace would have caught null
                blocks[i].Title = blocks[i].Title!.TrimEnd(':', ' ', '\t');
            }
        }

        return blocks;
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

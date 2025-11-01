using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.Domain;

/// <summary>
/// Repository interface for persisting and loading hierarchical tag taxonomies.
/// </summary>
public interface ITagTaxonomyRepository
{
    /// <summary>
    /// Gets the root node of the tag taxonomy, or creates a default root if none exists.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The root tag taxonomy node.</returns>
    Task<TagTaxonomyNode> GetRootAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tag taxonomy node by its ID.
    /// </summary>
    /// <param name="id">The node ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The tag taxonomy node, or null if not found.</returns>
    Task<TagTaxonomyNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all child nodes of a parent node.
    /// </summary>
    /// <param name="parentId">The parent node ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of child nodes, ordered by Order.</returns>
    Task<IReadOnlyList<TagTaxonomyNode>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a tag taxonomy node (upsert by ID).
    /// </summary>
    /// <param name="node">The node to save.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task SaveAsync(TagTaxonomyNode node, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a tag taxonomy node.
    /// </summary>
    /// <param name="id">The node ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the node was found and deleted; false otherwise.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches tag taxonomy nodes by name (LIKE) and UpdatedAt range.
    /// </summary>
    /// <param name="nameContains">Optional text to search for in Name (LIKE %text%).</param>
    /// <param name="updatedAtFrom">Optional start of UpdatedAt range filter (inclusive).</param>
    /// <param name="updatedAtTo">Optional end of UpdatedAt range filter (inclusive).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of tag taxonomy nodes matching the search criteria.</returns>
    Task<IReadOnlyList<TagTaxonomyNode>> SearchAsync(
        string? nameContains = null,
        DateTime? updatedAtFrom = null,
        DateTime? updatedAtTo = null,
        CancellationToken cancellationToken = default);
}


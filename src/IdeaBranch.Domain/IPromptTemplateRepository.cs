using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.Domain;

/// <summary>
/// Repository interface for persisting and loading hierarchical prompt template collections.
/// </summary>
public interface IPromptTemplateRepository
{
    /// <summary>
    /// Gets the root node of the prompt template collection, or creates a default root if none exists.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The root prompt template node.</returns>
    Task<PromptTemplate> GetRootAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prompt template by its ID.
    /// </summary>
    /// <param name="id">The template ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The prompt template, or null if not found.</returns>
    Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all child nodes (categories or templates) of a parent node.
    /// </summary>
    /// <param name="parentId">The parent node ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of child nodes, ordered by Order.</returns>
    Task<IReadOnlyList<PromptTemplate>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a template by its hierarchical path (e.g., "Information Retrieval/Definitions and explanations").
    /// </summary>
    /// <param name="path">The hierarchical path, with '/' as separator.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The prompt template, or null if not found.</returns>
    Task<PromptTemplate?> GetByPathAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all templates within a subtree (recursively).
    /// </summary>
    /// <param name="parentId">The parent node ID to start from.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of all templates (not categories) in the subtree.</returns>
    Task<IReadOnlyList<PromptTemplate>> GetSubtreeAsync(Guid? parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a prompt template (upsert by ID).
    /// </summary>
    /// <param name="template">The template to save.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task SaveAsync(PromptTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a prompt template.
    /// </summary>
    /// <param name="id">The template ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the template was found and deleted; false otherwise.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches prompt templates by name/body (LIKE) and UpdatedAt range.
    /// </summary>
    /// <param name="textContains">Optional text to search for in Name or Body (LIKE %text%).</param>
    /// <param name="updatedAtFrom">Optional start of UpdatedAt range filter (inclusive).</param>
    /// <param name="updatedAtTo">Optional end of UpdatedAt range filter (inclusive).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of prompt templates matching the search criteria.</returns>
    Task<IReadOnlyList<PromptTemplate>> SearchAsync(
        string? textContains = null,
        DateTime? updatedAtFrom = null,
        DateTime? updatedAtTo = null,
        CancellationToken cancellationToken = default);
}


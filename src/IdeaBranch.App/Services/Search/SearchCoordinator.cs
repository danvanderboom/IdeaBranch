using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;

namespace IdeaBranch.App.Services.Search;

/// <summary>
/// Coordinates searches across different content types: nodes, annotations, tags, and templates.
/// </summary>
public class SearchCoordinator
{
    private readonly IAnnotationsRepository _annotationsRepository;
    private readonly ITagTaxonomyRepository _tagTaxonomyRepository;
    private readonly IPromptTemplateRepository _promptTemplateRepository;
    private readonly ITopicTreeRepository _topicTreeRepository;

    /// <summary>
    /// Initializes a new instance with required repositories.
    /// </summary>
    public SearchCoordinator(
        IAnnotationsRepository annotationsRepository,
        ITagTaxonomyRepository tagTaxonomyRepository,
        IPromptTemplateRepository promptTemplateRepository,
        ITopicTreeRepository topicTreeRepository)
    {
        _annotationsRepository = annotationsRepository ?? throw new ArgumentNullException(nameof(annotationsRepository));
        _tagTaxonomyRepository = tagTaxonomyRepository ?? throw new ArgumentNullException(nameof(tagTaxonomyRepository));
        _promptTemplateRepository = promptTemplateRepository ?? throw new ArgumentNullException(nameof(promptTemplateRepository));
        _topicTreeRepository = topicTreeRepository ?? throw new ArgumentNullException(nameof(topicTreeRepository));
    }

    /// <summary>
    /// Performs a search across the specified content types.
    /// </summary>
    /// <param name="request">The search request with content types and filters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Search results grouped by content type.</returns>
    public async Task<SearchResults> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var results = new SearchResults
        {
            RequestedContentTypes = request.ContentTypes.ToList()
        };

        // Search each requested content type
        if (request.ContentTypes.Contains(SearchContentType.TopicNodes))
        {
            results.Nodes = await SearchNodesAsync(request, cancellationToken);
        }

        if (request.ContentTypes.Contains(SearchContentType.Annotations))
        {
            results.Annotations = await SearchAnnotationsAsync(request, cancellationToken);
        }

        if (request.ContentTypes.Contains(SearchContentType.Tags))
        {
            results.Tags = await SearchTagsAsync(request, cancellationToken);
        }

        if (request.ContentTypes.Contains(SearchContentType.PromptTemplates))
        {
            results.Templates = await SearchTemplatesAsync(request, cancellationToken);
        }

        return results;
    }

    private async Task<IReadOnlyList<TopicNodeSearchResult>> SearchNodesAsync(
        SearchRequest request,
        CancellationToken cancellationToken)
    {
        // Load the root tree
        var root = await _topicTreeRepository.GetRootAsync(cancellationToken);
        
        // Collect all nodes recursively
        var allNodes = new List<TopicNode>();
        CollectNodesRecursive(root, allNodes);

        // Filter nodes
        var filtered = allNodes.Where(node =>
        {
            // Text search in Prompt or Response
            if (!string.IsNullOrWhiteSpace(request.TextContains))
            {
                var matchesText = 
                    (node.Prompt?.Contains(request.TextContains, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (node.Response?.Contains(request.TextContains, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (node.Title?.Contains(request.TextContains, StringComparison.OrdinalIgnoreCase) ?? false);
                
                if (!matchesText)
                    return false;
            }

            // UpdatedAt range filter
            if (request.UpdatedAtFrom.HasValue && node.UpdatedAt < request.UpdatedAtFrom.Value)
                return false;

            if (request.UpdatedAtTo.HasValue && node.UpdatedAt > request.UpdatedAtTo.Value)
                return false;

            return true;
        }).ToList();

        return filtered.Select(node => new TopicNodeSearchResult
        {
            NodeId = node.Id,
            Title = node.Title,
            Prompt = node.Prompt,
            Response = node.Response,
            UpdatedAt = node.UpdatedAt
        }).ToList();
    }

    private void CollectNodesRecursive(TopicNode node, List<TopicNode> collection)
    {
        collection.Add(node);
        foreach (var child in node.Children)
        {
            CollectNodesRecursive(child, collection);
        }
    }

    private async Task<IReadOnlyList<Annotation>> SearchAnnotationsAsync(
        SearchRequest request,
        CancellationToken cancellationToken)
    {
        // Build search options for annotations
        var options = new AnnotationsSearchOptions
        {
            IncludeTags = request.IncludeTags?.ToList(),
            ExcludeTags = request.ExcludeTags?.ToList(),
            CommentContains = request.TextContains,
            UpdatedAtFrom = request.UpdatedAtFrom,
            UpdatedAtTo = request.UpdatedAtTo,
            TemporalStart = request.TemporalStart,
            TemporalEnd = request.TemporalEnd,
            TagWeightFilters = request.TagWeightFilters?.ToList(),
            PageSize = request.PageSize,
            PageOffset = request.PageOffset
        };

        // For annotations, we need to search per node. For now, search across all nodes.
        // In a real implementation, we might want to search across all annotations or limit by node.
        var root = await _topicTreeRepository.GetRootAsync(cancellationToken);
        var allNodes = new List<TopicNode>();
        CollectNodesRecursive(root, allNodes);

        var allResults = new List<Annotation>();
        foreach (var node in allNodes)
        {
            var nodeResults = await _annotationsRepository.SearchAsync(node.Id, options, cancellationToken);
            allResults.AddRange(nodeResults);
        }

        // Remove duplicates by ID
        return allResults.GroupBy(a => a.Id).Select(g => g.First()).ToList();
    }

    private async Task<IReadOnlyList<TagTaxonomyNode>> SearchTagsAsync(
        SearchRequest request,
        CancellationToken cancellationToken)
    {
        return await _tagTaxonomyRepository.SearchAsync(
            nameContains: request.TextContains,
            updatedAtFrom: request.UpdatedAtFrom,
            updatedAtTo: request.UpdatedAtTo,
            cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<PromptTemplate>> SearchTemplatesAsync(
        SearchRequest request,
        CancellationToken cancellationToken)
    {
        return await _promptTemplateRepository.SearchAsync(
            textContains: request.TextContains,
            updatedAtFrom: request.UpdatedAtFrom,
            updatedAtTo: request.UpdatedAtTo,
            cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Content types that can be searched.
/// </summary>
public enum SearchContentType
{
    TopicNodes,
    Annotations,
    Tags,
    PromptTemplates
}

/// <summary>
/// Search request with filters applicable across content types.
/// </summary>
public class SearchRequest
{
    /// <summary>
    /// Gets or sets the content types to search.
    /// </summary>
    public IReadOnlySet<SearchContentType> ContentTypes { get; set; } = new HashSet<SearchContentType>();

    /// <summary>
    /// Gets or sets text to search for (applies to relevant fields per content type).
    /// </summary>
    public string? TextContains { get; set; }

    /// <summary>
    /// Gets or sets tag IDs to include (AND logic).
    /// </summary>
    public IReadOnlyList<Guid>? IncludeTags { get; set; }

    /// <summary>
    /// Gets or sets tag IDs to exclude.
    /// </summary>
    public IReadOnlyList<Guid>? ExcludeTags { get; set; }

    /// <summary>
    /// Gets or sets tag weight filters (for annotations).
    /// </summary>
    public IReadOnlyList<TagWeightFilter>? TagWeightFilters { get; set; }

    /// <summary>
    /// Gets or sets the start of the UpdatedAt range filter (inclusive).
    /// </summary>
    public DateTime? UpdatedAtFrom { get; set; }

    /// <summary>
    /// Gets or sets the end of the UpdatedAt range filter (inclusive).
    /// </summary>
    public DateTime? UpdatedAtTo { get; set; }

    /// <summary>
    /// Gets or sets the start of the temporal/historical time range filter (inclusive, for annotations).
    /// </summary>
    public DateTime? TemporalStart { get; set; }

    /// <summary>
    /// Gets or sets the end of the temporal/historical time range filter (inclusive, for annotations).
    /// </summary>
    public DateTime? TemporalEnd { get; set; }

    /// <summary>
    /// Gets or sets the page size for pagination.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets the page offset for pagination.
    /// </summary>
    public int? PageOffset { get; set; }
}

/// <summary>
/// Search results grouped by content type.
/// </summary>
public class SearchResults
{
    /// <summary>
    /// Gets or sets the content types that were requested.
    /// </summary>
    public IReadOnlyList<SearchContentType> RequestedContentTypes { get; set; } = Array.Empty<SearchContentType>();

    /// <summary>
    /// Gets or sets the topic node search results.
    /// </summary>
    public IReadOnlyList<TopicNodeSearchResult> Nodes { get; set; } = Array.Empty<TopicNodeSearchResult>();

    /// <summary>
    /// Gets or sets the annotation search results.
    /// </summary>
    public IReadOnlyList<Annotation> Annotations { get; set; } = Array.Empty<Annotation>();

    /// <summary>
    /// Gets or sets the tag search results.
    /// </summary>
    public IReadOnlyList<TagTaxonomyNode> Tags { get; set; } = Array.Empty<TagTaxonomyNode>();

    /// <summary>
    /// Gets or sets the prompt template search results.
    /// </summary>
    public IReadOnlyList<PromptTemplate> Templates { get; set; } = Array.Empty<PromptTemplate>();
}

/// <summary>
/// Search result for a topic node.
/// </summary>
public class TopicNodeSearchResult
{
    /// <summary>
    /// Gets or sets the node ID.
    /// </summary>
    public Guid NodeId { get; set; }

    /// <summary>
    /// Gets or sets the node title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the prompt text.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response text.
    /// </summary>
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last updated timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}


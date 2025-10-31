using System;
using System.Collections.Generic;

namespace CriticalInsight.Data.Agents;

public enum AgentErrorCode
{
    invalid_argument,
    not_found,
    forbidden,
    conflict,
    rate_limited,
    internal_error,
    validation_failed,
    deserialization_failed
}

public sealed class AgentError
{
    public AgentErrorCode Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RetryAfter { get; set; }
}

public sealed class AgentResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public AgentError? Error { get; set; }

    public static AgentResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static AgentResult<T> Fail(AgentErrorCode code, string message, string? retryAfter = null) => new()
    {
        Success = false,
        Error = new AgentError { Code = code, Message = message, RetryAfter = retryAfter }
    };
}

public sealed class PageOptions
{
    public int? PageSize { get; set; }
    public string? PageToken { get; set; }
}

public sealed class Page<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public string? NextPageToken { get; set; }
}

public sealed class PropertyFilters
{
    public List<string> IncludedProperties { get; set; } = new();
    public List<string> ExcludedProperties { get; set; } = new();
}

public sealed class ViewOptions
{
    public bool IncludeViewRoot { get; set; }
    public bool DefaultExpanded { get; set; } = true;
    public int? DepthLimit { get; set; }
    public PropertyFilters Filters { get; set; } = new();
    public PageOptions Paging { get; set; } = new();
}

public sealed class SearchFilter
{
    public string Path { get; set; } = string.Empty;
    public string Op { get; set; } = "eq"; // eq | contains
    public string Value { get; set; } = string.Empty;
}

public sealed class MutationOptions
{
    public string? VersionToken { get; set; }
    public string? IdempotencyKey { get; set; }
}

public sealed class PathItem
{
    public string NodeId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int Depth { get; set; }
}

public sealed class PathResult
{
    public IReadOnlyList<PathItem> Path { get; set; } = Array.Empty<PathItem>();
}

public sealed class SearchPredicate
{
    public string Path { get; set; } = string.Empty;
    public string Op { get; set; } = "eq"; // eq | contains | gt | lt | between
    public string Value { get; set; } = string.Empty;
    public string? Value2 { get; set; } // for 'between' operation
}

public sealed class SearchGroup
{
    public string Op { get; set; } = "and"; // and | or
    public List<SearchPredicate> Predicates { get; set; } = new();
    public List<SearchGroup> Groups { get; set; } = new();
}

public sealed class AdvancedSearchOptions
{
    public SearchGroup? RootGroup { get; set; }
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "asc"; // asc | desc
    public bool Stable { get; set; } = true;
}

public sealed class SelectQuery
{
    public string Expression { get; set; } = string.Empty; // Simple DSL for node selection
}

public sealed class CopyOptions
{
    public string Mode { get; set; } = "duplicate"; // duplicate | reference
}

public sealed class SortOptions
{
    public string ByProperty { get; set; } = string.Empty;
    public string Direction { get; set; } = "asc"; // asc | desc
    public bool Stable { get; set; } = true;
}

public sealed class BulkUpdateItem
{
    public string NodeId { get; set; } = string.Empty;
    public Dictionary<string, object?> Properties { get; set; } = new();
}

public sealed class BulkUpdateOptions
{
    public bool ContinueOnError { get; set; } = true;
    public bool ValidateBeforeUpdate { get; set; } = true;
}

public sealed class ExpansionOptions
{
    public bool Expanded { get; set; } = true;
    public int? MaxDepth { get; set; }
    public bool IncludeRoot { get; set; } = true;
}

public sealed class ViewFilterOptions
{
    public List<string> IncludedProperties { get; set; } = new();
    public List<string> ExcludedProperties { get; set; } = new();
    public bool ReplaceExisting { get; set; } = true;
}

public sealed class TagOptions
{
    public List<string> Tags { get; set; } = new();
    public bool ReplaceExisting { get; set; } = false;
}

public sealed class BookmarkOptions
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, object?> Metadata { get; set; } = new();
}

public sealed class ValidationOptions
{
    public bool CheckStructure { get; set; } = true;
    public bool CheckPayloads { get; set; } = true;
    public bool CheckReferences { get; set; } = true;
    public List<string> RequiredProperties { get; set; } = new();
}

public sealed class DiffOptions
{
    public bool IncludePayloads { get; set; } = true;
    public bool IncludeStructure { get; set; } = true;
    public bool IncludeMetadata { get; set; } = false;
    public int? MaxDepth { get; set; }
}

public sealed class SnapshotOptions
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, object?> Metadata { get; set; } = new();
    public bool IncludeViewState { get; set; } = true;
    public bool IncludeTags { get; set; } = true;
    public bool IncludeBookmarks { get; set; } = false;
}

public sealed class RestoreOptions
{
    public bool RestoreViewState { get; set; } = true;
    public bool RestoreTags { get; set; } = true;
    public bool RestoreBookmarks { get; set; } = false;
    public bool ValidateBeforeRestore { get; set; } = true;
}

public sealed class ExportOptions
{
    public bool IncludeViewState { get; set; } = true;
    public bool IncludeTags { get; set; } = true;
    public bool IncludeBookmarks { get; set; } = false;
    public bool IncludeMetadata { get; set; } = true;
    public string Format { get; set; } = "json"; // json, xml, csv
    public bool Compress { get; set; } = false;
}

public sealed class ImportOptions
{
    public bool ValidateBeforeImport { get; set; } = true;
    public bool MergeMode { get; set; } = false; // false = replace, true = merge
    public bool PreserveNodeIds { get; set; } = false;
    public Dictionary<string, string> NodeIdMapping { get; set; } = new();
}



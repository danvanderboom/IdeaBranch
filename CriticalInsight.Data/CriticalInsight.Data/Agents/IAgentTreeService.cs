using System;
using System.Collections.Generic;
using CriticalInsight.Data.Hierarchical;

namespace CriticalInsight.Data.Agents;

public interface IAgentTreeService
{
    // Read
    AgentResult<string> GetNode(AgentContext ctx, string nodeId, PropertyFilters filters);
    AgentResult<string> GetView(AgentContext ctx, string rootNodeId, ViewOptions options);
    AgentResult<Page<string>> ListChildren(AgentContext ctx, string nodeId, PageOptions paging);
    AgentResult<Page<string>> Search(AgentContext ctx, string rootNodeId, IEnumerable<SearchFilter> filters, PageOptions paging);

    // Mutations
    AgentResult<string> ExpandNode(AgentContext ctx, string nodeId, MutationOptions opts);
    AgentResult<string> CollapseNode(AgentContext ctx, string nodeId, MutationOptions opts);
    AgentResult<string> ToggleNode(AgentContext ctx, string nodeId, MutationOptions opts);
    AgentResult<string> ExpandAll(AgentContext ctx, string rootNodeId, MutationOptions opts);
    AgentResult<string> CollapseAll(AgentContext ctx, string rootNodeId, MutationOptions opts);
    AgentResult<string> AddChild(AgentContext ctx, string parentNodeId, string payloadTypeName, IDictionary<string, object?> payloadProps, MutationOptions opts);
    AgentResult<string> UpdatePayloadProperty(AgentContext ctx, string nodeId, string propertyName, object? newValue, MutationOptions opts);
    AgentResult<string> RemoveNode(AgentContext ctx, string nodeId, MutationOptions opts);
    AgentResult<string> MoveNode(AgentContext ctx, string nodeId, string newParentId, MutationOptions opts);
    AgentResult<string> ExportView(AgentContext ctx, string rootNodeId, bool includeViewRoot);
    AgentResult<string> ImportView(AgentContext ctx, string viewJson, IDictionary<string, Type> payloadTypes, Func<string, ITreeNode?> nodeLookup, MutationOptions opts);

    // Extended: Navigation & Retrieval
    AgentResult<PathResult> GetPath(AgentContext ctx, string nodeId);
    AgentResult<string> GetSubtree(AgentContext ctx, string nodeId, int? depthLimit, PropertyFilters filters, PageOptions paging);
    AgentResult<string> GetCommonAncestor(AgentContext ctx, IEnumerable<string> nodeIds);

    // Extended: Advanced Search & Selection
    AgentResult<Page<string>> SearchAdvanced(AgentContext ctx, string rootNodeId, AdvancedSearchOptions options, PageOptions paging);
    AgentResult<Page<string>> SelectNodes(AgentContext ctx, string rootNodeId, SelectQuery query, PageOptions paging);

    // Extended: Structural Editing
    AgentResult<string> CopySubtree(AgentContext ctx, string sourceNodeId, string targetParentId, CopyOptions options, MutationOptions mutationOpts);
    AgentResult<string> CloneNode(AgentContext ctx, string nodeId, string targetParentId, MutationOptions mutationOpts);
    AgentResult<string> MoveBefore(AgentContext ctx, string nodeId, string siblingId, MutationOptions mutationOpts);
    AgentResult<string> MoveAfter(AgentContext ctx, string nodeId, string siblingId, MutationOptions mutationOpts);
    AgentResult<string> SortChildren(AgentContext ctx, string parentId, SortOptions options, MutationOptions mutationOpts);

    // Extended: Bulk Update
    AgentResult<Dictionary<string, string>> UpdatePayload(AgentContext ctx, string nodeId, Dictionary<string, object?> properties, MutationOptions mutationOpts);
    AgentResult<Dictionary<string, string>> UpdateNodes(AgentContext ctx, IEnumerable<BulkUpdateItem> updates, BulkUpdateOptions options, MutationOptions mutationOpts);

    // Extended: View Control
    AgentResult<string> SetExpansionRecursive(AgentContext ctx, string rootNodeId, ExpansionOptions options, MutationOptions mutationOpts);
    AgentResult<string> SetFilters(AgentContext ctx, ViewFilterOptions options, MutationOptions mutationOpts);

    // Extended: Tagging & Bookmarks
    AgentResult<List<string>> AddTags(AgentContext ctx, string nodeId, TagOptions options, MutationOptions mutationOpts);
    AgentResult<List<string>> RemoveTags(AgentContext ctx, string nodeId, List<string> tags, MutationOptions mutationOpts);
    AgentResult<List<string>> GetTags(AgentContext ctx, string nodeId);
    AgentResult<Page<string>> FindNodesByTag(AgentContext ctx, string rootNodeId, string tag, PageOptions paging);
    AgentResult<string> CreateBookmark(AgentContext ctx, string nodeId, BookmarkOptions options, MutationOptions mutationOpts);
    AgentResult<List<string>> ListBookmarks(AgentContext ctx);
    AgentResult<string> DeleteBookmark(AgentContext ctx, string bookmarkId, MutationOptions mutationOpts);

    // Extended: Validation & Diff
    AgentResult<List<string>> ValidateTree(AgentContext ctx, string rootNodeId, ValidationOptions options);
    AgentResult<string> DiffTrees(AgentContext ctx, string rootNodeId1, string rootNodeId2, DiffOptions options);
    AgentResult<List<string>> ValidateNode(AgentContext ctx, string nodeId, ValidationOptions options);

    // Extended: Snapshots
    AgentResult<string> CreateSnapshot(AgentContext ctx, string rootNodeId, SnapshotOptions options, MutationOptions mutationOpts);
    AgentResult<List<string>> ListSnapshots(AgentContext ctx);
    AgentResult<string> GetSnapshot(AgentContext ctx, string snapshotId);
    AgentResult<string> RestoreSnapshot(AgentContext ctx, string snapshotId, RestoreOptions options, MutationOptions mutationOpts);
    AgentResult<string> DeleteSnapshot(AgentContext ctx, string snapshotId, MutationOptions mutationOpts);

    // Extended: Export/Import
    AgentResult<string> ExportTree(AgentContext ctx, string rootNodeId, ExportOptions options);
    AgentResult<string> ImportTree(AgentContext ctx, string jsonData, ImportOptions options, MutationOptions mutationOpts);
    AgentResult<string> ExportToFormat(AgentContext ctx, string rootNodeId, string format, ExportOptions options);
    AgentResult<Dictionary<string, object>> GetExportMetadata(AgentContext ctx, string rootNodeId);
}



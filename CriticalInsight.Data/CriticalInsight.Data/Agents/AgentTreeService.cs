using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using CriticalInsight.Data.Hierarchical;

namespace CriticalInsight.Data.Agents;

public sealed class AgentTreeService : IAgentTreeService
{
    private readonly ITreeNode _root;
    private readonly TreeView _treeView;
    private readonly Dictionary<string, Type> _payloadTypes;
    private readonly IAuditLogger _audit;
    private readonly IRateLimiter _rateLimiter;
    private readonly IIdempotencyStore _idempotency;
    private readonly IVersionProvider _versionProvider;

    private readonly TreeController<ITreeNode> _controller;

    public AgentTreeService(
        ITreeNode root,
        TreeView treeView,
        IDictionary<string, Type> payloadTypes,
        IAuditLogger? auditLogger = null,
        IRateLimiter? rateLimiter = null,
        IIdempotencyStore? idempotencyStore = null,
        IVersionProvider? versionProvider = null)
    {
        _root = root;
        _treeView = treeView;
        _payloadTypes = new Dictionary<string, Type>(payloadTypes);
        _audit = auditLogger ?? new NoopAuditLogger();
        _rateLimiter = rateLimiter ?? new TokenBucketRateLimiter(capacity: 60, refillPeriod: TimeSpan.FromMinutes(1));
        _idempotency = idempotencyStore ?? new InMemoryIdempotencyStore();
        _versionProvider = versionProvider ?? new InMemoryVersionProvider();
        _controller = new TreeController<ITreeNode>(_root, _treeView);
    }

    private AgentResult<T> Guard<T>(AgentContext ctx, string operation, Func<AgentResult<T>> action, bool isMutation = false, MutationOptions? opts = null)
    {
        if (!_rateLimiter.TryConsume(ctx.AgentId, out var retry))
        {
            _audit.Log(new AuditLogEntry
            {
                AgentId = ctx.AgentId,
                Operation = operation,
                Target = _root.NodeId,
                Success = false,
                ErrorCode = AgentErrorCode.rate_limited.ToString(),
                ErrorMessage = "Rate limit exceeded."
            });
            return AgentResult<T>.Fail(AgentErrorCode.rate_limited, "Rate limit exceeded.", retry?.ToString());
        }

        if (isMutation)
        {
            if (ctx.ReadOnly || !ctx.HasRole(AgentRole.Editor))
            {
                _audit.Log(new AuditLogEntry
                {
                    AgentId = ctx.AgentId,
                    Operation = operation,
                    Target = _root.NodeId,
                    Success = false,
                    ErrorCode = AgentErrorCode.forbidden.ToString(),
                    ErrorMessage = "Mutations are not permitted."
                });
                return AgentResult<T>.Fail(AgentErrorCode.forbidden, "Mutations are not permitted.");
            }

            var scopeId = _root.NodeId;
            if (!_versionProvider.TryCheckAndBump(scopeId, opts?.VersionToken, out var newVersion))
            {
                _audit.Log(new AuditLogEntry
                {
                    AgentId = ctx.AgentId,
                    Operation = operation,
                    Target = _root.NodeId,
                    Success = false,
                    ErrorCode = AgentErrorCode.conflict.ToString(),
                    ErrorMessage = "Version conflict."
                });
                return AgentResult<T>.Fail(AgentErrorCode.conflict, "Version conflict.");
            }

            if (!string.IsNullOrEmpty(opts?.IdempotencyKey))
            {
                if (_idempotency.TryGet(ctx.AgentId, opts!.IdempotencyKey!, out var cached) && cached is AgentResult<T> cachedRes)
                    return cachedRes;

                var result = WrapAudit(ctx, operation, action);
                if (result.Success)
                    _idempotency.Put(ctx.AgentId, opts!.IdempotencyKey!, result, TimeSpan.FromMinutes(10));
                return result;
            }

            return WrapAudit(ctx, operation, action);
        }

        return WrapAudit(ctx, operation, action);
    }

    private static string EncodePageToken(int startIndex)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(startIndex.ToString()));

    private static int DecodePageToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return 0;
        try
        {
            var s = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            return int.TryParse(s, out var idx) ? Math.Max(0, idx) : 0;
        }
        catch
        {
            return 0;
        }
    }

    private AgentResult<T> WrapAudit<T>(AgentContext ctx, string operation, Func<AgentResult<T>> action)
    {
        try
        {
            var res = action();
            _audit.Log(new AuditLogEntry
            {
                AgentId = ctx.AgentId,
                Operation = operation,
                Target = _root.NodeId,
                Success = res.Success,
                ErrorCode = res.Error != null ? res.Error.Code.ToString() : null,
                ErrorMessage = res.Error?.Message
            });
            return res;
        }
        catch (Exception ex)
        {
            _audit.Log(new AuditLogEntry
            {
                AgentId = ctx.AgentId,
                Operation = operation,
                Target = _root.NodeId,
                Success = false,
                ErrorCode = AgentErrorCode.internal_error.ToString(),
                ErrorMessage = ex.Message
            });
            return AgentResult<T>.Fail(AgentErrorCode.internal_error, ex.Message);
        }
    }

    private string SerializeNode(ITreeNode node, PropertyFilters filters)
    {
        var originalIncluded = _treeView.IncludedProperties.ToList();
        var originalExcluded = _treeView.ExcludedProperties.ToList();
        try
        {
            _treeView.IncludedProperties = filters.IncludedProperties.ToList();
            _treeView.ExcludedProperties = filters.ExcludedProperties.ToList();
            return TreeJsonSerializer.Serialize(node, _payloadTypes);
        }
        finally
        {
            _treeView.IncludedProperties = originalIncluded;
            _treeView.ExcludedProperties = originalExcluded;
        }
    }

    private string SerializeView(TreeView view, ViewOptions options)
    {
        var originalIncluded = view.IncludedProperties.ToList();
        var originalExcluded = view.ExcludedProperties.ToList();
        var originalDefault = view.DefaultExpanded;
        try
        {
            view.IncludedProperties = options.Filters.IncludedProperties.ToList();
            view.ExcludedProperties = options.Filters.ExcludedProperties.ToList();
            view.DefaultExpanded = options.DefaultExpanded;
            return TreeViewJsonSerializer.Serialize(view, _payloadTypes, options.IncludeViewRoot);
        }
        finally
        {
            view.IncludedProperties = originalIncluded;
            view.ExcludedProperties = originalExcluded;
            view.DefaultExpanded = originalDefault;
        }
    }

    private IEnumerable<ITreeNode> VisiblePreOrder(ITreeNode root)
    {
        var stack = new Stack<ITreeNode>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            if (_treeView.GetIsExpanded(node))
            {
                for (int i = node.Children.Count - 1; i >= 0; i--)
                    stack.Push(node.Children[i]);
            }
        }
    }

    private int RelativeDepth(ITreeNode node, ITreeNode root)
        => node.Depth - root.Depth;

    public AgentResult<string> GetNode(AgentContext ctx, string nodeId, PropertyFilters filters)
        => Guard(ctx, nameof(GetNode), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");
            return AgentResult<string>.Ok(SerializeNode(node, filters));
        });

    public AgentResult<string> GetView(AgentContext ctx, string rootNodeId, ViewOptions options)
        => Guard(ctx, nameof(GetView), () =>
        {
            var node = _controller.FindNode(rootNodeId);
            if (node == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Node {rootNodeId} not found.");
            
            // Apply depth limit by creating a filtered view
            var view = _treeView;
            if (options.DepthLimit.HasValue)
            {
                var filteredView = CreateDepthLimitedView(node, options.DepthLimit.Value);
                var filteredJson = SerializeView(filteredView, options);
                return AgentResult<string>.Ok(filteredJson);
            }
            
            var viewJson = SerializeView(view, options);
            return AgentResult<string>.Ok(viewJson);
        });

    public AgentResult<Page<string>> ListChildren(AgentContext ctx, string nodeId, PageOptions paging)
        => Guard(ctx, nameof(ListChildren), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<Page<string>>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            var start = DecodePageToken(paging.PageToken);
            var size = Math.Max(1, Math.Min(paging.PageSize ?? 50, 200));
            var children = node.Children.ToList();
            var pageItems = children.Skip(start).Take(size).Select(n => TreeJsonSerializer.Serialize(n, _payloadTypes)).ToList();
            var next = start + size < children.Count ? EncodePageToken(start + size) : null;
            return AgentResult<Page<string>>.Ok(new Page<string> { Items = pageItems, NextPageToken = next });
        });

    public AgentResult<Page<string>> Search(AgentContext ctx, string rootNodeId, IEnumerable<SearchFilter> filters, PageOptions paging)
        => Guard(ctx, nameof(Search), () =>
        {
            var node = _controller.FindNode(rootNodeId) ?? _root;
            var all = VisiblePreOrder(node).Skip(1); // exclude root
            bool Matches(ITreeNode n)
            {
                if (filters == null)
                    return true;
                foreach (var f in filters)
                {
                    // Minimal path support: match simple payload property names
                    var prop = n.PayloadObject.GetType().GetProperty(f.Path, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop == null)
                        return false;
                    var val = prop.GetValue(n.PayloadObject)?.ToString() ?? string.Empty;
                    if (string.Equals(f.Op, "contains", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!val.Contains(f.Value, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                    else
                    {
                        if (!string.Equals(val, f.Value, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                }
                return true;
            }

            var results = all.Where(Matches).ToList();
            var start = DecodePageToken(paging.PageToken);
            var size = Math.Max(1, Math.Min(paging.PageSize ?? 50, 200));
            var slice = results.Skip(start).Take(size).Select(n => TreeJsonSerializer.Serialize(n, _payloadTypes)).ToList();
            var next = start + size < results.Count ? EncodePageToken(start + size) : null;
            return AgentResult<Page<string>>.Ok(new Page<string> { Items = slice, NextPageToken = next });
        });

    public AgentResult<string> ExpandNode(AgentContext ctx, string nodeId, MutationOptions opts)
        => Guard(ctx, nameof(ExpandNode), () =>
        {
            _controller.ExpandNode(nodeId);
            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: opts);

    public AgentResult<string> CollapseNode(AgentContext ctx, string nodeId, MutationOptions opts)
        => Guard(ctx, nameof(CollapseNode), () =>
        {
            _controller.CollapseNode(nodeId);
            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: opts);

    public AgentResult<string> ToggleNode(AgentContext ctx, string nodeId, MutationOptions opts)
        => Guard(ctx, nameof(ToggleNode), () =>
        {
            _controller.ToggleNode(nodeId);
            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: opts);

    public AgentResult<string> ExpandAll(AgentContext ctx, string rootNodeId, MutationOptions opts)
        => Guard(ctx, nameof(ExpandAll), () =>
        {
            _controller.ExpandAll();
            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: opts);

    public AgentResult<string> CollapseAll(AgentContext ctx, string rootNodeId, MutationOptions opts)
        => Guard(ctx, nameof(CollapseAll), () =>
        {
            _controller.CollapseAll();
            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: opts);

    public AgentResult<string> AddChild(AgentContext ctx, string parentNodeId, string payloadTypeName, IDictionary<string, object?> payloadProps, MutationOptions opts)
        => Guard(ctx, nameof(AddChild), () =>
        {
            var parent = _controller.FindNode(parentNodeId);
            if (parent == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Parent {parentNodeId} not found.");

            if (!_payloadTypes.TryGetValue(payloadTypeName, out var pt))
                pt = Type.GetType(payloadTypeName) ?? throw new InvalidOperationException("Unknown payload type");

            ITreeNode newNode;
            if (typeof(ITreeNode).IsAssignableFrom(pt))
            {
                newNode = (Activator.CreateInstance(pt) as ITreeNode) ?? throw new InvalidOperationException("Cannot instantiate node type");
                ApplyProperties(newNode, payloadProps);
            }
            else
            {
                var nodeType = typeof(TreeNode<>).MakeGenericType(pt);
                newNode = (Activator.CreateInstance(nodeType) as ITreeNode) ?? throw new InvalidOperationException("Cannot instantiate node");
                ApplyPropertiesToPayload(newNode, payloadProps);
            }

            _controller.AddChild(parentNodeId, newNode);
            return AgentResult<string>.Ok(newNode.NodeId);
        }, isMutation: true, opts: opts);

    public AgentResult<string> UpdatePayloadProperty(AgentContext ctx, string nodeId, string propertyName, object? newValue, MutationOptions opts)
        => Guard(ctx, nameof(UpdatePayloadProperty), () =>
        {
            try
            {
                _controller.UpdateNodePayloadProperty(nodeId, propertyName, newValue!);
                return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
            }
            catch (InvalidOperationException ex)
            {
                return AgentResult<string>.Fail(AgentErrorCode.invalid_argument, ex.Message);
            }
        }, isMutation: true, opts: opts);

    public AgentResult<string> RemoveNode(AgentContext ctx, string nodeId, MutationOptions opts)
        => Guard(ctx, nameof(RemoveNode), () =>
        {
            _controller.RemoveNode(nodeId);
            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: opts);

    public AgentResult<string> MoveNode(AgentContext ctx, string nodeId, string newParentId, MutationOptions opts)
        => Guard(ctx, nameof(MoveNode), () =>
        {
            _controller.MoveNode(nodeId, newParentId);
            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: opts);

    public AgentResult<string> ExportView(AgentContext ctx, string rootNodeId, bool includeViewRoot)
        => Guard(ctx, nameof(ExportView), () =>
        {
            var json = _controller.ExportToJson(_payloadTypes, includeViewRoot, writeIndented: true);
            return AgentResult<string>.Ok(json);
        });

    public AgentResult<string> ImportView(AgentContext ctx, string viewJson, IDictionary<string, Type> payloadTypes, Func<string, ITreeNode?> nodeLookup, MutationOptions opts)
        => Guard(ctx, nameof(ImportView), () =>
        {
            _controller.ImportFromJson(viewJson, new Dictionary<string, Type>(payloadTypes), nodeLookup);
            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: opts);

    private static void ApplyProperties(ITreeNode node, IDictionary<string, object?> props)
    {
        foreach (var kv in props)
        {
            var pi = node.GetType().GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pi == null || !pi.CanWrite)
                continue;
            pi.SetValue(node, ConvertTo(pi.PropertyType, kv.Value));
        }
    }

    private static void ApplyPropertiesToPayload(ITreeNode node, IDictionary<string, object?> props)
    {
        var payload = node.PayloadObject;
        foreach (var kv in props)
        {
            var pi = payload.GetType().GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pi == null || !pi.CanWrite)
                continue;
            pi.SetValue(payload, ConvertTo(pi.PropertyType, kv.Value));
        }
        node.PayloadObject = payload;
    }

    private TreeView CreateDepthLimitedView(ITreeNode root, int maxDepth)
    {
        var limitedView = new TreeView(root, _treeView.DefaultExpanded);
        
        // Set expansion states based on depth
        void SetExpansionByDepth(ITreeNode node, int currentDepth)
        {
            if (currentDepth >= maxDepth)
            {
                limitedView.SetIsExpanded(node, false);
            }
            else
            {
                limitedView.SetIsExpanded(node, _treeView.GetIsExpanded(node));
            }
            
            foreach (var child in node.Children)
            {
                SetExpansionByDepth(child, currentDepth + 1);
            }
        }
        
        SetExpansionByDepth(root, 0);
        return limitedView;
    }

    private static object? ConvertTo(Type targetType, object? value)
    {
        if (value == null)
            return null;
        if (targetType.IsInstanceOfType(value))
            return value;
        try
        {
            if (value is JsonElement je)
            {
                return je.Deserialize(targetType, new JsonSerializerOptions());
            }
        }
        catch { }
        try
        {
            return System.Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType);
        }
        catch
        {
            return value;
        }
    }

    // Extended: Navigation & Retrieval

    public AgentResult<PathResult> GetPath(AgentContext ctx, string nodeId)
        => Guard(ctx, nameof(GetPath), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<PathResult>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            var path = new List<PathItem>();
            var current = node;
            while (current != null)
            {
                path.Insert(0, new PathItem
                {
                    NodeId = current.NodeId,
                    Name = GetNodeName(current),
                    Depth = current.Depth
                });
                current = current.Parent;
            }

            return AgentResult<PathResult>.Ok(new PathResult { Path = path });
        });

    public AgentResult<string> GetSubtree(AgentContext ctx, string nodeId, int? depthLimit, PropertyFilters filters, PageOptions paging)
        => Guard(ctx, nameof(GetSubtree), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            // Build a limited subtree if depth is specified
            ITreeNode? limitedSubtree = null;
            if (depthLimit.HasValue)
            {
                limitedSubtree = CreateDepthLimitedSubtree(node, depthLimit.Value);
            }

            var targetNode = limitedSubtree ?? node;

            // Apply filters
            var originalIncluded = _treeView.IncludedProperties.ToList();
            var originalExcluded = _treeView.ExcludedProperties.ToList();
            try
            {
                _treeView.IncludedProperties = filters.IncludedProperties.ToList();
                _treeView.ExcludedProperties = filters.ExcludedProperties.ToList();
                var json = TreeJsonSerializer.Serialize(targetNode, _payloadTypes);
                return AgentResult<string>.Ok(json);
            }
            finally
            {
                _treeView.IncludedProperties = originalIncluded;
                _treeView.ExcludedProperties = originalExcluded;
            }
        });

    public AgentResult<string> GetCommonAncestor(AgentContext ctx, IEnumerable<string> nodeIds)
        => Guard(ctx, nameof(GetCommonAncestor), () =>
        {
            var ids = nodeIds.ToList();
            if (ids.Count == 0)
                return AgentResult<string>.Fail(AgentErrorCode.invalid_argument, "At least one node ID required.");

            var nodes = ids.Select(id => _controller.FindNode(id)).Where(n => n != null).ToList();
            if (nodes.Count != ids.Count)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, "One or more nodes not found.");

            if (nodes.Count == 1)
                return AgentResult<string>.Ok(nodes[0]!.NodeId);

            // Find common ancestor by walking up from first node
            var first = nodes[0]!;
            foreach (var anc in first.Ancestors)
            {
                if (nodes.All(n => IsAncestorOf(anc, n)))
                    return AgentResult<string>.Ok(anc.NodeId);
            }
            return AgentResult<string>.Ok(first.Root.NodeId);
        });

    private static bool IsAncestorOf(ITreeNode ancestor, ITreeNode node)
    {
        var current = node;
        while (current != null)
        {
            if (current.NodeId == ancestor.NodeId)
                return true;
            current = current.Parent;
        }
        return false;
    }

    private string? GetNodeName(ITreeNode node)
    {
        try
        {
            var nameProp = node.PayloadObject?.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return nameProp?.GetValue(node.PayloadObject)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private ITreeNode CreateDepthLimitedSubtree(ITreeNode node, int maxDepth)
    {
        // Create a copy of the node with limited depth
        var nodeType = node.GetType();
        var constructor = nodeType.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
        
        if (constructor != null)
        {
            var cloned = constructor.Invoke(null) as ITreeNode;
            if (cloned != null)
            {
                // Copy payload
                cloned.PayloadObject = node.PayloadObject;
                
                if (maxDepth > 0)
                {
                    // Recursively copy children up to maxDepth
                    foreach (var child in node.Children)
                    {
                        var childCopy = CreateDepthLimitedSubtree(child, maxDepth - 1);
                        cloned.Children.Add(childCopy);
                        childCopy.SetParent(cloned);
                    }
                }
                
                return cloned;
            }
        }
        
        return node;
    }

    // Extended: Advanced Search & Selection

    public AgentResult<Page<string>> SearchAdvanced(AgentContext ctx, string rootNodeId, AdvancedSearchOptions options, PageOptions paging)
        => Guard(ctx, nameof(SearchAdvanced), () =>
        {
            var node = _controller.FindNode(rootNodeId) ?? _root;
            var all = VisiblePreOrder(node).Skip(1); // exclude root

            IEnumerable<ITreeNode> filtered;
            if (options.RootGroup != null)
            {
                filtered = all.Where(n => EvaluateSearchGroup(n, options.RootGroup));
            }
            else
            {
                filtered = all;
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(options.SortBy))
            {
                filtered = SortNodes(filtered, options.SortBy, options.SortDirection, options.Stable);
            }

            var results = filtered.ToList();
            var start = DecodePageToken(paging.PageToken);
            var size = Math.Max(1, Math.Min(paging.PageSize ?? 50, 200));
            var slice = results.Skip(start).Take(size).Select(n => TreeJsonSerializer.Serialize(n, _payloadTypes)).ToList();
            var next = start + size < results.Count ? EncodePageToken(start + size) : null;
            return AgentResult<Page<string>>.Ok(new Page<string> { Items = slice, NextPageToken = next });
        });

    public AgentResult<Page<string>> SelectNodes(AgentContext ctx, string rootNodeId, SelectQuery query, PageOptions paging)
        => Guard(ctx, nameof(SelectNodes), () =>
        {
            var node = _controller.FindNode(rootNodeId) ?? _root;
            var all = VisiblePreOrder(node).Skip(1); // exclude root

            var filtered = all.Where(n => EvaluateSelectExpression(n, query.Expression)).ToList();
            var start = DecodePageToken(paging.PageToken);
            var size = Math.Max(1, Math.Min(paging.PageSize ?? 50, 200));
            var slice = filtered.Skip(start).Take(size).Select(n => TreeJsonSerializer.Serialize(n, _payloadTypes)).ToList();
            var next = start + size < filtered.Count ? EncodePageToken(start + size) : null;
            return AgentResult<Page<string>>.Ok(new Page<string> { Items = slice, NextPageToken = next });
        });

    private bool EvaluateSearchGroup(ITreeNode node, SearchGroup group)
    {
        var predicateResults = group.Predicates.Select(p => EvaluatePredicate(node, p)).ToList();
        var groupResults = group.Groups.Select(g => EvaluateSearchGroup(node, g)).ToList();

        var allResults = predicateResults.Concat(groupResults).ToList();
        if (allResults.Count == 0) return true;

        var op = group.Op.ToLowerInvariant();
        
        // BUT-NOT-IF operator: positive criteria AND NOT exclusion criteria
        if (op == "but-not-if")
        {
            if (allResults.Count < 2)
            {
                // Need at least one positive and one exclusion criterion
                // If only one, treat as positive (always matches if positive matches)
                return allResults.Count == 1 ? allResults[0] : true;
            }

            // First result is positive, rest are exclusions
            var positive = allResults[0];
            var exclusions = allResults.Skip(1).ToList();
            
            // Positive must match AND no exclusion should match
            // If any exclusion matches, the node is excluded
            if (!positive) return false; // Positive criterion must match first
            if (exclusions.Any(e => e)) return false; // Any exclusion matching means exclusion
            return true; // Positive matches and no exclusions match
        }

        return op switch
        {
            "and" => allResults.All(r => r),
            "or" => allResults.Any(r => r),
            _ => allResults.All(r => r)
        };
    }

    private bool EvaluatePredicate(ITreeNode node, SearchPredicate predicate)
    {
        try
        {
            var prop = node.PayloadObject?.GetType().GetProperty(predicate.Path, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) return false;

            var val = prop.GetValue(node.PayloadObject);
            var valStr = val?.ToString() ?? string.Empty;

            return predicate.Op.ToLowerInvariant() switch
            {
                "eq" => string.Equals(valStr, predicate.Value, StringComparison.OrdinalIgnoreCase),
                "contains" => valStr.Contains(predicate.Value, StringComparison.OrdinalIgnoreCase),
                "gt" => TryCompare(val, predicate.Value) > 0,
                "lt" => TryCompare(val, predicate.Value) < 0,
                "between" => TryCompare(val, predicate.Value) >= 0 && TryCompare(val, predicate.Value2 ?? predicate.Value) <= 0,
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    private static int TryCompare(object? val, string? compareTo)
    {
        if (val == null || string.IsNullOrEmpty(compareTo)) return 0;
        
        if (val is IComparable comparable && double.TryParse(compareTo, out var num))
        {
            try
            {
                return comparable.CompareTo(Convert.ChangeType(num, val.GetType()));
            }
            catch
            {
                return string.Compare(val.ToString(), compareTo, StringComparison.OrdinalIgnoreCase);
            }
        }
        
        return string.Compare(val.ToString(), compareTo, StringComparison.OrdinalIgnoreCase);
    }

    private IEnumerable<ITreeNode> SortNodes(IEnumerable<ITreeNode> nodes, string sortBy, string direction, bool stable)
    {
        try
        {
            var sorted = nodes.OrderBy(n =>
            {
                var prop = n.PayloadObject?.GetType().GetProperty(sortBy, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                return prop?.GetValue(n.PayloadObject);
            });

            if (direction.ToLowerInvariant() == "desc")
                sorted = sorted.OrderByDescending(n =>
                {
                    var prop = n.PayloadObject?.GetType().GetProperty(sortBy, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    return prop?.GetValue(n.PayloadObject);
                });

            if (stable)
            {
                // Add secondary sort by NodeId for stability
                sorted = sorted.ThenBy(n => n.NodeId);
            }

            return sorted;
        }
        catch
        {
            return nodes; // Return unsorted on error
        }
    }

    private bool EvaluateSelectExpression(ITreeNode node, string expression)
    {
        // Simple DSL: "Name contains 'Kitchen' AND SquareFeet > 100"
        // For now, implement basic property matching
        try
        {
            var parts = expression.Split(new[] { " AND ", " OR " }, StringSplitOptions.None);
            var results = new List<bool>();

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Contains(" contains "))
                {
                    var propAndValue = trimmed.Split(new[] { " contains " }, 2, StringSplitOptions.None);
                    if (propAndValue.Length == 2)
                    {
                        var propName = propAndValue[0].Trim().Trim('"');
                        var value = propAndValue[1].Trim().Trim('"');
                        var prop = node.PayloadObject?.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        var propVal = prop?.GetValue(node.PayloadObject)?.ToString() ?? string.Empty;
                        results.Add(propVal.Contains(value, StringComparison.OrdinalIgnoreCase));
                    }
                }
                else if (trimmed.Contains(" > "))
                {
                    var propAndValue = trimmed.Split(new[] { " > " }, 2, StringSplitOptions.None);
                    if (propAndValue.Length == 2)
                    {
                        var propName = propAndValue[0].Trim().Trim('"');
                        var value = propAndValue[1].Trim().Trim('"');
                        var prop = node.PayloadObject?.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        var propVal = prop?.GetValue(node.PayloadObject);
                        results.Add(TryCompare(propVal, value) > 0);
                    }
                }
                else if (trimmed.Contains(" < "))
                {
                    var propAndValue = trimmed.Split(new[] { " < " }, 2, StringSplitOptions.None);
                    if (propAndValue.Length == 2)
                    {
                        var propName = propAndValue[0].Trim().Trim('"');
                        var value = propAndValue[1].Trim().Trim('"');
                        var prop = node.PayloadObject?.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        var propVal = prop?.GetValue(node.PayloadObject);
                        results.Add(TryCompare(propVal, value) < 0);
                    }
                }
            }

            // Simple AND logic for now
            return results.Count > 0 && results.All(r => r);
        }
        catch
        {
            return false;
        }
    }

    // Extended: Structural Editing

    public AgentResult<string> CopySubtree(AgentContext ctx, string sourceNodeId, string targetParentId, CopyOptions options, MutationOptions mutationOpts)
        => Guard(ctx, nameof(CopySubtree), () =>
        {
            var source = _controller.FindNode(sourceNodeId);
            if (source == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Source node {sourceNodeId} not found.");

            var target = _controller.FindNode(targetParentId);
            if (target == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Target parent {targetParentId} not found.");

            ITreeNode copied;
            if (options.Mode.ToLowerInvariant() == "reference")
            {
                // Reference mode: share the same payload object
                copied = source;
            }
            else
            {
                // Duplicate mode: create a deep copy
                copied = CreateDeepCopy(source);
            }

            _controller.AddChild(targetParentId, copied);
            return AgentResult<string>.Ok(copied.NodeId);
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<string> CloneNode(AgentContext ctx, string nodeId, string targetParentId, MutationOptions mutationOpts)
        => Guard(ctx, nameof(CloneNode), () =>
        {
            var source = _controller.FindNode(nodeId);
            if (source == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            var target = _controller.FindNode(targetParentId);
            if (target == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Target parent {targetParentId} not found.");

            var cloned = CreateDeepCopy(source);
            _controller.AddChild(targetParentId, cloned);
            return AgentResult<string>.Ok(cloned.NodeId);
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<string> MoveBefore(AgentContext ctx, string nodeId, string siblingId, MutationOptions mutationOpts)
        => Guard(ctx, nameof(MoveBefore), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            var sibling = _controller.FindNode(siblingId);
            if (sibling == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Sibling {siblingId} not found.");

            if (node.Parent != sibling.Parent)
                return AgentResult<string>.Fail(AgentErrorCode.invalid_argument, "Node and sibling must have the same parent.");

            var parent = node.Parent;
            if (parent == null)
                return AgentResult<string>.Fail(AgentErrorCode.invalid_argument, "Cannot move root node.");

            // Remove from current position
            parent.Children.Remove(node);
            
            // Find sibling index and insert before it
            var siblingIndex = parent.Children.IndexOf(sibling);
            parent.Children.Insert(siblingIndex, node);

            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<string> MoveAfter(AgentContext ctx, string nodeId, string siblingId, MutationOptions mutationOpts)
        => Guard(ctx, nameof(MoveAfter), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            var sibling = _controller.FindNode(siblingId);
            if (sibling == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Sibling {siblingId} not found.");

            if (node.Parent != sibling.Parent)
                return AgentResult<string>.Fail(AgentErrorCode.invalid_argument, "Node and sibling must have the same parent.");

            var parent = node.Parent;
            if (parent == null)
                return AgentResult<string>.Fail(AgentErrorCode.invalid_argument, "Cannot move root node.");

            // Remove from current position
            parent.Children.Remove(node);
            
            // Find sibling index and insert after it
            var siblingIndex = parent.Children.IndexOf(sibling);
            parent.Children.Insert(siblingIndex + 1, node);

            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<string> SortChildren(AgentContext ctx, string parentId, SortOptions options, MutationOptions mutationOpts)
        => Guard(ctx, nameof(SortChildren), () =>
        {
            var parent = _controller.FindNode(parentId);
            if (parent == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Parent {parentId} not found.");

            var children = parent.Children.ToList();
            
            try
            {
                // Sort children by the specified property
                IOrderedEnumerable<ITreeNode> sorted;
                if (options.Direction.ToLowerInvariant() == "desc")
                {
                    sorted = children.OrderByDescending(child =>
                    {
                        var prop = child.PayloadObject?.GetType().GetProperty(options.ByProperty, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        return prop?.GetValue(child.PayloadObject);
                    });
                }
                else
                {
                    sorted = children.OrderBy(child =>
                    {
                        var prop = child.PayloadObject?.GetType().GetProperty(options.ByProperty, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        return prop?.GetValue(child.PayloadObject);
                    });
                }

                if (options.Stable)
                {
                    sorted = sorted.ThenBy(child => child.NodeId);
                }

                var sortedList = sorted.ToList();
                
                // Clear and re-add in sorted order
                parent.Children.Clear();
                foreach (var child in sortedList)
                {
                    parent.Children.Add(child);
                }

                return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
            }
            catch (Exception ex)
            {
                return AgentResult<string>.Fail(AgentErrorCode.internal_error, $"Sorting failed: {ex.Message}");
            }
        }, isMutation: true, opts: mutationOpts);

    private ITreeNode CreateDeepCopy(ITreeNode source)
    {
        var nodeType = source.GetType();
        var constructor = nodeType.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
        
        if (constructor != null)
        {
            var cloned = constructor.Invoke(null) as ITreeNode;
            if (cloned != null)
            {
                // Deep copy payload
                cloned.PayloadObject = DeepCopyPayload(source.PayloadObject);
                
                // Recursively copy children
                foreach (var child in source.Children)
                {
                    var childCopy = CreateDeepCopy(child);
                    cloned.Children.Add(childCopy);
                    childCopy.SetParent(cloned);
                }
                
                return cloned;
            }
        }
        
        return source;
    }

    private object? DeepCopyPayload(object? payload)
    {
        if (payload == null) return null;
        
        try
        {
            // Simple deep copy using JSON serialization for now
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            return System.Text.Json.JsonSerializer.Deserialize(json, payload.GetType());
        }
        catch
        {
            // Fallback: return original if deep copy fails
            return payload;
        }
    }

    // Extended: Bulk Update

    public AgentResult<Dictionary<string, string>> UpdatePayload(AgentContext ctx, string nodeId, Dictionary<string, object?> properties, MutationOptions mutationOpts)
        => Guard(ctx, nameof(UpdatePayload), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<Dictionary<string, string>>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            var results = new Dictionary<string, string>();
            var payloadType = node.PayloadObject?.GetType();
            
            if (payloadType == null)
                return AgentResult<Dictionary<string, string>>.Fail(AgentErrorCode.invalid_argument, "Node has no payload object.");

            foreach (var kvp in properties)
            {
                try
                {
                    var prop = payloadType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop == null)
                    {
                        results[kvp.Key] = "Property not found";
                        continue;
                    }

                    if (!prop.CanWrite)
                    {
                        results[kvp.Key] = "Property is read-only";
                        continue;
                    }

                    // Check for internal property guards
                    if (IsInternalProperty(kvp.Key))
                    {
                        results[kvp.Key] = "Cannot modify internal property";
                        continue;
                    }

                    var convertedValue = ConvertTo(prop.PropertyType, kvp.Value);
                    prop.SetValue(node.PayloadObject, convertedValue);
                    results[kvp.Key] = "Updated";
                }
                catch (Exception ex)
                {
                    results[kvp.Key] = $"Error: {ex.Message}";
                }
            }

            return AgentResult<Dictionary<string, string>>.Ok(results);
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<Dictionary<string, string>> UpdateNodes(AgentContext ctx, IEnumerable<BulkUpdateItem> updates, BulkUpdateOptions options, MutationOptions mutationOpts)
        => Guard(ctx, nameof(UpdateNodes), () =>
        {
            var results = new Dictionary<string, string>();
            var updateList = updates.ToList();

            // Validate all nodes exist first if requested
            if (options.ValidateBeforeUpdate)
            {
                foreach (var update in updateList)
                {
                    var node = _controller.FindNode(update.NodeId);
                    if (node == null)
                    {
                        results[update.NodeId] = "Node not found";
                        if (!options.ContinueOnError)
                            return AgentResult<Dictionary<string, string>>.Fail(AgentErrorCode.not_found, $"Node {update.NodeId} not found.");
                    }
                }
            }

            // Process updates
            foreach (var update in updateList)
            {
                try
                {
                    var node = _controller.FindNode(update.NodeId);
                    if (node == null)
                    {
                        results[update.NodeId] = "Node not found";
                        continue;
                    }

                    var payloadType = node.PayloadObject?.GetType();
                    if (payloadType == null)
                    {
                        results[update.NodeId] = "No payload object";
                        continue;
                    }

                    var nodeResults = new Dictionary<string, string>();
                    foreach (var kvp in update.Properties)
                    {
                        try
                        {
                            var prop = payloadType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                            if (prop == null)
                            {
                                nodeResults[kvp.Key] = "Property not found";
                                continue;
                            }

                            if (!prop.CanWrite)
                            {
                                nodeResults[kvp.Key] = "Property is read-only";
                                continue;
                            }

                            if (IsInternalProperty(kvp.Key))
                            {
                                nodeResults[kvp.Key] = "Cannot modify internal property";
                                continue;
                            }

                            var convertedValue = ConvertTo(prop.PropertyType, kvp.Value);
                            prop.SetValue(node.PayloadObject, convertedValue);
                            nodeResults[kvp.Key] = "Updated";
                        }
                        catch (Exception ex)
                        {
                            nodeResults[kvp.Key] = $"Error: {ex.Message}";
                        }
                    }

                    results[update.NodeId] = string.Join("; ", nodeResults.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                }
                catch (Exception ex)
                {
                    results[update.NodeId] = $"Node error: {ex.Message}";
                    if (!options.ContinueOnError)
                        return AgentResult<Dictionary<string, string>>.Fail(AgentErrorCode.internal_error, $"Bulk update failed on {update.NodeId}: {ex.Message}");
                }
            }

            return AgentResult<Dictionary<string, string>>.Ok(results);
        }, isMutation: true, opts: mutationOpts);

    private bool IsInternalProperty(string propertyName)
    {
        var internalProps = new[] { "NodeId", "Children", "Parent", "PayloadType" };
        return internalProps.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
    }

    // Extended: View Control

    public AgentResult<string> SetExpansionRecursive(AgentContext ctx, string rootNodeId, ExpansionOptions options, MutationOptions mutationOpts)
        => Guard(ctx, nameof(SetExpansionRecursive), () =>
        {
            var root = _controller.FindNode(rootNodeId) ?? _root;
            
            SetExpansionRecursiveInternal(root, options.Expanded, options.MaxDepth, options.IncludeRoot);
            
            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<string> SetFilters(AgentContext ctx, ViewFilterOptions options, MutationOptions mutationOpts)
        => Guard(ctx, nameof(SetFilters), () =>
        {
            if (options.ReplaceExisting)
            {
                _treeView.IncludedProperties.Clear();
                _treeView.ExcludedProperties.Clear();
            }
            
            foreach (var prop in options.IncludedProperties)
            {
                if (!_treeView.IncludedProperties.Contains(prop))
                    _treeView.IncludedProperties.Add(prop);
            }
            
            foreach (var prop in options.ExcludedProperties)
            {
                if (!_treeView.ExcludedProperties.Contains(prop))
                    _treeView.ExcludedProperties.Add(prop);
            }
            
            return AgentResult<string>.Ok(_versionProvider.GetVersion(_root.NodeId));
        }, isMutation: true, opts: mutationOpts);

    private void SetExpansionRecursiveInternal(ITreeNode node, bool expanded, int? maxDepth, bool includeRoot, int currentDepth = 0)
    {
        if (includeRoot || currentDepth > 0)
        {
            // Only set expansion if we're within the max depth limit
            if (!maxDepth.HasValue || currentDepth < maxDepth.Value)
            {
                _treeView.SetIsExpanded(node, expanded);
            }
            else
            {
                // Beyond max depth - always collapse
                _treeView.SetIsExpanded(node, false);
            }
        }
        
        foreach (var child in node.Children)
        {
            SetExpansionRecursiveInternal(child, expanded, maxDepth, includeRoot, currentDepth + 1);
        }
    }

    private readonly Dictionary<string, HashSet<string>> _nodeTags = new();
    private readonly Dictionary<string, BookmarkInfo> _bookmarks = new();

    // Extended: Tagging & Bookmarks

    public AgentResult<List<string>> AddTags(AgentContext ctx, string nodeId, TagOptions options, MutationOptions mutationOpts)
        => Guard(ctx, nameof(AddTags), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<List<string>>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            if (!_nodeTags.ContainsKey(nodeId))
                _nodeTags[nodeId] = new HashSet<string>();

            var existingTags = _nodeTags[nodeId].ToList();
            
            if (options.ReplaceExisting)
            {
                _nodeTags[nodeId].Clear();
            }
            
            foreach (var tag in options.Tags)
            {
                _nodeTags[nodeId].Add(tag);
            }
            
            return AgentResult<List<string>>.Ok(_nodeTags[nodeId].ToList());
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<List<string>> RemoveTags(AgentContext ctx, string nodeId, List<string> tags, MutationOptions mutationOpts)
        => Guard(ctx, nameof(RemoveTags), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<List<string>>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            if (!_nodeTags.ContainsKey(nodeId))
                return AgentResult<List<string>>.Ok(new List<string>());

            foreach (var tag in tags)
            {
                _nodeTags[nodeId].Remove(tag);
            }
            
            return AgentResult<List<string>>.Ok(_nodeTags[nodeId].ToList());
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<List<string>> GetTags(AgentContext ctx, string nodeId)
        => Guard(ctx, nameof(GetTags), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<List<string>>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            var tags = _nodeTags.ContainsKey(nodeId) ? _nodeTags[nodeId].ToList() : new List<string>();
            return AgentResult<List<string>>.Ok(tags);
        });

    public AgentResult<Page<string>> FindNodesByTag(AgentContext ctx, string rootNodeId, string tag, PageOptions paging)
        => Guard(ctx, nameof(FindNodesByTag), () =>
        {
            var root = _controller.FindNode(rootNodeId) ?? _root;
            var allNodes = VisiblePreOrder(root).Skip(1); // exclude root
            
            var matchingNodes = allNodes.Where(n => _nodeTags.ContainsKey(n.NodeId) && _nodeTags[n.NodeId].Contains(tag)).ToList();
            
            var start = DecodePageToken(paging.PageToken);
            var size = Math.Max(1, Math.Min(paging.PageSize ?? 50, 200));
            var slice = matchingNodes.Skip(start).Take(size).Select(n => n.NodeId).ToList();
            var next = start + size < matchingNodes.Count ? EncodePageToken(start + size) : null;
            
            return AgentResult<Page<string>>.Ok(new Page<string> { Items = slice, NextPageToken = next });
        });

    public AgentResult<string> CreateBookmark(AgentContext ctx, string nodeId, BookmarkOptions options, MutationOptions mutationOpts)
        => Guard(ctx, nameof(CreateBookmark), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            var bookmarkId = Guid.NewGuid().ToString();
            var bookmark = new BookmarkInfo
            {
                Id = bookmarkId,
                NodeId = nodeId,
                Name = options.Name,
                Description = options.Description,
                Metadata = options.Metadata,
                CreatedBy = ctx.AgentId,
                CreatedAt = DateTime.UtcNow
            };
            
            _bookmarks[bookmarkId] = bookmark;
            
            return AgentResult<string>.Ok(bookmarkId);
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<List<string>> ListBookmarks(AgentContext ctx)
        => Guard(ctx, nameof(ListBookmarks), () =>
        {
            var bookmarkIds = _bookmarks.Values
                .Where(b => b.CreatedBy == ctx.AgentId)
                .OrderBy(b => b.CreatedAt)
                .Select(b => b.Id)
                .ToList();
                
            return AgentResult<List<string>>.Ok(bookmarkIds);
        });

    public AgentResult<string> DeleteBookmark(AgentContext ctx, string bookmarkId, MutationOptions mutationOpts)
        => Guard(ctx, nameof(DeleteBookmark), () =>
        {
            if (!_bookmarks.ContainsKey(bookmarkId))
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Bookmark {bookmarkId} not found.");

            var bookmark = _bookmarks[bookmarkId];
            if (bookmark.CreatedBy != ctx.AgentId)
                return AgentResult<string>.Fail(AgentErrorCode.forbidden, "Cannot delete bookmark created by another agent.");

            _bookmarks.Remove(bookmarkId);
            
            return AgentResult<string>.Ok(bookmarkId);
        }, isMutation: true, opts: mutationOpts);

    // Extended: Validation & Diff

    public AgentResult<List<string>> ValidateTree(AgentContext ctx, string rootNodeId, ValidationOptions options)
        => Guard(ctx, nameof(ValidateTree), () =>
        {
            var root = _controller.FindNode(rootNodeId) ?? _root;
            var issues = new List<string>();
            
            ValidateNodeRecursive(root, options, issues, 0);
            
            return AgentResult<List<string>>.Ok(issues);
        });

    public AgentResult<string> DiffTrees(AgentContext ctx, string rootNodeId1, string rootNodeId2, DiffOptions options)
        => Guard(ctx, nameof(DiffTrees), () =>
        {
            var root1 = _controller.FindNode(rootNodeId1) ?? _root;
            var root2 = _controller.FindNode(rootNodeId2) ?? _root;
            
            var diff = new Dictionary<string, object>();
            
            if (options.IncludeStructure)
            {
                diff["structure"] = DiffStructure(root1, root2, options.MaxDepth);
            }
            
            if (options.IncludePayloads)
            {
                diff["payloads"] = DiffPayloads(root1, root2, options.MaxDepth);
            }
            
            if (options.IncludeMetadata)
            {
                diff["metadata"] = DiffMetadata(root1, root2);
            }
            
            var json = System.Text.Json.JsonSerializer.Serialize(diff, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return AgentResult<string>.Ok(json);
        });

    public AgentResult<List<string>> ValidateNode(AgentContext ctx, string nodeId, ValidationOptions options)
        => Guard(ctx, nameof(ValidateNode), () =>
        {
            var node = _controller.FindNode(nodeId);
            if (node == null)
                return AgentResult<List<string>>.Fail(AgentErrorCode.not_found, $"Node {nodeId} not found.");

            var issues = new List<string>();
            ValidateNodeRecursive(node, options, issues, 0);
            
            return AgentResult<List<string>>.Ok(issues);
        });

    private void ValidateNodeRecursive(ITreeNode node, ValidationOptions options, List<string> issues, int depth)
    {
        if (options.CheckStructure)
        {
            ValidateStructure(node, issues);
        }
        
        if (options.CheckPayloads)
        {
            ValidatePayload(node, options, issues);
        }
        
        if (options.CheckReferences)
        {
            ValidateReferences(node, issues);
        }
        
        foreach (var child in node.Children)
        {
            ValidateNodeRecursive(child, options, issues, depth + 1);
        }
    }

    private void ValidateStructure(ITreeNode node, List<string> issues)
    {
        if (string.IsNullOrEmpty(node.NodeId))
            issues.Add($"Node at depth {node.Depth} has empty NodeId");
            
        if (node.PayloadObject == null)
            issues.Add($"Node {node.NodeId} has null payload");
            
        if (string.IsNullOrEmpty(node.PayloadType))
            issues.Add($"Node {node.NodeId} has empty PayloadType");
    }

    private void ValidatePayload(ITreeNode node, ValidationOptions options, List<string> issues)
    {
        if (node.PayloadObject == null) return;
        
        var payloadType = node.PayloadObject.GetType();
        
        foreach (var requiredProp in options.RequiredProperties)
        {
            var prop = payloadType.GetProperty(requiredProp, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null)
            {
                issues.Add($"Node {node.NodeId} missing required property '{requiredProp}'");
                continue;
            }
            
            var value = prop.GetValue(node.PayloadObject);
            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
            {
                issues.Add($"Node {node.NodeId} property '{requiredProp}' is null or empty");
            }
        }
    }

    private void ValidateReferences(ITreeNode node, List<string> issues)
    {
        // Check parent-child consistency
        foreach (var child in node.Children)
        {
            if (child.Parent != node)
            {
                issues.Add($"Node {child.NodeId} parent reference inconsistent");
            }
        }
        
        // Check depth consistency
        var expectedDepth = node.Parent?.Depth + 1 ?? 0;
        if (node.Depth != expectedDepth)
        {
            issues.Add($"Node {node.NodeId} depth {node.Depth} inconsistent with parent depth");
        }
    }

    private Dictionary<string, object> DiffStructure(ITreeNode node1, ITreeNode node2, int? maxDepth)
    {
        var diff = new Dictionary<string, object>();
        
        if (node1.Children.Count != node2.Children.Count)
        {
            diff["children_count"] = new { node1 = node1.Children.Count, node2 = node2.Children.Count };
        }
        
        if (maxDepth.HasValue && maxDepth.Value <= 0) return diff;
        
        var childDiffs = new List<object>();
        var maxChildren = Math.Max(node1.Children.Count, node2.Children.Count);
        
        for (int i = 0; i < maxChildren; i++)
        {
            var child1 = i < node1.Children.Count ? node1.Children[i] : null;
            var child2 = i < node2.Children.Count ? node2.Children[i] : null;
            
            if (child1 == null || child2 == null)
            {
                childDiffs.Add(new { index = i, missing = child1 == null ? "node1" : "node2" });
            }
            else if (child1.NodeId != child2.NodeId)
            {
                childDiffs.Add(new { index = i, nodeId_diff = new { node1 = child1.NodeId, node2 = child2.NodeId } });
            }
            else if (maxDepth == null || maxDepth.Value > 1)
            {
                var childDiff = DiffStructure(child1, child2, maxDepth.HasValue ? maxDepth.Value - 1 : null);
                if (childDiff.Any())
                {
                    childDiffs.Add(new { index = i, nodeId = child1.NodeId, diff = childDiff });
                }
            }
        }
        
        if (childDiffs.Any())
        {
            diff["children"] = childDiffs;
        }
        
        return diff;
    }

    private Dictionary<string, object> DiffPayloads(ITreeNode node1, ITreeNode node2, int? maxDepth)
    {
        var diff = new Dictionary<string, object>();
        
        if (node1.PayloadObject == null && node2.PayloadObject == null) return diff;
        if (node1.PayloadObject == null || node2.PayloadObject == null)
        {
            diff["payload"] = new { node1 = node1.PayloadObject?.GetType().Name, node2 = node2.PayloadObject?.GetType().Name };
            return diff;
        }
        
        var type1 = node1.PayloadObject.GetType();
        var type2 = node2.PayloadObject.GetType();
        
        if (type1 != type2)
        {
            diff["payload_type"] = new { node1 = type1.Name, node2 = type2.Name };
            return diff;
        }
        
        var payloadDiffs = new Dictionary<string, object>();
        var properties = type1.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var prop in properties)
        {
            if (!prop.CanRead) continue;
            
            var value1 = prop.GetValue(node1.PayloadObject);
            var value2 = prop.GetValue(node2.PayloadObject);
            
            if (!Equals(value1, value2))
            {
                payloadDiffs[prop.Name] = new { node1 = value1, node2 = value2 };
            }
        }
        
        if (payloadDiffs.Any())
        {
            diff["payload_properties"] = payloadDiffs;
        }
        
        return diff;
    }

    private Dictionary<string, object> DiffMetadata(ITreeNode node1, ITreeNode node2)
    {
        var diff = new Dictionary<string, object>();
        
        var tags1 = _nodeTags.ContainsKey(node1.NodeId) ? _nodeTags[node1.NodeId].ToList() : new List<string>();
        var tags2 = _nodeTags.ContainsKey(node2.NodeId) ? _nodeTags[node2.NodeId].ToList() : new List<string>();
        
        if (!tags1.SequenceEqual(tags2))
        {
            diff["tags"] = new { node1 = tags1, node2 = tags2 };
        }
        
        return diff;
    }

    // Extended: Snapshots

    private readonly Dictionary<string, SnapshotInfo> _snapshots = new();

    public AgentResult<string> CreateSnapshot(AgentContext ctx, string rootNodeId, SnapshotOptions options, MutationOptions mutationOpts)
        => Guard(ctx, nameof(CreateSnapshot), () =>
        {
            var root = _controller.FindNode(rootNodeId) ?? _root;
            var snapshotId = Guid.NewGuid().ToString();
            
            var snapshot = new SnapshotInfo
            {
                Id = snapshotId,
                Name = options.Name,
                Description = options.Description,
                Metadata = options.Metadata,
                RootNodeId = rootNodeId,
                CreatedBy = ctx.AgentId,
                CreatedAt = DateTime.UtcNow,
                TreeData = SerializeTreeForSnapshot(root),
                ViewState = options.IncludeViewState ? SerializeViewState() : null,
                Tags = options.IncludeTags ? SerializeTags() : null,
                Bookmarks = options.IncludeBookmarks ? SerializeBookmarks(ctx.AgentId) : null
            };
            
            _snapshots[snapshotId] = snapshot;
            
            return AgentResult<string>.Ok(snapshotId);
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<List<string>> ListSnapshots(AgentContext ctx)
        => Guard(ctx, nameof(ListSnapshots), () =>
        {
            var snapshotIds = _snapshots.Values
                .Where(s => s.CreatedBy == ctx.AgentId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => s.Id)
                .ToList();
                
            return AgentResult<List<string>>.Ok(snapshotIds);
        });

    public AgentResult<string> GetSnapshot(AgentContext ctx, string snapshotId)
        => Guard(ctx, nameof(GetSnapshot), () =>
        {
            if (!_snapshots.ContainsKey(snapshotId))
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Snapshot {snapshotId} not found.");

            var snapshot = _snapshots[snapshotId];
            if (snapshot.CreatedBy != ctx.AgentId)
                return AgentResult<string>.Fail(AgentErrorCode.forbidden, "Cannot access snapshot created by another agent.");

            var snapshotJson = System.Text.Json.JsonSerializer.Serialize(snapshot, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return AgentResult<string>.Ok(snapshotJson);
        });

    public AgentResult<string> RestoreSnapshot(AgentContext ctx, string snapshotId, RestoreOptions options, MutationOptions mutationOpts)
        => Guard(ctx, nameof(RestoreSnapshot), () =>
        {
            if (!_snapshots.ContainsKey(snapshotId))
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Snapshot {snapshotId} not found.");

            var snapshot = _snapshots[snapshotId];
            if (snapshot.CreatedBy != ctx.AgentId)
                return AgentResult<string>.Fail(AgentErrorCode.forbidden, "Cannot restore snapshot created by another agent.");

            if (options.ValidateBeforeRestore)
            {
                var validationOptions = new ValidationOptions { CheckStructure = true, CheckPayloads = true, CheckReferences = true };
                var validationIssues = new List<string>();
                ValidateSnapshotData(snapshot.TreeData, validationOptions, validationIssues);
                
                if (validationIssues.Any())
                {
                    return AgentResult<string>.Fail(AgentErrorCode.validation_failed, $"Snapshot validation failed: {string.Join(", ", validationIssues)}");
                }
            }

            // Restore tree structure
            var restoredRoot = DeserializeTreeFromSnapshot(snapshot.TreeData);
            if (restoredRoot == null)
                return AgentResult<string>.Fail(AgentErrorCode.deserialization_failed, "Failed to deserialize snapshot tree data.");

            // Note: Since _root, _controller, and _treeView are readonly fields,
            // we cannot reassign them. This is a limitation of the current design.
            // In a real implementation, you would need to recreate the service instance
            // or use a different approach for restoration.
            // For now, we'll return an error indicating this limitation.
            return AgentResult<string>.Fail(AgentErrorCode.internal_error, "Snapshot restoration requires service recreation due to readonly field constraints.");
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<string> DeleteSnapshot(AgentContext ctx, string snapshotId, MutationOptions mutationOpts)
        => Guard(ctx, nameof(DeleteSnapshot), () =>
        {
            if (!_snapshots.ContainsKey(snapshotId))
                return AgentResult<string>.Fail(AgentErrorCode.not_found, $"Snapshot {snapshotId} not found.");

            var snapshot = _snapshots[snapshotId];
            if (snapshot.CreatedBy != ctx.AgentId)
                return AgentResult<string>.Fail(AgentErrorCode.forbidden, "Cannot delete snapshot created by another agent.");

            _snapshots.Remove(snapshotId);
            
            return AgentResult<string>.Ok(snapshotId);
        }, isMutation: true, opts: mutationOpts);

    private string SerializeTreeForSnapshot(ITreeNode root)
    {
        return TreeJsonSerializer.Serialize(root, _payloadTypes);
    }

    private ITreeNode? DeserializeTreeFromSnapshot(string treeData)
    {
        try
        {
            return TreeJsonSerializer.Deserialize<ITreeNode>(treeData, _payloadTypes);
        }
        catch
        {
            return null;
        }
    }

    private string SerializeViewState()
    {
        var viewState = new Dictionary<string, object>();
        
        // Serialize expansion states
        var expansionStates = new Dictionary<string, bool>();
        foreach (var node in VisiblePreOrder(_root))
        {
            expansionStates[node.NodeId] = _treeView.GetIsExpanded(node);
        }
        viewState["expansionStates"] = expansionStates;
        
        // Serialize filters
        viewState["includedProperties"] = _treeView.IncludedProperties.ToList();
        viewState["excludedProperties"] = _treeView.ExcludedProperties.ToList();
        
        return System.Text.Json.JsonSerializer.Serialize(viewState);
    }

    private void RestoreViewState(string viewStateJson)
    {
        try
        {
            var viewState = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(viewStateJson);
            if (viewState == null) return;

            // Restore expansion states
            if (viewState.ContainsKey("expansionStates"))
            {
                var expansionStatesJson = viewState["expansionStates"].ToString();
                var expansionStates = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(expansionStatesJson!);
                if (expansionStates != null)
                {
                    foreach (var kvp in expansionStates)
                    {
                        var node = _controller.FindNode(kvp.Key);
                        if (node != null)
                        {
                            _treeView.SetIsExpanded(node, kvp.Value);
                        }
                    }
                }
            }

            // Restore filters
            if (viewState.ContainsKey("includedProperties"))
            {
                var includedJson = viewState["includedProperties"].ToString();
                var included = System.Text.Json.JsonSerializer.Deserialize<List<string>>(includedJson!);
                if (included != null)
                {
                    _treeView.IncludedProperties.Clear();
                    foreach (var prop in included)
                    {
                        _treeView.IncludedProperties.Add(prop);
                    }
                }
            }

            if (viewState.ContainsKey("excludedProperties"))
            {
                var excludedJson = viewState["excludedProperties"].ToString();
                var excluded = System.Text.Json.JsonSerializer.Deserialize<List<string>>(excludedJson!);
                if (excluded != null)
                {
                    _treeView.ExcludedProperties.Clear();
                    foreach (var prop in excluded)
                    {
                        _treeView.ExcludedProperties.Add(prop);
                    }
                }
            }
        }
        catch
        {
            // Ignore view state restoration errors
        }
    }

    private string SerializeTags()
    {
        return System.Text.Json.JsonSerializer.Serialize(_nodeTags);
    }

    private void RestoreTags(string tagsJson)
    {
        try
        {
            var tags = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, HashSet<string>>>(tagsJson);
            if (tags != null)
            {
                _nodeTags.Clear();
                foreach (var kvp in tags)
                {
                    _nodeTags[kvp.Key] = new HashSet<string>(kvp.Value);
                }
            }
        }
        catch
        {
            // Ignore tag restoration errors
        }
    }

    private string SerializeBookmarks(string agentId)
    {
        var agentBookmarks = _bookmarks.Values
            .Where(b => b.CreatedBy == agentId)
            .ToDictionary(b => b.Id, b => b);
        return System.Text.Json.JsonSerializer.Serialize(agentBookmarks);
    }

    private void RestoreBookmarks(string bookmarksJson, string agentId)
    {
        try
        {
            var bookmarks = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, BookmarkInfo>>(bookmarksJson);
            if (bookmarks != null)
            {
                // Remove existing bookmarks for this agent
                var existingBookmarks = _bookmarks.Where(kvp => kvp.Value.CreatedBy == agentId).ToList();
                foreach (var kvp in existingBookmarks)
                {
                    _bookmarks.Remove(kvp.Key);
                }

                // Add restored bookmarks
                foreach (var kvp in bookmarks)
                {
                    _bookmarks[kvp.Key] = kvp.Value;
                }
            }
        }
        catch
        {
            // Ignore bookmark restoration errors
        }
    }

    private void ValidateSnapshotData(string treeData, ValidationOptions options, List<string> issues)
    {
        try
        {
            var root = DeserializeTreeFromSnapshot(treeData);
            if (root == null)
            {
                issues.Add("Failed to deserialize tree data");
                return;
            }

            ValidateNodeRecursive(root, options, issues, 0);
        }
        catch (Exception ex)
        {
            issues.Add($"Snapshot validation error: {ex.Message}");
        }
    }

    // Extended: Export/Import

    public AgentResult<string> ExportTree(AgentContext ctx, string rootNodeId, ExportOptions options)
        => Guard(ctx, nameof(ExportTree), () =>
        {
            var root = _controller.FindNode(rootNodeId) ?? _root;
            
            var exportData = new Dictionary<string, object>
            {
                ["version"] = "1.0",
                ["exportedAt"] = DateTime.UtcNow,
                ["exportedBy"] = ctx.AgentId,
                ["rootNodeId"] = rootNodeId
            };

            // Export tree structure
            exportData["tree"] = SerializeTreeForSnapshot(root);

            // Export view state if requested
            if (options.IncludeViewState)
            {
                exportData["viewState"] = SerializeViewState();
            }

            // Export tags if requested
            if (options.IncludeTags)
            {
                exportData["tags"] = SerializeTags();
            }

            // Export bookmarks if requested
            if (options.IncludeBookmarks)
            {
                exportData["bookmarks"] = SerializeBookmarks(ctx.AgentId);
            }

            // Export metadata if requested
            if (options.IncludeMetadata)
            {
                exportData["metadata"] = GetExportMetadataInternal(root);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            
            if (options.Compress)
            {
                // Simple compression simulation - in real implementation, use actual compression
                json = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
            }

            return AgentResult<string>.Ok(json);
        });

    public AgentResult<string> ImportTree(AgentContext ctx, string jsonData, ImportOptions options, MutationOptions mutationOpts)
        => Guard(ctx, nameof(ImportTree), () =>
        {
            string actualJson = jsonData;
            
            // Handle compressed data - check if it's base64 encoded
            if (IsBase64String(jsonData))
            {
                try
                {
                    actualJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jsonData));
                }
                catch
                {
                    return AgentResult<string>.Fail(AgentErrorCode.invalid_argument, "Invalid compressed data format.");
                }
            }

            Dictionary<string, object>? importData;
            try
            {
                importData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(actualJson);
            }
            catch (Exception ex)
            {
                return AgentResult<string>.Fail(AgentErrorCode.deserialization_failed, $"Failed to deserialize import data: {ex.Message}");
            }
            
            if (importData == null)
                return AgentResult<string>.Fail(AgentErrorCode.deserialization_failed, "Failed to deserialize import data.");

            // Validate before import if requested
            if (options.ValidateBeforeImport)
            {
                var validationIssues = new List<string>();
                ValidateImportData(importData, validationIssues);
                
                if (validationIssues.Any())
                {
                    return AgentResult<string>.Fail(AgentErrorCode.validation_failed, $"Import validation failed: {string.Join(", ", validationIssues)}");
                }
            }

            // Import tree structure
            if (!importData.ContainsKey("tree"))
                return AgentResult<string>.Fail(AgentErrorCode.invalid_argument, "Import data missing tree structure.");

            var treeJson = importData["tree"].ToString();
            var importedRoot = DeserializeTreeFromSnapshot(treeJson!);
            if (importedRoot == null)
                return AgentResult<string>.Fail(AgentErrorCode.deserialization_failed, "Failed to deserialize imported tree.");

            // Apply node ID mapping if provided
            if (options.NodeIdMapping.Any())
            {
                ApplyNodeIdMapping(importedRoot, options.NodeIdMapping);
            }

            // Note: Similar to snapshot restoration, we cannot replace readonly fields
            // In a real implementation, this would require service recreation
            return AgentResult<string>.Fail(AgentErrorCode.internal_error, "Tree import requires service recreation due to readonly field constraints.");
        }, isMutation: true, opts: mutationOpts);

    public AgentResult<string> ExportToFormat(AgentContext ctx, string rootNodeId, string format, ExportOptions options)
        => Guard(ctx, nameof(ExportToFormat), () =>
        {
            var root = _controller.FindNode(rootNodeId) ?? _root;
            
            switch (format.ToLowerInvariant())
            {
                case "json":
                    return ExportTree(ctx, rootNodeId, options);
                    
                case "xml":
                    return ExportToXml(ctx, root, options);
                    
                case "csv":
                    return ExportToCsv(ctx, root, options);
                    
                default:
                    return AgentResult<string>.Fail(AgentErrorCode.invalid_argument, $"Unsupported export format: {format}");
            }
        });

    public AgentResult<Dictionary<string, object>> GetExportMetadata(AgentContext ctx, string rootNodeId)
        => Guard(ctx, nameof(GetExportMetadata), () =>
        {
            var root = _controller.FindNode(rootNodeId) ?? _root;
            var metadata = GetExportMetadataInternal(root);
            return AgentResult<Dictionary<string, object>>.Ok(metadata);
        });

    private Dictionary<string, object> GetExportMetadataInternal(ITreeNode root)
    {
        var metadata = new Dictionary<string, object>
        {
            ["nodeCount"] = CountNodes(root),
            ["maxDepth"] = GetMaxDepth(root),
            ["payloadTypes"] = GetPayloadTypes(root),
            ["exportTimestamp"] = DateTime.UtcNow,
            ["rootNodeId"] = root.NodeId
        };

        return metadata;
    }

    private AgentResult<string> ExportToXml(AgentContext ctx, ITreeNode root, ExportOptions options)
    {
        try
        {
            var xml = new System.Text.StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<Tree>");
            
            AppendNodeToXml(root, xml, 1);
            
            xml.AppendLine("</Tree>");
            
            return AgentResult<string>.Ok(xml.ToString());
        }
        catch (Exception ex)
        {
            return AgentResult<string>.Fail(AgentErrorCode.internal_error, $"XML export failed: {ex.Message}");
        }
    }

    private AgentResult<string> ExportToCsv(AgentContext ctx, ITreeNode root, ExportOptions options)
    {
        try
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("NodeId,ParentId,Depth,PayloadType,Name,SquareFeet"); // Header
            
            AppendNodeToCsv(root, csv);
            
            return AgentResult<string>.Ok(csv.ToString());
        }
        catch (Exception ex)
        {
            return AgentResult<string>.Fail(AgentErrorCode.internal_error, $"CSV export failed: {ex.Message}");
        }
    }

    private void AppendNodeToXml(ITreeNode node, System.Text.StringBuilder xml, int depth)
    {
        var indent = new string(' ', depth * 2);
        xml.AppendLine($"{indent}<Node>");
        xml.AppendLine($"{indent}  <NodeId>{node.NodeId}</NodeId>");
        xml.AppendLine($"{indent}  <PayloadType>{node.PayloadType}</PayloadType>");
        
        if (node.PayloadObject != null)
        {
            xml.AppendLine($"{indent}  <Payload>");
            var payloadType = node.PayloadObject.GetType();
            var properties = payloadType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var prop in properties)
            {
                if (!prop.CanRead) continue;
                var value = prop.GetValue(node.PayloadObject);
                xml.AppendLine($"{indent}    <{prop.Name}>{value}</{prop.Name}>");
            }
            xml.AppendLine($"{indent}  </Payload>");
        }
        
        foreach (var child in node.Children)
        {
            AppendNodeToXml(child, xml, depth + 1);
        }
        
        xml.AppendLine($"{indent}</Node>");
    }

    private void AppendNodeToCsv(ITreeNode node, System.Text.StringBuilder csv)
    {
        var parentId = node.Parent?.NodeId ?? "";
        var payloadType = node.PayloadType ?? "";
        var name = "";
        var squareFeet = "";
        
        if (node.PayloadObject != null)
        {
            var payloadTypeObj = node.PayloadObject.GetType();
            var nameProp = payloadTypeObj.GetProperty("Name");
            var squareFeetProp = payloadTypeObj.GetProperty("SquareFeet");
            
            if (nameProp != null) name = nameProp.GetValue(node.PayloadObject)?.ToString() ?? "";
            if (squareFeetProp != null) squareFeet = squareFeetProp.GetValue(node.PayloadObject)?.ToString() ?? "";
        }
        
        csv.AppendLine($"{node.NodeId},{parentId},{node.Depth},{payloadType},{name},{squareFeet}");
        
        foreach (var child in node.Children)
        {
            AppendNodeToCsv(child, csv);
        }
    }

    private int CountNodes(ITreeNode root)
    {
        int count = 1;
        foreach (var child in root.Children)
        {
            count += CountNodes(child);
        }
        return count;
    }

    private int GetMaxDepth(ITreeNode root)
    {
        int maxDepth = root.Depth;
        foreach (var child in root.Children)
        {
            maxDepth = Math.Max(maxDepth, GetMaxDepth(child));
        }
        return maxDepth;
    }

    private List<string> GetPayloadTypes(ITreeNode root)
    {
        var types = new HashSet<string>();
        CollectPayloadTypes(root, types);
        return types.ToList();
    }

    private void CollectPayloadTypes(ITreeNode node, HashSet<string> types)
    {
        if (!string.IsNullOrEmpty(node.PayloadType))
        {
            types.Add(node.PayloadType);
        }
        
        foreach (var child in node.Children)
        {
            CollectPayloadTypes(child, types);
        }
    }

    private void ValidateImportData(Dictionary<string, object> importData, List<string> issues)
    {
        if (!importData.ContainsKey("version"))
            issues.Add("Missing version information");
            
        if (!importData.ContainsKey("tree"))
            issues.Add("Missing tree structure");
            
        if (importData.ContainsKey("tree"))
        {
            var treeJson = importData["tree"].ToString();
            if (string.IsNullOrEmpty(treeJson))
                issues.Add("Empty tree structure");
        }
    }

    private void ApplyNodeIdMapping(ITreeNode root, Dictionary<string, string> mapping)
    {
        // Note: This would require modifying node IDs, which isn't possible with readonly fields
        // In a real implementation, this would be handled during tree reconstruction
    }

    private bool IsBase64String(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length % 4 != 0)
            return false;

        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class SnapshotInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Dictionary<string, object?> Metadata { get; set; } = new();
        public string RootNodeId { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string TreeData { get; set; } = string.Empty;
        public string? ViewState { get; set; }
        public string? Tags { get; set; }
        public string? Bookmarks { get; set; }
    }

    private sealed class BookmarkInfo
    {
        public string Id { get; set; } = string.Empty;
        public string NodeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Dictionary<string, object?> Metadata { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}



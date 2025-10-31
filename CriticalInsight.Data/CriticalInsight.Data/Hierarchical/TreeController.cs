using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CriticalInsight.Data.Hierarchical;

public class TreeController<T>
    where T : ITreeNode
{
    public T TreeRoot { get; init; }

    public TreeView TreeView { get; init; }

    public TreeController(T treeRoot, TreeView treeView)
    {
        TreeRoot = treeRoot;
        TreeView = treeView;
    }

    // Helper: Find a node by NodeId (recursive search)
    public ITreeNode? FindNode(string nodeId) => 
        TreeRoot.NodeId == nodeId ? TreeRoot : FindNodeRecursive(TreeRoot, nodeId);

    private ITreeNode? FindNodeRecursive(ITreeNode node, string nodeId)
    {
        foreach (var child in node.Children)
        {
            if (child.NodeId == nodeId)
                return child;

            var found = FindNodeRecursive(child, nodeId);
            if (found != null)
                return found;
        }

        return null;
    }

    #region Node Expansion/Collapse

    public void ExpandNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node != null)
        {
            TreeView.SetIsExpanded(node, true);
            TreeView.UpdateProjectedCollection();
        }
    }

    public void CollapseNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node != null)
        {
            TreeView.SetIsExpanded(node, false);
            TreeView.UpdateProjectedCollection();
        }
    }

    public void ToggleNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node != null)
        {
            bool current = TreeView.GetIsExpanded(node);
            TreeView.SetIsExpanded(node, !current);
            TreeView.UpdateProjectedCollection();
        }
    }

    public void ExpandAll()
    {
        void SetAll(ITreeNode node)
        {
            TreeView.SetIsExpanded(node, true);
            foreach (var child in node.Children)
                SetAll(child);
        }
        SetAll(TreeRoot);
        TreeView.UpdateProjectedCollection();
    }

    public void CollapseAll()
    {
        void SetAll(ITreeNode node)
        {
            TreeView.SetIsExpanded(node, false);
            foreach (var child in node.Children)
                SetAll(child);
        }
        SetAll(TreeRoot);
        TreeView.UpdateProjectedCollection();
    }

    #endregion

    #region Property Filtering

    public void SetIncludedProperties(IEnumerable<string> properties)
    {
        TreeView.IncludedProperties = properties.ToList();
        TreeView.UpdateProjectedCollection();
    }

    public void SetExcludedProperties(IEnumerable<string> properties)
    {
        TreeView.ExcludedProperties = properties.ToList();
        TreeView.UpdateProjectedCollection();
    }

    #endregion

    #region Node Modification

    public void AddChild(string parentNodeId, ITreeNode newChild)
    {
        var parent = FindNode(parentNodeId);
        if (parent != null)
        {
            parent.Children.Add(newChild);
            newChild.SetParent(parent);
            TreeView.UpdateProjectedCollection();
        }
    }

    // Update only properties on the payload object.
    // If the payload is the same as the node (i.e. for ITreeNode types that inherit from TreeNode<T>),
    // restrict updates to avoid changing internal properties (e.g. NodeId, Children, Parent, etc).
    public void UpdateNodePayloadProperty(string nodeId, string propertyName, object newValue)
    {
        var node = FindNode(nodeId);
        if (node != null)
        {
            // If the node's payload is itself, ensure that only "safe" properties are updated.
            if (ReferenceEquals(node, node.PayloadObject))
            {
                var forbidden = new HashSet<string>
                {
                    nameof(ITreeNode.NodeId),
                    nameof(ITreeNode.Children),
                    nameof(ITreeNode.Parent),
                    nameof(ITreeNode.PayloadType)
                };
                if (forbidden.Contains(propertyName))
                    throw new InvalidOperationException($"Updating property '{propertyName}' is not allowed.");
            }

            // Use reflection to update the property on the payload.
            PropertyInfo? prop = node.PayloadObject.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(node.PayloadObject, newValue);
            }
        }
    }

    // Batch update payload properties.
    public void UpdateNodesPayloadProperty(IEnumerable<string> nodeIds, string propertyName, object newValue)
    {
        foreach (var id in nodeIds)
        {
            UpdateNodePayloadProperty(id, propertyName, newValue);
        }
    }

    public void RemoveNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node != null && node.Parent != null)
        {
            node.Parent.Children.Remove(node);
            TreeView.UpdateProjectedCollection();
        }
    }

    public void RemoveNodes(IEnumerable<string> nodeIds)
    {
        foreach (var id in nodeIds.ToList())
        {
            RemoveNode(id);
        }
    }

    public void MoveNode(string nodeId, string newParentId)
    {
        var node = FindNode(nodeId);
        var newParent = FindNode(newParentId);
        if (node != null && newParent != null && node.Parent != null)
        {
            node.Parent.Children.Remove(node);
            node.SetParent(newParent);
            newParent.Children.Add(node);
            TreeView.UpdateProjectedCollection();
        }
    }

    #endregion

    #region Query and Navigation

    public IEnumerable<ITreeNode> GetDescendants(string nodeId)
    {
        var node = FindNode(nodeId);
        return node != null ? node.Descendants : Enumerable.Empty<ITreeNode>();
    }

    public IEnumerable<ITreeNode> GetAncestors(string nodeId)
    {
        var node = FindNode(nodeId);
        return node != null ? node.Ancestors : Enumerable.Empty<ITreeNode>();
    }

    public IEnumerable<ITreeNode> SearchNodes(Func<ITreeNode, bool> predicate)
    {
        var results = new List<ITreeNode>();
        void Search(ITreeNode node)
        {
            if (predicate(node))
                results.Add(node);
            foreach (var child in node.Children)
                Search(child);
        }
        Search(TreeRoot);
        return results;
    }

    #endregion

    #region Serialization & Persistence

    public string ExportToJson(Dictionary<string, Type> payloadTypes, bool includeViewRoot = true, bool writeIndented = true)
    {
        return TreeViewJsonSerializer.Serialize(TreeView, payloadTypes, includeViewRoot, writeIndented);
    }

    public void ImportFromJson(string json, Dictionary<string, Type> payloadTypes, Func<string, ITreeNode?> nodeLookup)
    {
        // Deserialize a new TreeView and update relevant properties.
        TreeView? newTreeView = TreeViewJsonSerializer.Deserialize(json, payloadTypes, nodeLookup);
        if (newTreeView != null)
        {
            TreeView.IncludedProperties = newTreeView.IncludedProperties;
            TreeView.ExcludedProperties = newTreeView.ExcludedProperties;
            // Optionally, update expansion states or other settings.
            TreeView.UpdateProjectedCollection();
        }
    }

    #endregion
}
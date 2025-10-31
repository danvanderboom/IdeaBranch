using System;
using System.Collections.ObjectModel;
using CriticalInsight.Data.Hierarchical;

namespace IdeaBranch.App.Adapters;

/// <summary>
/// Provider that creates and manages a TreeView over a TreeNode&lt;TopicNodePayload&gt; tree.
/// Exposes ProjectedCollection for binding to MAUI CollectionView with Depth-based indentation.
/// </summary>
public class TopicTreeViewProvider
{
    private readonly TopicTreeAdapter _adapter;
    private TreeView? _treeView;

    public TopicTreeViewProvider(TopicTreeAdapter adapter)
    {
        _adapter = adapter;
    }

    /// <summary>
    /// Creates a TreeView from a domain topic tree.
    /// </summary>
    /// <param name="rootDomainNode">The root domain TopicNode</param>
    /// <param name="defaultExpanded">Whether nodes should be expanded by default</param>
    public void InitializeTreeView(IdeaBranch.Domain.TopicNode rootDomainNode, bool defaultExpanded = true)
    {
        var rootTreeNode = _adapter.BuildTree(rootDomainNode);
        _treeView = new TreeView(rootTreeNode, defaultExpanded);
    }

    /// <summary>
    /// Gets the TreeView's ProjectedCollection for binding to CollectionView.
    /// </summary>
    public ObservableCollection<ITreeNode> ProjectedCollection
    {
        get
        {
            if (_treeView == null)
                throw new InvalidOperationException("TreeView not initialized. Call InitializeTreeView first.");
            
            return _treeView.ProjectedCollection;
        }
    }

    /// <summary>
    /// Gets or sets the expanded state of a node.
    /// </summary>
    public void SetIsExpanded(ITreeNode node, bool isExpanded)
    {
        if (_treeView == null)
            throw new InvalidOperationException("TreeView not initialized. Call InitializeTreeView first.");
        
        _treeView.SetIsExpanded(node, isExpanded);
    }

    /// <summary>
    /// Gets the expanded state of a node.
    /// </summary>
    public bool GetIsExpanded(ITreeNode node)
    {
        if (_treeView == null)
            throw new InvalidOperationException("TreeView not initialized. Call InitializeTreeView first.");
        
        return _treeView.GetIsExpanded(node);
    }

    /// <summary>
    /// Checks if a node is visible in the current projection.
    /// </summary>
    public bool IsNodeVisible(ITreeNode node)
    {
        if (_treeView == null)
            throw new InvalidOperationException("TreeView not initialized. Call InitializeTreeView first.");
        
        return _treeView.IsNodeVisible(node);
    }

    /// <summary>
    /// Gets the root TreeNode.
    /// </summary>
    public ITreeNode? RootNode => _treeView?.Root;
}


using System;
using System.Collections.ObjectModel;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.Domain;

namespace IdeaBranch.App.Adapters;

/// <summary>
/// Provider that creates and manages a TreeView over a TreeNode&lt;TagTaxonomyPayload&gt; tree.
/// Exposes ProjectedCollection for binding to MAUI CollectionView with Depth-based indentation.
/// </summary>
public class TagTaxonomyViewProvider
{
    private readonly TagTaxonomyAdapter _adapter;
    private TreeView? _treeView;

    public TagTaxonomyViewProvider(TagTaxonomyAdapter adapter)
    {
        _adapter = adapter;
    }

    /// <summary>
    /// Creates a TreeView from a domain tag taxonomy tree.
    /// </summary>
    /// <param name="rootDomainNode">The root domain TagTaxonomyNode</param>
    /// <param name="defaultExpanded">Whether nodes should be expanded by default</param>
    public void InitializeTreeView(TagTaxonomyNode rootDomainNode, bool defaultExpanded = true)
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
    /// Gets the adapter for mapping between domain nodes and tree nodes.
    /// </summary>
    public TagTaxonomyAdapter Adapter => _adapter;
}


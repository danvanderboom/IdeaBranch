using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace CriticalInsight.Data.Hierarchical;

public class TreeView : INotifyPropertyChanged
{
    private readonly Dictionary<ITreeNode, bool> _expandedStates = new();

    public bool DefaultExpanded { get; set; }

    public List<string> IncludedProperties { get; set; } = new();
    public List<string> ExcludedProperties { get; set; } = new();

    public ITreeNode Root { get; protected set; }

    public ObservableCollection<ITreeNode> ProjectedCollection { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public TreeView(ITreeNode root, bool defaultExpanded = true)
    {
        Root = root;
        DefaultExpanded = defaultExpanded;
        InitializeExpandedState(Root);
        SubscribeToNode(Root);
        UpdateProjectedCollection();
    }

    // Recursively initialize expansion state for the entire tree.
    private void InitializeExpandedState(ITreeNode node)
    {
        if (!_expandedStates.ContainsKey(node))
            _expandedStates[node] = DefaultExpanded;

        foreach (var child in node.Children)
            InitializeExpandedState(child);
    }

    // Subscribe to property and collection changes for a node and its subtree.
    private void SubscribeToNode(ITreeNode node)
    {
        node.PropertyChanged += Node_PropertyChanged;
        node.Children.CollectionChanged += OnChildrenCollectionChanged;

        foreach (var child in node.Children)
        {
            SubscribeToNode(child);
        }
    }

    // Unsubscribe from property and collection changes for a node and its subtree.
    private void UnsubscribeFromNode(ITreeNode node)
    {
        node.PropertyChanged -= Node_PropertyChanged;
        node.Children.CollectionChanged -= OnChildrenCollectionChanged;

        foreach (var child in node.Children)
        {
            UnsubscribeFromNode(child);
        }
    }

    // CollectionChanged event handler for Children collections.
    private void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ITreeNode newNode in e.NewItems)
            {
                InitializeExpandedState(newNode);
                SubscribeToNode(newNode);
            }
        }

        if (e.OldItems != null)
        {
            foreach (ITreeNode oldNode in e.OldItems)
            {
                UnsubscribeFromNode(oldNode);
                _expandedStates.Remove(oldNode);
            }
        }

        UpdateProjectedCollection();
    }

    // React to property changes on nodes.
    private void Node_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // In the absence of an IsExpanded property on ITreeNode,
        // we might trigger an update if properties affecting appearance change.
        UpdateProjectedCollection();
    }

    // Returns the expanded state for a node. If not tracked, returns the default.
    public bool GetIsExpanded(ITreeNode node) =>
        _expandedStates.TryGetValue(node, out bool isExpanded) ? isExpanded : DefaultExpanded;

    // Updates the expanded state for a node and refreshes the view.
    public void SetIsExpanded(ITreeNode node, bool isExpanded)
    {
        _expandedStates[node] = isExpanded;
        UpdateProjectedCollection();
    }

    // Rebuild the flattened view based on each node's (or view's overridden) expanded state.
    // Incrementally update the flattened projected collection.
    public void UpdateProjectedCollection()
    {
        // Build the new list of visible nodes, including the root.
        var newVisibleNodes = new List<ITreeNode>();
        AddVisibleNodes(Root, newVisibleNodes);

        // For display purposes, skip the root node.
        int newIndex = 0;
        foreach (var node in newVisibleNodes.Skip(1))
        {
            if (newIndex < ProjectedCollection.Count)
            {
                if (!object.ReferenceEquals(ProjectedCollection[newIndex], node))
                    ProjectedCollection.Insert(newIndex, node);
            }
            else
            {
                ProjectedCollection.Add(node);
            }
            newIndex++;
        }

        // Remove any extra nodes that no longer belong.
        while (ProjectedCollection.Count > newIndex)
        {
            ProjectedCollection.RemoveAt(newIndex);
        }

        OnPropertyChanged(nameof(ProjectedCollection));
    }

    // Recursively add visible nodes to the list, based on the expanded state.
    private void AddVisibleNodes(ITreeNode node, List<ITreeNode> list)
    {
        list.Add(node);
        if (GetIsExpanded(node))
        {
            foreach (var child in node.Children)
                AddVisibleNodes(child, list);
        }
    }

    public bool IsNodeVisible(ITreeNode node)
    {
        foreach (var ancestor in node.Ancestors)
        {
            if (!GetIsExpanded(ancestor))
                return false;
        }
        return true;
    }

    public string Serialize(Dictionary<string, Type> payloadTypes, bool includeViewRoot = false) =>
        TreeViewJsonSerializer.Serialize(this, payloadTypes, includeViewRoot);
}
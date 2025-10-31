using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for TopicTreePage that manages the TreeView projection and expansion state.
/// </summary>
public class TopicTreeViewModel : INotifyPropertyChanged
{
    private readonly TopicTreeViewProvider _viewProvider;
    private readonly TopicTreeAdapter _adapter;

    public TopicTreeViewModel()
    {
        _adapter = new TopicTreeAdapter();
        _viewProvider = new TopicTreeViewProvider(_adapter);
        
        // TODO: Initialize from domain repository once implemented
        // For now, create a placeholder tree for testing
        var placeholderRoot = new { Id = Guid.NewGuid() };
        _viewProvider.InitializeTreeView(placeholderRoot, defaultExpanded: true);
    }

    /// <summary>
    /// Gets the projected collection of visible tree nodes for binding to CollectionView.
    /// </summary>
    public ObservableCollection<ITreeNode> ProjectedCollection 
        => _viewProvider.ProjectedCollection;

    /// <summary>
    /// Toggles the expanded state of a node.
    /// </summary>
    public void ToggleExpansion(ITreeNode node)
    {
        var currentState = _viewProvider.GetIsExpanded(node);
        _viewProvider.SetIsExpanded(node, !currentState);
        OnPropertyChanged(nameof(ProjectedCollection));
    }

    /// <summary>
    /// Gets the payload for a node.
    /// </summary>
    public static TopicNodePayload? GetPayload(ITreeNode node)
    {
        if (node is ITreeNode<TopicNodePayload> typedNode)
        {
            return typedNode.Payload;
        }
        return null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


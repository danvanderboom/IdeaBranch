using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;
using IdeaBranch.Domain;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for TopicTreePage that manages the TreeView projection and expansion state.
/// </summary>
public class TopicTreeViewModel : INotifyPropertyChanged
{
    private readonly TopicTreeViewProvider _viewProvider;
    private readonly TopicTreeAdapter _adapter;
    private readonly ITopicTreeRepository _repository;

    /// <summary>
    /// Initializes a new instance with the topic tree repository.
    /// </summary>
    public TopicTreeViewModel(ITopicTreeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _adapter = new TopicTreeAdapter();
        _viewProvider = new TopicTreeViewProvider(_adapter);
        
        // Initialize from repository
        InitializeFromRepository();
    }

    /// <summary>
    /// Initializes the tree view from the repository (parameterless constructor for XAML).
    /// </summary>
    public TopicTreeViewModel() : this(new InMemoryTopicTreeRepository())
    {
    }

    private async void InitializeFromRepository()
    {
        try
        {
            var root = await _repository.GetRootAsync();
            _viewProvider.InitializeTreeView(root, defaultExpanded: false);
            OnPropertyChanged(nameof(ProjectedCollection));
        }
        catch
        {
            // Fallback to placeholder if repository fails
            var placeholderRoot = new TopicNode("What would you like to explore?", "Root Topic");
            _viewProvider.InitializeTreeView(placeholderRoot, defaultExpanded: false);
        }
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


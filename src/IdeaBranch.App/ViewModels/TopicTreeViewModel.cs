using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;
using IdeaBranch.App.Services.LLM;
using IdeaBranch.App.Views;
using IdeaBranch.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for TopicTreePage that manages the TreeView projection and expansion state.
/// </summary>
public class TopicTreeViewModel : INotifyPropertyChanged
{
    private readonly TopicTreeViewProvider _viewProvider;
    private readonly TopicTreeAdapter _adapter;
    private readonly ITopicTreeRepository _repository;
    private readonly LLMClientFactory? _llmFactory;
    private readonly Services.TelemetryService? _telemetry;
    private TopicNode? _rootDomainNode;

    /// <summary>
    /// Initializes a new instance with the topic tree repository.
    /// </summary>
    public TopicTreeViewModel(ITopicTreeRepository repository, LLMClientFactory? llmFactory = null, Services.TelemetryService? telemetry = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _llmFactory = llmFactory;
        _telemetry = telemetry;
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
            _rootDomainNode = await _repository.GetRootAsync();
            _viewProvider.InitializeTreeView(_rootDomainNode, defaultExpanded: false);
            OnPropertyChanged(nameof(ProjectedCollection));
        }
        catch
        {
            // Fallback to placeholder if repository fails
            _rootDomainNode = new TopicNode("What would you like to explore?", "Root Topic");
            _viewProvider.InitializeTreeView(_rootDomainNode, defaultExpanded: false);
        }
    }

    /// <summary>
    /// Saves the current tree state and refreshes the view.
    /// </summary>
    private async Task SaveAndRefreshAsync()
    {
        if (_rootDomainNode == null)
            return;

        try
        {
            await _repository.SaveAsync(_rootDomainNode);
            
            // Rebuild view from updated domain tree
            _viewProvider.InitializeTreeView(_rootDomainNode, defaultExpanded: false);
            OnPropertyChanged(nameof(ProjectedCollection));
        }
        catch
        {
            // Error handling will be added later
        }
    }

    /// <summary>
    /// Finds a domain TopicNode by its ID in the tree.
    /// </summary>
    private TopicNode? FindDomainNode(Guid nodeId)
    {
        if (_rootDomainNode == null)
            return null;

        if (_rootDomainNode.Id == nodeId)
            return _rootDomainNode;

        return FindDomainNodeRecursive(_rootDomainNode, nodeId);
    }

    /// <summary>
    /// Recursively searches for a domain node by ID.
    /// </summary>
    private TopicNode? FindDomainNodeRecursive(TopicNode node, Guid nodeId)
    {
        foreach (var child in node.Children)
        {
            if (child.Id == nodeId)
                return child;

            var found = FindDomainNodeRecursive(child, nodeId);
            if (found != null)
                return found;
        }

        return null;
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

    /// <summary>
    /// Gets the domain node ID from an ITreeNode.
    /// </summary>
    private Guid? GetDomainNodeId(ITreeNode node)
    {
        var payload = GetPayload(node);
        return payload?.DomainNodeId;
    }

    /// <summary>
    /// Adds a new child node to the specified parent.
    /// </summary>
    public async Task AddChildAsync(ITreeNode parentNode)
    {
        var parentId = GetDomainNodeId(parentNode);
        if (parentId == null)
            return;

        var parentDomainNode = FindDomainNode(parentId.Value);
        if (parentDomainNode == null)
            return;

        var newChild = new TopicNode("New Topic", "Untitled");
        parentDomainNode.AddChild(newChild);

        await SaveAndRefreshAsync();
        
        // Emit telemetry
        _telemetry?.EmitCrudEvent("create", newChild.Id.ToString());
    }

    /// <summary>
    /// Adds a new sibling node after the specified node.
    /// </summary>
    public async Task AddSiblingAsync(ITreeNode node)
    {
        var nodeId = GetDomainNodeId(node);
        if (nodeId == null)
            return;

        var domainNode = FindDomainNode(nodeId.Value);
        if (domainNode == null || domainNode.Parent == null)
            return; // Can't add sibling to root

        var newSibling = new TopicNode("New Topic", "Untitled");
        var parent = domainNode.Parent;
        
        // Find the next order position
        var maxOrder = parent.Children.Count > 0 
            ? parent.Children.Max(c => c.Order) 
            : -1;
        newSibling.Order = maxOrder + 1;
        
        parent.AddChild(newSibling);

        await SaveAndRefreshAsync();
        
        // Emit telemetry
        _telemetry?.EmitCrudEvent("create", newSibling.Id.ToString());
    }

    /// <summary>
    /// Deletes a node from the tree.
    /// </summary>
    public async Task DeleteNodeAsync(ITreeNode node)
    {
        var nodeId = GetDomainNodeId(node);
        if (nodeId == null)
            return;

        var domainNode = FindDomainNode(nodeId.Value);
        if (domainNode == null || domainNode.Parent == null)
            return; // Can't delete root

        var deletedNodeId = domainNode.Id.ToString();
        domainNode.Parent.RemoveChild(domainNode);

        await SaveAndRefreshAsync();
        
        // Emit telemetry
        _telemetry?.EmitCrudEvent("delete", deletedNodeId);
    }

    /// <summary>
    /// Moves a node to a new parent.
    /// </summary>
    public async Task MoveNodeAsync(ITreeNode node, ITreeNode newParentNode)
    {
        var nodeId = GetDomainNodeId(node);
        var newParentId = GetDomainNodeId(newParentNode);
        
        if (nodeId == null || newParentId == null)
            return;

        var domainNode = FindDomainNode(nodeId.Value);
        var newParentDomainNode = FindDomainNode(newParentId.Value);
        
        if (domainNode == null || newParentDomainNode == null || domainNode.Parent == null)
            return;

        domainNode.Parent.MoveChild(domainNode, newParentDomainNode);

        await SaveAndRefreshAsync();
        
        // Emit telemetry
        _telemetry?.EmitCrudEvent("move", domainNode.Id.ToString());
    }

    /// <summary>
    /// Gets the domain TopicNode for an ITreeNode. Used for editing.
    /// </summary>
    public TopicNode? GetDomainNode(ITreeNode node)
    {
        var nodeId = GetDomainNodeId(node);
        if (nodeId == null)
            return null;

        return FindDomainNode(nodeId.Value);
    }

    /// <summary>
    /// Navigates to the detail page for editing a node.
    /// </summary>
    public async Task EditNodeAsync(ITreeNode node)
    {
        var domainNode = GetDomainNode(node);
        if (domainNode == null)
            return;

        // Get LLM factory from services if not provided
        var llmFactory = _llmFactory;
        if (llmFactory == null)
        {
            var services = Application.Current?.MainPage?.Handler?.MauiContext?.Services;
            llmFactory = services?.GetService<LLMClientFactory>();
        }

        // Get telemetry service
        var telemetry = _telemetry;
        if (telemetry == null)
        {
            var services = Application.Current?.MainPage?.Handler?.MauiContext?.Services;
            telemetry = services?.GetService<Services.TelemetryService>();
        }

        // Get annotations repository
        var annotationsRepository = Application.Current?.MainPage?.Handler?.MauiContext?.Services?
            .GetService<Domain.IAnnotationsRepository>();

        // Create detail ViewModel with save callback
        var detailViewModel = new TopicNodeDetailViewModel(
            domainNode, 
            async (updatedNode) =>
            {
                // Update the domain tree (node is already updated in place)
                await SaveAndRefreshAsync();
            },
            llmFactory ?? throw new InvalidOperationException("LLMClientFactory is not available."),
            async (updatedNode) =>
            {
                // Refresh view when children are added
                await SaveAndRefreshAsync();
            },
            telemetry,
            annotationsRepository);

        // Create detail page and navigate
        var detailPage = new Views.TopicNodeDetailPage(detailViewModel);
        
        // Get current page for navigation
        var currentPage = Application.Current?.MainPage;
        if (currentPage != null)
        {
            await currentPage.Navigation.PushAsync(detailPage);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


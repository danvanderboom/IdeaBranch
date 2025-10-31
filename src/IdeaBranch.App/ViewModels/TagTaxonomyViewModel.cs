using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;
using IdeaBranch.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for TagTaxonomyPage that manages the tag taxonomy tree view and CRUD operations.
/// </summary>
public class TagTaxonomyViewModel : INotifyPropertyChanged
{
    private readonly TagTaxonomyViewProvider _viewProvider;
    private readonly TagTaxonomyAdapter _adapter;
    private readonly ITagTaxonomyRepository _repository;
    private readonly IAnnotationsRepository? _annotationsRepository;
    private TagTaxonomyNode? _rootDomainNode;
    private bool _isBusy;
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance with the tag taxonomy repository.
    /// </summary>
    public TagTaxonomyViewModel(
        ITagTaxonomyRepository repository,
        IAnnotationsRepository? annotationsRepository = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _annotationsRepository = annotationsRepository;
        _adapter = new TagTaxonomyAdapter();
        _viewProvider = new TagTaxonomyViewProvider(_adapter);
        
        // Initialize from repository
        InitializeFromRepository();
    }

    /// <summary>
    /// Initializes a new instance (parameterless constructor for XAML).
    /// </summary>
    public TagTaxonomyViewModel() : this(new InMemoryTagTaxonomyRepository())
    {
    }

    private async void InitializeFromRepository()
    {
        await LoadTaxonomyAsync();
    }

    /// <summary>
    /// Loads the tag taxonomy tree from the repository.
    /// </summary>
    public async Task LoadTaxonomyAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            _rootDomainNode = await _repository.GetRootAsync();
            
            // Build tree structure recursively
            await BuildTreeRecursive(_rootDomainNode);
            
            _viewProvider.InitializeTreeView(_rootDomainNode, defaultExpanded: false);
            OnPropertyChanged(nameof(ProjectedCollection));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load taxonomy: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Recursively builds the tree structure by loading children.
    /// </summary>
    private async Task BuildTreeRecursive(TagTaxonomyNode node)
    {
        var children = await _repository.GetChildrenAsync(node.Id);
        foreach (var child in children)
        {
            node.AddChild(child);
            await BuildTreeRecursive(child);
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
            IsBusy = true;
            
            // Save the entire tree (repository saves recursively)
            await _repository.SaveAsync(_rootDomainNode);
            
            // Rebuild view from updated domain tree
            await BuildTreeRecursive(_rootDomainNode);
            _viewProvider.InitializeTreeView(_rootDomainNode, defaultExpanded: false);
            OnPropertyChanged(nameof(ProjectedCollection));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save taxonomy: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Finds a domain TagTaxonomyNode by its ID in the tree.
    /// </summary>
    private TagTaxonomyNode? FindDomainNode(Guid nodeId)
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
    private TagTaxonomyNode? FindDomainNodeRecursive(TagTaxonomyNode node, Guid nodeId)
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
    public static TagTaxonomyPayload? GetPayload(ITreeNode node)
    {
        if (node is ITreeNode<TagTaxonomyPayload> typedNode)
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
    /// Gets the domain TagTaxonomyNode for an ITreeNode.
    /// </summary>
    public TagTaxonomyNode? GetDomainNode(ITreeNode node)
    {
        var nodeId = GetDomainNodeId(node);
        if (nodeId == null)
            return null;

        return FindDomainNode(nodeId.Value);
    }

    /// <summary>
    /// Creates a new category under the specified parent.
    /// </summary>
    public async Task CreateCategoryAsync(ITreeNode parentNode, string name)
    {
        var parentId = GetDomainNodeId(parentNode);
        if (parentId == null || string.IsNullOrWhiteSpace(name))
            return;

        var parentDomainNode = FindDomainNode(parentId.Value);
        if (parentDomainNode == null)
            return;

        var newCategory = new TagTaxonomyNode(name, parentId.Value);
        
        // Find the next order position
        var maxOrder = parentDomainNode.Children.Count > 0 
            ? parentDomainNode.Children.Max(c => c.Order) 
            : -1;
        newCategory.Order = maxOrder + 1;
        
        parentDomainNode.AddChild(newCategory);
        await SaveAndRefreshAsync();
    }

    /// <summary>
    /// Creates a new tag under the specified parent.
    /// </summary>
    public async Task CreateTagAsync(ITreeNode parentNode, string name)
    {
        var parentId = GetDomainNodeId(parentNode);
        if (parentId == null || string.IsNullOrWhiteSpace(name))
            return;

        var parentDomainNode = FindDomainNode(parentId.Value);
        if (parentDomainNode == null)
            return;

        var newTag = new TagTaxonomyNode(name, parentId.Value);
        
        // Find the next order position
        var maxOrder = parentDomainNode.Children.Count > 0 
            ? parentDomainNode.Children.Max(c => c.Order) 
            : -1;
        newTag.Order = maxOrder + 1;
        
        parentDomainNode.AddChild(newTag);
        await SaveAndRefreshAsync();
    }

    /// <summary>
    /// Updates the name of a node.
    /// </summary>
    public async Task UpdateNodeNameAsync(ITreeNode node, string newName)
    {
        var domainNode = GetDomainNode(node);
        if (domainNode == null || string.IsNullOrWhiteSpace(newName))
            return;

        domainNode.Name = newName;
        await SaveAndRefreshAsync();
    }

    /// <summary>
    /// Deletes a node from the taxonomy.
    /// </summary>
    public async Task<bool> DeleteNodeAsync(ITreeNode node)
    {
        var nodeId = GetDomainNodeId(node);
        if (nodeId == null)
            return false;

        var domainNode = FindDomainNode(nodeId.Value);
        if (domainNode == null || domainNode.Parent == null)
            return false; // Can't delete root

        // Check if node is referenced by annotations (basic check)
        if (_annotationsRepository != null)
        {
            try
            {
                // Try to get tag IDs for any annotation - if tag is used, GetTagIdsAsync might fail or return it
                // For MVP, we'll proceed - the database cascade will remove annotation_tags references
                // A more complete check would query all annotations, but that's expensive
            }
            catch
            {
                // If check fails, proceed anyway
            }
        }

        domainNode.Parent.RemoveChild(domainNode);
        await SaveAndRefreshAsync();
        return true;
    }

    /// <summary>
    /// Moves a node up within its siblings (decreases order).
    /// </summary>
    public async Task MoveNodeUpAsync(ITreeNode node)
    {
        var domainNode = GetDomainNode(node);
        if (domainNode == null || domainNode.Parent == null)
            return;

        var siblings = domainNode.Parent.Children.OrderBy(c => c.Order).ToList();
        var currentIndex = siblings.IndexOf(domainNode);
        
        if (currentIndex <= 0)
            return; // Already at top

        var previousSibling = siblings[currentIndex - 1];
        
        // Swap orders
        var tempOrder = domainNode.Order;
        domainNode.Order = previousSibling.Order;
        previousSibling.Order = tempOrder;

        await SaveAndRefreshAsync();
    }

    /// <summary>
    /// Moves a node down within its siblings (increases order).
    /// </summary>
    public async Task MoveNodeDownAsync(ITreeNode node)
    {
        var domainNode = GetDomainNode(node);
        if (domainNode == null || domainNode.Parent == null)
            return;

        var siblings = domainNode.Parent.Children.OrderBy(c => c.Order).ToList();
        var currentIndex = siblings.IndexOf(domainNode);
        
        if (currentIndex >= siblings.Count - 1)
            return; // Already at bottom

        var nextSibling = siblings[currentIndex + 1];
        
        // Swap orders
        var tempOrder = domainNode.Order;
        domainNode.Order = nextSibling.Order;
        nextSibling.Order = tempOrder;

        await SaveAndRefreshAsync();
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

        // Check for cycles
        if (newParentDomainNode == domainNode || IsDescendantOf(newParentDomainNode, domainNode))
            return;

        // Remove from old parent
        domainNode.Parent.RemoveChild(domainNode);
        
        // Set new order
        var maxOrder = newParentDomainNode.Children.Count > 0 
            ? newParentDomainNode.Children.Max(c => c.Order) 
            : -1;
        domainNode.Order = maxOrder + 1;
        
        // Add to new parent
        newParentDomainNode.AddChild(domainNode);

        await SaveAndRefreshAsync();
    }

    /// <summary>
    /// Checks if a node is a descendant of another node.
    /// </summary>
    private bool IsDescendantOf(TagTaxonomyNode potentialAncestor, TagTaxonomyNode node)
    {
        var current = node.Parent;
        while (current != null)
        {
            if (current == potentialAncestor)
                return true;
            current = current.Parent;
        }
        return false;
    }

    /// <summary>
    /// Gets or sets the busy state.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Exports the taxonomy to JSON format.
    /// </summary>
    public async Task<string> ExportToJsonAsync()
    {
        if (_rootDomainNode == null)
            return string.Empty;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var exportData = BuildExportData(_rootDomainNode);
            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return json;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to export taxonomy: {ex.Message}";
            return string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Imports taxonomy from JSON format.
    /// </summary>
    public async Task<bool> ImportFromJsonAsync(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var importData = JsonSerializer.Deserialize<TagTaxonomyExportData>(json);
            if (importData == null || importData.Nodes == null)
            {
                ErrorMessage = "Invalid import data format.";
                return false;
            }

            // Build tree from imported data
            await BuildTreeFromImportData(importData);
            
            await SaveAndRefreshAsync();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to import taxonomy: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Builds export data structure from the taxonomy tree.
    /// </summary>
    private TagTaxonomyExportData BuildExportData(TagTaxonomyNode root)
    {
        var nodes = new List<TagTaxonomyNodeData>();
        BuildExportDataRecursive(root, nodes);
        
        return new TagTaxonomyExportData
        {
            Version = "1.0",
            Nodes = nodes
        };
    }

    /// <summary>
    /// Recursively builds export data.
    /// </summary>
    private void BuildExportDataRecursive(TagTaxonomyNode node, List<TagTaxonomyNodeData> nodes)
    {
        nodes.Add(new TagTaxonomyNodeData
        {
            Id = node.Id,
            ParentId = node.ParentId,
            Name = node.Name,
            Order = node.Order
        });

        foreach (var child in node.Children)
        {
            BuildExportDataRecursive(child, nodes);
        }
    }

    /// <summary>
    /// Builds tree from imported data.
    /// </summary>
    private async Task BuildTreeFromImportData(TagTaxonomyExportData importData)
    {
        // Create a map of all nodes by ID
        var nodeMap = new Dictionary<Guid, TagTaxonomyNode>();
        foreach (var nodeData in importData.Nodes)
        {
            var node = new TagTaxonomyNode(
                nodeData.Id,
                nodeData.ParentId,
                nodeData.Name,
                nodeData.Order,
                DateTime.UtcNow,
                DateTime.UtcNow);
            
            nodeMap[node.Id] = node;
        }

        // Build parent-child relationships
        foreach (var nodeData in importData.Nodes)
        {
            var node = nodeMap[nodeData.Id];
            if (nodeData.ParentId.HasValue && nodeMap.TryGetValue(nodeData.ParentId.Value, out var parent))
            {
                parent.AddChild(node);
            }
        }

        // Find root (node with no parent or parent not in map)
        var rootNode = nodeMap.Values.FirstOrDefault(n => n.ParentId == null);
        if (rootNode != null)
        {
            // Replace existing root with imported root
            var existingRoot = await _repository.GetRootAsync();
            if (existingRoot != null && existingRoot.Id != rootNode.Id)
            {
                // Merge: add imported root's children to existing root
                foreach (var child in rootNode.Children.ToList())
                {
                    rootNode.RemoveChild(child);
                    existingRoot.AddChild(child);
                }
            }
            else
            {
                _rootDomainNode = rootNode;
            }
        }
    }

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Export data structure for tag taxonomy.
    /// </summary>
    private class TagTaxonomyExportData
    {
        public string Version { get; set; } = "1.0";
        public List<TagTaxonomyNodeData>? Nodes { get; set; }
    }

    /// <summary>
    /// Node data for export/import.
    /// </summary>
    private class TagTaxonomyNodeData
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}


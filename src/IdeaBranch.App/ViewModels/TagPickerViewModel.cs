using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for TagPickerPopup that manages hierarchical tag selection with per-tag descendant control.
/// </summary>
public class TagPickerViewModel : INotifyPropertyChanged
{
    private readonly ITagTaxonomyRepository _tagTaxonomyRepository;
    private TagTaxonomyNode? _rootNode;
    private bool _isLoading;

    /// <summary>
    /// Initializes a new instance with the tag taxonomy repository.
    /// </summary>
    public TagPickerViewModel(ITagTaxonomyRepository tagTaxonomyRepository)
    {
        _tagTaxonomyRepository = tagTaxonomyRepository ?? throw new ArgumentNullException(nameof(tagTaxonomyRepository));
        TagItems = new ObservableCollection<TagPickerItem>();
    }

    /// <summary>
    /// Initializes a new instance (parameterless constructor for XAML).
    /// </summary>
    public TagPickerViewModel() : this(
        Application.Current?.Handler?.MauiContext?.Services?.GetRequiredService<ITagTaxonomyRepository>()
        ?? throw new InvalidOperationException("Services not available"))
    {
    }

    /// <summary>
    /// Gets the collection of tag picker items for display.
    /// </summary>
    public ObservableCollection<TagPickerItem> TagItems { get; }

    /// <summary>
    /// Gets or sets whether loading is in progress.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    /// <summary>
    /// Loads the tag taxonomy tree and initializes the picker items.
    /// </summary>
    public async Task LoadTagsAsync()
    {
        try
        {
            IsLoading = true;
            _rootNode = await _tagTaxonomyRepository.GetRootAsync();
            
            // Load full tree recursively
            await LoadTreeRecursive(_rootNode);

            // Build flat list with hierarchy visualization (skip root)
            TagItems.Clear();
            if (_rootNode != null)
            {
                BuildTagItems(_rootNode, TagItems, depth: 0);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Recursively loads children from repository.
    /// </summary>
    private async Task LoadTreeRecursive(TagTaxonomyNode node)
    {
        var children = await _tagTaxonomyRepository.GetChildrenAsync(node.Id);
        foreach (var child in children)
        {
            if (!node.Children.Any(c => c.Id == child.Id))
            {
                node.AddChild(child);
                await LoadTreeRecursive(child);
            }
        }
    }

    /// <summary>
    /// Builds flat list of tag items with hierarchy indication.
    /// </summary>
    private void BuildTagItems(TagTaxonomyNode node, ObservableCollection<TagPickerItem> items, int depth)
    {
        // Skip root node (it's not selectable - it's just the container)
        // Add all children recursively
        foreach (var child in node.Children.OrderBy(c => c.Order).ThenBy(c => c.Name))
        {
            BuildTagItemsRecursive(child, items, depth);
        }
    }

    /// <summary>
    /// Recursively builds tag items for a node tree.
    /// </summary>
    private void BuildTagItemsRecursive(TagTaxonomyNode node, ObservableCollection<TagPickerItem> items, int depth)
    {
        // Add this node
        items.Add(new TagPickerItem(node, depth));
        
        // Recursively add children with increased depth
        foreach (var child in node.Children.OrderBy(c => c.Order).ThenBy(c => c.Name))
        {
            BuildTagItemsRecursive(child, items, depth + 1);
        }
    }

    /// <summary>
    /// Gets the selected tag selections.
    /// </summary>
    public IReadOnlyList<TagSelection> GetSelectedSelections()
    {
        return TagItems
            .Where(item => item.IsSelected)
            .Select(item => new TagSelection(item.Node.Id, item.IncludeDescendants))
            .ToList();
    }

    /// <summary>
    /// Sets the selected tag selections from the provided list.
    /// </summary>
    public void SetSelectedSelections(IReadOnlyList<TagSelection> selections)
    {
        var selectionLookup = selections.ToDictionary(s => s.TagId, s => s.IncludeDescendants);
        
        foreach (var item in TagItems)
        {
            if (selectionLookup.TryGetValue(item.Node.Id, out var includeDescendants))
            {
                item.IsSelected = true;
                item.IncludeDescendants = includeDescendants;
            }
            else
            {
                item.IsSelected = false;
                item.IncludeDescendants = false;
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Represents a tag item in the picker with selection state.
/// </summary>
public class TagPickerItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _includeDescendants;
    private bool _isExpanded;

    public TagPickerItem(TagTaxonomyNode node, int depth)
    {
        Node = node ?? throw new ArgumentNullException(nameof(node));
        Depth = depth;
        _isExpanded = true; // Default expanded for picker
    }

    /// <summary>
    /// Gets the tag taxonomy node.
    /// </summary>
    public TagTaxonomyNode Node { get; }

    /// <summary>
    /// Gets the depth in the hierarchy (0 = root level, increases for children).
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// Gets or sets whether this tag is selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                
                // Reset include descendants when unselected
                if (!value)
                {
                    IncludeDescendants = false;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to include descendants when selected.
    /// </summary>
    public bool IncludeDescendants
    {
        get => _includeDescendants;
        set
        {
            if (_includeDescendants != value)
            {
                _includeDescendants = value;
                OnPropertyChanged(nameof(IncludeDescendants));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether this node is expanded (for showing children).
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
    }

    /// <summary>
    /// Gets the indentation margin for hierarchy display.
    /// </summary>
    public Thickness IndentationMargin => new Thickness(Depth * 20, 0, 0, 0);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


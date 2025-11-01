using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.App.Services.Search;
using IdeaBranch.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for advanced search that coordinates searches across content types.
/// </summary>
public class AdvancedSearchViewModel : INotifyPropertyChanged
{
    private readonly SearchCoordinator _searchCoordinator;
    private SearchResults? _results;
    private bool _isLoading;
    private string? _searchText;
    private DateTime? _updatedAtFrom;
    private DateTime? _updatedAtTo;
    private IReadOnlyList<Guid>? _selectedIncludeTags;
    private IReadOnlyList<Guid>? _selectedExcludeTags;
    private int? _pageSize;
    private int? _pageOffset;

    /// <summary>
    /// Initializes a new instance with the search coordinator.
    /// </summary>
    public AdvancedSearchViewModel(SearchCoordinator searchCoordinator)
    {
        _searchCoordinator = searchCoordinator ?? throw new ArgumentNullException(nameof(searchCoordinator));
        SelectedContentTypes = new ObservableCollection<SearchContentType>();
        ExecuteSearchCommand = new Command(async () => await ExecuteSearchAsync(), () => !IsLoading && SelectedContentTypes.Count > 0);
    }

    /// <summary>
    /// Initializes a new instance from dependency injection (parameterless constructor for XAML).
    /// </summary>
    public AdvancedSearchViewModel() : this(GetSearchCoordinatorFromServices())
    {
    }

    private static SearchCoordinator GetSearchCoordinatorFromServices()
    {
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Services not available");
        return services.GetRequiredService<SearchCoordinator>();
    }

    /// <summary>
    /// Gets or sets the selected content types to search.
    /// </summary>
    public ObservableCollection<SearchContentType> SelectedContentTypes { get; set; }

    /// <summary>
    /// Gets or sets the search text to find in content.
    /// </summary>
    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }

    /// <summary>
    /// Gets or sets the start of the UpdatedAt range filter.
    /// </summary>
    public DateTime? UpdatedAtFrom
    {
        get => _updatedAtFrom;
        set
        {
            if (_updatedAtFrom != value)
            {
                _updatedAtFrom = value;
                OnPropertyChanged(nameof(UpdatedAtFrom));
            }
        }
    }

    /// <summary>
    /// Gets or sets the end of the UpdatedAt range filter.
    /// </summary>
    public DateTime? UpdatedAtTo
    {
        get => _updatedAtTo;
        set
        {
            if (_updatedAtTo != value)
            {
                _updatedAtTo = value;
                OnPropertyChanged(nameof(UpdatedAtTo));
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected tag IDs to include (AND logic).
    /// </summary>
    public IReadOnlyList<Guid>? SelectedIncludeTags
    {
        get => _selectedIncludeTags;
        set
        {
            if (_selectedIncludeTags != value)
            {
                _selectedIncludeTags = value;
                OnPropertyChanged(nameof(SelectedIncludeTags));
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected tag IDs to exclude.
    /// </summary>
    public IReadOnlyList<Guid>? SelectedExcludeTags
    {
        get => _selectedExcludeTags;
        set
        {
            if (_selectedExcludeTags != value)
            {
                _selectedExcludeTags = value;
                OnPropertyChanged(nameof(SelectedExcludeTags));
            }
        }
    }

    /// <summary>
    /// Gets or sets the page size for pagination.
    /// </summary>
    public int? PageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize != value)
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
            }
        }
    }

    /// <summary>
    /// Gets or sets the page offset for pagination.
    /// </summary>
    public int? PageOffset
    {
        get => _pageOffset;
        set
        {
            if (_pageOffset != value)
            {
                _pageOffset = value;
                OnPropertyChanged(nameof(PageOffset));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether a search is currently in progress.
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
                ExecuteSearchCommand.ChangeCanExecute();
            }
        }
    }

    /// <summary>
    /// Gets or sets the search results.
    /// </summary>
    public SearchResults? Results
    {
        get => _results;
        set
        {
            if (_results != value)
            {
                _results = value;
                OnPropertyChanged(nameof(Results));
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(TotalResultCount));
            }
        }
    }

    /// <summary>
    /// Gets whether there are any search results.
    /// </summary>
    public bool HasResults => Results != null && (
        Results.Nodes.Count > 0 ||
        Results.Annotations.Count > 0 ||
        Results.Tags.Count > 0 ||
        Results.Templates.Count > 0);

    /// <summary>
    /// Gets the total count of results across all content types.
    /// </summary>
    public int TotalResultCount => Results != null
        ? Results.Nodes.Count + Results.Annotations.Count + Results.Tags.Count + Results.Templates.Count
        : 0;

    /// <summary>
    /// Gets the command to execute the search.
    /// </summary>
    public Command ExecuteSearchCommand { get; }

    /// <summary>
    /// Executes the search with current filter settings.
    /// </summary>
    public async Task ExecuteSearchAsync()
    {
        if (SelectedContentTypes.Count == 0)
            return;

        try
        {
            IsLoading = true;

            var request = new SearchRequest
            {
                ContentTypes = SelectedContentTypes.ToHashSet(),
                TextContains = SearchText,
                IncludeTags = SelectedIncludeTags,
                ExcludeTags = SelectedExcludeTags,
                UpdatedAtFrom = UpdatedAtFrom,
                UpdatedAtTo = UpdatedAtTo,
                PageSize = PageSize,
                PageOffset = PageOffset
            };

            Results = await _searchCoordinator.SearchAsync(request);
        }
        catch (Exception ex)
        {
            // In a real implementation, we'd show error messages to the user
            System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
            Results = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Adds a content type to the search.
    /// </summary>
    public void AddContentType(SearchContentType contentType)
    {
        if (!SelectedContentTypes.Contains(contentType))
        {
            SelectedContentTypes.Add(contentType);
            ExecuteSearchCommand.ChangeCanExecute();
        }
    }

    /// <summary>
    /// Removes a content type from the search.
    /// </summary>
    public void RemoveContentType(SearchContentType contentType)
    {
        if (SelectedContentTypes.Contains(contentType))
        {
            SelectedContentTypes.Remove(contentType);
            ExecuteSearchCommand.ChangeCanExecute();
        }
    }

    /// <summary>
    /// Clears all search filters and results.
    /// </summary>
    public void ClearFilters()
    {
        SearchText = null;
        UpdatedAtFrom = null;
        UpdatedAtTo = null;
        SelectedIncludeTags = null;
        SelectedExcludeTags = null;
        PageSize = null;
        PageOffset = null;
        Results = null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


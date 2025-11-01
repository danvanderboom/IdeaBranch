using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.App.Services.Search;
using IdeaBranch.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for simple inline search panel.
/// </summary>
public class SimpleSearchViewModel : INotifyPropertyChanged
{
    private readonly SearchCoordinator _searchCoordinator;
    private bool _isExpanded;
    private bool _isLoading;
    private string? _searchText;
    private bool _searchTopicNodes = true;
    private bool _searchAnnotations = true;
    private bool _searchTags;
    private bool _searchTemplates;
    private DateTime? _updatedAtFrom;
    private SearchResults? _results;
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance with the search coordinator.
    /// </summary>
    public SimpleSearchViewModel(SearchCoordinator searchCoordinator)
    {
        _searchCoordinator = searchCoordinator ?? throw new ArgumentNullException(nameof(searchCoordinator));
        ExecuteSearchCommand = new Command(async () => await ExecuteSearchAsync(CancellationToken.None), () => !IsLoading);
    }

    /// <summary>
    /// Initializes a new instance from dependency injection (parameterless constructor for XAML).
    /// </summary>
    public SimpleSearchViewModel() : this(GetSearchCoordinatorFromServices())
    {
    }

    private static SearchCoordinator GetSearchCoordinatorFromServices()
    {
        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Services not available");
        return services.GetRequiredService<SearchCoordinator>();
    }

    /// <summary>
    /// Gets or sets whether the panel is expanded.
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
    /// Gets or sets the search text.
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
    /// Gets or sets whether to search topic nodes.
    /// </summary>
    public bool SearchTopicNodes
    {
        get => _searchTopicNodes;
        set
        {
            if (_searchTopicNodes != value)
            {
                _searchTopicNodes = value;
                OnPropertyChanged(nameof(SearchTopicNodes));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to search annotations.
    /// </summary>
    public bool SearchAnnotations
    {
        get => _searchAnnotations;
        set
        {
            if (_searchAnnotations != value)
            {
                _searchAnnotations = value;
                OnPropertyChanged(nameof(SearchAnnotations));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to search tags.
    /// </summary>
    public bool SearchTags
    {
        get => _searchTags;
        set
        {
            if (_searchTags != value)
            {
                _searchTags = value;
                OnPropertyChanged(nameof(SearchTags));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to search templates.
    /// </summary>
    public bool SearchTemplates
    {
        get => _searchTemplates;
        set
        {
            if (_searchTemplates != value)
            {
                _searchTemplates = value;
                OnPropertyChanged(nameof(SearchTemplates));
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
                OnPropertyChanged(nameof(ResultSummary));
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
    /// Gets a summary of the search results.
    /// </summary>
    public string? ResultSummary
    {
        get
        {
            if (Results == null || !HasResults)
                return null;

            var parts = new List<string>();
            if (Results.Nodes.Count > 0)
                parts.Add($"{Results.Nodes.Count} topic(s)");
            if (Results.Annotations.Count > 0)
                parts.Add($"{Results.Annotations.Count} annotation(s)");
            if (Results.Tags.Count > 0)
                parts.Add($"{Results.Tags.Count} tag(s)");
            if (Results.Templates.Count > 0)
                parts.Add($"{Results.Templates.Count} template(s)");

            return parts.Count > 0 ? string.Join(", ", parts) : null;
        }
    }

    /// <summary>
    /// Gets or sets any error message from the last search.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    /// <summary>
    /// Gets whether there is an error message.
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Gets the command to execute the search.
    /// </summary>
    public Command ExecuteSearchCommand { get; }

    /// <summary>
    /// Checks if any content type is selected.
    /// </summary>
    public bool HasAnyContentTypeSelected()
    {
        return SearchTopicNodes || SearchAnnotations || SearchTags || SearchTemplates;
    }

    /// <summary>
    /// Executes the search with current filter settings.
    /// </summary>
    public async Task ExecuteSearchAsync(CancellationToken cancellationToken = default)
    {
        if (!HasAnyContentTypeSelected())
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var contentTypes = new HashSet<SearchContentType>();
            if (SearchTopicNodes)
                contentTypes.Add(SearchContentType.TopicNodes);
            if (SearchAnnotations)
                contentTypes.Add(SearchContentType.Annotations);
            if (SearchTags)
                contentTypes.Add(SearchContentType.Tags);
            if (SearchTemplates)
                contentTypes.Add(SearchContentType.PromptTemplates);

            var request = new SearchRequest
            {
                ContentTypes = contentTypes,
                TextContains = SearchText,
                UpdatedAtFrom = UpdatedAtFrom
            };

            Results = await _searchCoordinator.SearchAsync(request, cancellationToken);
            OnPropertyChanged(nameof(ResultSummary));
        }
        catch (OperationCanceledException)
        {
            // Search was cancelled, ignore
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Search failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
            Results = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

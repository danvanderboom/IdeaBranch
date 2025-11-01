using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Services;
using IdeaBranch.Domain;
using IdeaBranch.Domain.Timeline;
using IdeaBranch.Infrastructure.Export;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SkiaSharp;

namespace IdeaBranch.App.ViewModels.Analytics;

/// <summary>
/// ViewModel for timeline analytics page.
/// </summary>
public class TimelineViewModel : INotifyPropertyChanged
{
    private readonly IAnalyticsService _analyticsService;
    private readonly AnalyticsExportService _exportService;
    private readonly ITagTaxonomyRepository _tagTaxonomyRepository;
    private readonly SettingsService _settingsService;
    private TimelineData? _timelineData;
    private bool _isLoading;
    private string? _errorMessage;
    private bool _includeTopics = true;
    private bool _includeAnnotations = true;
    private bool _includeConversations = true;
    private bool _includeTagDescendants = true;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private TimelineGrouping _grouping = TimelineGrouping.Day;
    private bool _showCreated = true;
    private bool _showUpdated = true;
    private string? _searchQuery;
    private string? _activeDatePreset;
    private TimelineEventView? _selectedEvent;
    private bool _groupByType = false;
    private Dictionary<string, int>? _eventTypeCounts;
    private Dictionary<string, List<(DateTime Date, int Count)>>? _eventTypeTrends;
    private string? _highlightedEventType;
    private bool _isExporting = false;

    /// <summary>
    /// Initializes a new instance with required services.
    /// </summary>
    public TimelineViewModel(
        IAnalyticsService analyticsService,
        AnalyticsExportService exportService,
        ITagTaxonomyRepository tagTaxonomyRepository,
        SettingsService settingsService)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _tagTaxonomyRepository = tagTaxonomyRepository ?? throw new ArgumentNullException(nameof(tagTaxonomyRepository));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        SelectedTags = new ObservableCollection<TagTaxonomyNode>();
        SelectedTagSelections = new ObservableCollection<TagSelection>();
        GenerateCommand = new Command(async () => await GenerateTimelineAsync(), () => !IsLoading);
        ExportJsonCommand = new Command(async () => await ExportJsonAsync(), () => TimelineData != null);
        ExportCsvCommand = new Command(async () => await ExportCsvAsync(), () => TimelineData != null);
        ExportPngCommand = new Command(async () => await ExportPngAsync(), () => TimelineData != null);
        ExportSvgCommand = new Command(async () => await ExportSvgAsync(), () => TimelineData != null);
        ApplyLast7DaysCommand = new Command(() => ApplyLast7Days());
        ApplyThisMonthCommand = new Command(() => ApplyThisMonth());
        ApplyThisYearCommand = new Command(() => ApplyThisYear());
        NavigateToNodeCommand = new Command<Guid?>(async (nodeId) => await NavigateToNodeAsync(nodeId));
        NavigateToAnnotationCommand = new Command<Guid?>(async (nodeId) => await NavigateToAnnotationAsync(nodeId));
        CloseEventDetailsCommand = new Command(() => SelectedEvent = null);
        ToggleGroupByTypeCommand = new Command(() => GroupByType = !GroupByType);
        HighlightEventTypeCommand = new Command<string?>(type => HighlightedEventType = type == HighlightedEventType ? null : type);
    }

    /// <summary>
    /// Initializes a new instance from dependency injection (parameterless constructor for XAML).
    /// </summary>
    public TimelineViewModel() : this(
        GetService<IAnalyticsService>(),
        GetService<AnalyticsExportService>(),
        GetService<ITagTaxonomyRepository>(),
        GetService<SettingsService>())
    {
    }

    private static T GetService<T>() where T : notnull
    {
        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Services not available");
        return services.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets or sets the timeline data.
    /// </summary>
    public TimelineData? TimelineData
    {
        get => _timelineData;
        set
        {
            if (_timelineData != value)
            {
                _timelineData = value;
                OnPropertyChanged(nameof(TimelineData));
                OnPropertyChanged(nameof(HasData));
                OnPropertyChanged(nameof(EventCount));
                OnPropertyChanged(nameof(TimelineEventViews));
                GenerateCommand.ChangeCanExecute();
                ExportJsonCommand.ChangeCanExecute();
                ExportCsvCommand.ChangeCanExecute();
                ExportPngCommand.ChangeCanExecute();
                ExportSvgCommand.ChangeCanExecute();
                UpdateStatistics();
            }
        }
    }

    /// <summary>
    /// Gets whether there is timeline data.
    /// </summary>
    public bool HasData => TimelineData != null && TimelineData.Events.Count > 0;

    /// <summary>
    /// Gets the event count.
    /// </summary>
    public int EventCount => TimelineData?.Events.Count ?? 0;

    /// <summary>
    /// Gets the timeline events as TimelineEventView collection for SkiaSharp rendering.
    /// </summary>
    public ObservableCollection<TimelineEventView> TimelineEventViews
    {
        get
        {
            var views = new ObservableCollection<TimelineEventView>();
            if (TimelineData?.Events != null)
            {
                foreach (var evt in TimelineData.Events)
                {
                    views.Add(TimelineEventView.FromDomainEvent(evt));
                }
            }
            return views;
        }
    }

    /// <summary>
    /// Gets or sets whether a generation is in progress.
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
                GenerateCommand.ChangeCanExecute();
            }
        }
    }

    /// <summary>
    /// Gets or sets any error message.
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
    /// Gets or sets whether to include topics.
    /// </summary>
    public bool IncludeTopics
    {
        get => _includeTopics;
        set
        {
            if (_includeTopics != value)
            {
                _includeTopics = value;
                OnPropertyChanged(nameof(IncludeTopics));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to include annotations.
    /// </summary>
    public bool IncludeAnnotations
    {
        get => _includeAnnotations;
        set
        {
            if (_includeAnnotations != value)
            {
                _includeAnnotations = value;
                OnPropertyChanged(nameof(IncludeAnnotations));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to include conversations.
    /// </summary>
    public bool IncludeConversations
    {
        get => _includeConversations;
        set
        {
            if (_includeConversations != value)
            {
                _includeConversations = value;
                OnPropertyChanged(nameof(IncludeConversations));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to include tag descendants in filtering.
    /// </summary>
    public bool IncludeTagDescendants
    {
        get => _includeTagDescendants;
        set
        {
            if (_includeTagDescendants != value)
            {
                _includeTagDescendants = value;
                OnPropertyChanged(nameof(IncludeTagDescendants));
            }
        }
    }

    /// <summary>
    /// Gets or sets the start date filter.
    /// </summary>
    public DateTime? StartDate
    {
        get => _startDate;
        set
        {
            if (_startDate != value)
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                // Clear preset if manually adjusted
                if (!string.IsNullOrEmpty(ActiveDatePreset))
                {
                    ActiveDatePreset = null;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the end date filter.
    /// </summary>
    public DateTime? EndDate
    {
        get => _endDate;
        set
        {
            if (_endDate != value)
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
                // Clear preset if manually adjusted
                if (!string.IsNullOrEmpty(ActiveDatePreset))
                {
                    ActiveDatePreset = null;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the timeline grouping level.
    /// </summary>
    public TimelineGrouping Grouping
    {
        get => _grouping;
        set
        {
            if (_grouping != value)
            {
                _grouping = value;
                OnPropertyChanged(nameof(Grouping));
            }
        }
    }

    /// <summary>
    /// Gets the selected tags for filtering.
    /// </summary>
    [Obsolete("Use SelectedTagSelections instead for per-tag descendant control.")]
    public ObservableCollection<TagTaxonomyNode> SelectedTags { get; }

    /// <summary>
    /// Gets the selected tag selections with per-tag descendant control.
    /// </summary>
    public ObservableCollection<TagSelection> SelectedTagSelections { get; }

    /// <summary>
    /// Gets or sets whether to show Created event types.
    /// </summary>
    public bool ShowCreated
    {
        get => _showCreated;
        set
        {
            if (_showCreated != value)
            {
                _showCreated = value;
                OnPropertyChanged(nameof(ShowCreated));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to show Updated event types.
    /// </summary>
    public bool ShowUpdated
    {
        get => _showUpdated;
        set
        {
            if (_showUpdated != value)
            {
                _showUpdated = value;
                OnPropertyChanged(nameof(ShowUpdated));
            }
        }
    }

    /// <summary>
    /// Gets or sets the search query for free-text filtering.
    /// </summary>
    public string? SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery != value)
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
            }
        }
    }

    /// <summary>
    /// Gets or sets the active date preset label (Last 7 days, This month, This year).
    /// </summary>
    public string? ActiveDatePreset
    {
        get => _activeDatePreset;
        private set
        {
            if (_activeDatePreset != value)
            {
                _activeDatePreset = value;
                OnPropertyChanged(nameof(ActiveDatePreset));
            }
        }
    }

    /// <summary>
    /// Gets the command to generate the timeline.
    /// </summary>
    public Command GenerateCommand { get; }

    /// <summary>
    /// Gets the command to export as JSON.
    /// </summary>
    public Command ExportJsonCommand { get; }

    /// <summary>
    /// Gets the command to export as CSV.
    /// </summary>
    public Command ExportCsvCommand { get; }

    /// <summary>
    /// Gets the command to export as PNG.
    /// </summary>
    public Command ExportPngCommand { get; }

    /// <summary>
    /// Gets the command to export as SVG.
    /// </summary>
    public Command ExportSvgCommand { get; }

    /// <summary>
    /// Gets the command to apply "Last 7 days" date preset.
    /// </summary>
    public Command ApplyLast7DaysCommand { get; }

    /// <summary>
    /// Gets the command to apply "This month" date preset.
    /// </summary>
    public Command ApplyThisMonthCommand { get; }

    /// <summary>
    /// Gets the command to apply "This year" date preset.
    /// </summary>
    public Command ApplyThisYearCommand { get; }

    /// <summary>
    /// Gets or sets the selected event.
    /// </summary>
    public TimelineEventView? SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            if (_selectedEvent != value)
            {
                _selectedEvent = value;
                OnPropertyChanged(nameof(SelectedEvent));
                OnPropertyChanged(nameof(HasSelectedEvent));
            }
        }
    }

    /// <summary>
    /// Gets whether an event is selected.
    /// </summary>
    public bool HasSelectedEvent => SelectedEvent != null;

    /// <summary>
    /// Gets or sets whether to group events by type.
    /// </summary>
    public bool GroupByType
    {
        get => _groupByType;
        set
        {
            if (_groupByType != value)
            {
                _groupByType = value;
                OnPropertyChanged(nameof(GroupByType));
                OnPropertyChanged(nameof(TimelineEventViews));
            }
        }
    }

    /// <summary>
    /// Gets the event type counts for statistics.
    /// </summary>
    public Dictionary<string, int>? EventTypeCounts
    {
        get => _eventTypeCounts;
        private set
        {
            if (_eventTypeCounts != value)
            {
                _eventTypeCounts = value;
                OnPropertyChanged(nameof(EventTypeCounts));
                OnPropertyChanged(nameof(EventTypeCountsString));
                OnPropertyChanged(nameof(EventTypeTrendsString));
            }
        }
    }

    /// <summary>
    /// Gets the event type counts as a formatted string.
    /// </summary>
    public string EventTypeCountsString
    {
        get
        {
            if (EventTypeCounts == null || EventTypeCounts.Count == 0)
                return "No events";

            return string.Join(Environment.NewLine, EventTypeCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        }
    }

    /// <summary>
    /// Gets the event type trends as a formatted string.
    /// </summary>
    public string EventTypeTrendsString
    {
        get
        {
            if (EventTypeTrends == null || EventTypeTrends.Count == 0)
                return "No trends available";

            var trendLines = new List<string>();
            foreach (var kvp in EventTypeTrends.OrderBy(k => k.Key))
            {
                var trendData = kvp.Value.OrderBy(t => t.Date).Take(10); // Show last 10 data points
                var trendValues = trendData.Select(t => t.Count).ToArray();
                var maxValue = trendValues.Any() ? trendValues.Max() : 0;
                var sparkline = maxValue > 0
                    ? string.Join(" ", trendValues.Select(v => GetSparklineChar(v, maxValue)))
                    : "─";
                
                trendLines.Add($"{kvp.Key}: {sparkline} (max: {maxValue})");
            }

            return string.Join(Environment.NewLine, trendLines);
        }
    }

    /// <summary>
    /// Gets a sparkline character for a value.
    /// </summary>
    private static string GetSparklineChar(int value, int maxValue)
    {
        if (maxValue == 0) return "─";
        
        var normalized = (double)value / maxValue;
        if (normalized < 0.125) return "▁";
        if (normalized < 0.25) return "▂";
        if (normalized < 0.375) return "▃";
        if (normalized < 0.5) return "▄";
        if (normalized < 0.625) return "▅";
        if (normalized < 0.75) return "▆";
        if (normalized < 0.875) return "▇";
        return "█";
    }

    /// <summary>
    /// Gets the event type trends for statistics.
    /// </summary>
    public Dictionary<string, List<(DateTime Date, int Count)>>? EventTypeTrends
    {
        get => _eventTypeTrends;
        private set
        {
            if (_eventTypeTrends != value)
            {
                _eventTypeTrends = value;
                OnPropertyChanged(nameof(EventTypeTrends));
                OnPropertyChanged(nameof(EventTypeTrendsString));
            }
        }
    }

    /// <summary>
    /// Gets the command to navigate to a node.
    /// </summary>
    public Command<Guid?> NavigateToNodeCommand { get; }

    /// <summary>
    /// Gets the command to navigate to an annotation.
    /// </summary>
    public Command<Guid?> NavigateToAnnotationCommand { get; }

    /// <summary>
    /// Gets the command to close event details.
    /// </summary>
    public Command CloseEventDetailsCommand { get; }

    /// <summary>
    /// Gets the command to toggle grouping by type.
    /// </summary>
    public Command ToggleGroupByTypeCommand { get; }

    /// <summary>
    /// Gets or sets the highlighted event type.
    /// </summary>
    public string? HighlightedEventType
    {
        get => _highlightedEventType;
        set
        {
            if (_highlightedEventType != value)
            {
                _highlightedEventType = value;
                OnPropertyChanged(nameof(HighlightedEventType));
                OnPropertyChanged(nameof(TimelineEventViews));
            }
        }
    }

    /// <summary>
    /// Gets the command to highlight an event type.
    /// </summary>
    public Command<string?> HighlightEventTypeCommand { get; }

    /// <summary>
    /// Gets or sets whether an export is in progress.
    /// </summary>
    public bool IsExporting
    {
        get => _isExporting;
        private set
        {
            if (_isExporting != value)
            {
                _isExporting = value;
                OnPropertyChanged(nameof(IsExporting));
            }
        }
    }

    /// <summary>
    /// Generates the timeline.
    /// </summary>
    public async Task GenerateTimelineAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var sourceTypes = new HashSet<EventSourceType>();
            if (IncludeTopics)
                sourceTypes.Add(EventSourceType.Topics);
            if (IncludeAnnotations)
                sourceTypes.Add(EventSourceType.Annotations);
            if (IncludeConversations)
                sourceTypes.Add(EventSourceType.Conversations);

            if (sourceTypes.Count == 0)
            {
                ErrorMessage = "Please select at least one source type.";
                return;
            }

            // Build event types filter (Created/Updated mapping)
            HashSet<TimelineEventType>? eventTypes = null;
            if (!ShowCreated && !ShowUpdated)
            {
                // Both unchecked means no events (empty set)
                eventTypes = new HashSet<TimelineEventType>();
            }
            else if (!ShowCreated || !ShowUpdated)
            {
                // Only one checked
                eventTypes = new HashSet<TimelineEventType>();
                if (ShowCreated)
                {
                    eventTypes.Add(TimelineEventType.TopicCreated);
                    eventTypes.Add(TimelineEventType.AnnotationCreated);
                    eventTypes.Add(TimelineEventType.ConversationMessage);
                }
                if (ShowUpdated)
                {
                    eventTypes.Add(TimelineEventType.TopicUpdated);
                    eventTypes.Add(TimelineEventType.AnnotationUpdated);
                }
            }
            // If both checked (default), leave null to include all event types

            var options = new TimelineOptions
            {
                SourceTypes = sourceTypes,
                TagSelections = SelectedTagSelections.Count > 0 ? SelectedTagSelections.ToList() : null,
                EventTypes = eventTypes,
                SearchQuery = string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length < 2 ? null : SearchQuery,
                StartDate = StartDate,
                EndDate = EndDate,
                Grouping = Grouping
            };

            TimelineData = await _analyticsService.GenerateTimelineAsync(options, cancellationToken);
            UpdateStatistics();
        }
        catch (OperationCanceledException)
        {
            // Generation was cancelled, ignore
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Generation failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Timeline generation error: {ex.Message}");
            TimelineData = null;
            UpdateStatistics();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Updates statistics from current timeline data.
    /// </summary>
    private void UpdateStatistics()
    {
        if (TimelineData == null || TimelineData.Events.Count == 0)
        {
            EventTypeCounts = null;
            EventTypeTrends = null;
            return;
        }

        // Calculate per-type counts
        var counts = new Dictionary<string, int>();
        var trends = new Dictionary<string, List<(DateTime Date, int Count)>>();

        // Determine auto-binning based on visible date range
        var visibleStart = StartDate ?? TimelineData.Events.Min(e => e.Timestamp);
        var visibleEnd = EndDate ?? TimelineData.Events.Max(e => e.Timestamp);
        var timeSpan = visibleEnd - visibleStart;
        
        // Auto-select binning: day for < 3 months, week for < 2 years, month otherwise
        bool binByWeek = timeSpan.TotalDays >= 90 && timeSpan.TotalDays < 730;
        bool binByMonth = timeSpan.TotalDays >= 730;

        foreach (var evt in TimelineData.Events)
        {
            var typeName = evt.EventType.ToString();
            counts.TryGetValue(typeName, out var count);
            counts[typeName] = count + 1;

            // Group by date for trends (auto-binned by day/week/month)
            DateTime binDate;
            if (binByMonth)
            {
                // Bin by month (first day of month)
                binDate = new DateTime(evt.Timestamp.Year, evt.Timestamp.Month, 1);
            }
            else if (binByWeek)
            {
                // Bin by week (Monday of the week)
                var dayOfWeek = (int)evt.Timestamp.DayOfWeek;
                var monday = evt.Timestamp.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
                binDate = monday.Date;
            }
            else
            {
                // Bin by day
                binDate = evt.Timestamp.Date;
            }

            if (!trends.ContainsKey(typeName))
            {
                trends[typeName] = new List<(DateTime Date, int Count)>();
            }

            var trendList = trends[typeName];
            var existingEntry = trendList.FirstOrDefault(e => e.Date == binDate);
            if (existingEntry.Date == default)
            {
                trendList.Add((binDate, 1));
            }
            else
            {
                var index = trendList.IndexOf(existingEntry);
                trendList[index] = (binDate, existingEntry.Count + 1);
            }
        }

        // Sort trends by date
        foreach (var key in trends.Keys.ToList())
        {
            trends[key] = trends[key].OrderBy(e => e.Date).ToList();
        }

        EventTypeCounts = counts;
        EventTypeTrends = trends;
    }

    /// <summary>
    /// Navigates to a topic node.
    /// </summary>
    private async Task NavigateToNodeAsync(Guid? nodeId)
    {
        if (!nodeId.HasValue)
            return;

        try
        {
            // Get TopicTreeViewModel from services
            var services = Application.Current?.Handler?.MauiContext?.Services;
            var topicTreeViewModel = services?.GetService<IdeaBranch.App.ViewModels.TopicTreeViewModel>();
            
            if (topicTreeViewModel == null)
            {
                ErrorMessage = "Topic tree view model not available.";
                return;
            }

            // Find ITreeNode by domain node ID in the projected collection
            ITreeNode? treeNode = null;
            foreach (var node in topicTreeViewModel.ProjectedCollection)
            {
                var payload = IdeaBranch.App.ViewModels.TopicTreeViewModel.GetPayload(node);
                if (payload?.DomainNodeId == nodeId.Value)
                {
                    treeNode = node;
                    break;
                }
            }

            if (treeNode != null)
            {
                await topicTreeViewModel.EditNodeAsync(treeNode);
            }
            else
            {
                ErrorMessage = $"Node with ID {nodeId} not found.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Navigation failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates to an annotation for annotation events.
    /// </summary>
    private async Task NavigateToAnnotationAsync(Guid? nodeId)
    {
        if (!nodeId.HasValue || SelectedEvent == null || !SelectedEvent.IsAnnotationEvent)
            return;

        try
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;
            var annotationsRepository = services?.GetService<IAnnotationsRepository>();
            var tagTaxonomyRepository = services?.GetService<ITagTaxonomyRepository>();
            
            if (annotationsRepository == null)
            {
                ErrorMessage = "Annotations repository not available.";
                return;
            }

            // Get annotations for this node
            var annotations = await annotationsRepository.GetByNodeIdAsync(nodeId.Value);
            
            if (annotations.Count == 0)
            {
                // No annotations found, navigate to node instead
                await NavigateToNodeAsync(nodeId);
                return;
            }

            // Try to find annotation that matches the event timestamp/details
            Annotation? matchingAnnotation = null;
            var eventTimestamp = SelectedEvent.When.Start.Date; // TemporalInstant.Date is DateTime
            var eventDetails = SelectedEvent.Details;

            // First try to match by timestamp (within 1 second)
            matchingAnnotation = annotations.FirstOrDefault(a => 
                Math.Abs((a.CreatedAt - eventTimestamp).TotalSeconds) < 1 ||
                Math.Abs((a.UpdatedAt - eventTimestamp).TotalSeconds) < 1);

            // If no timestamp match, try to match by comment/details
            if (matchingAnnotation == null && !string.IsNullOrEmpty(eventDetails))
            {
                matchingAnnotation = annotations.FirstOrDefault(a => 
                    !string.IsNullOrEmpty(a.Comment) && 
                    a.Comment.Contains(eventDetails, StringComparison.OrdinalIgnoreCase));
            }

            // If still no match, use the first annotation
            matchingAnnotation ??= annotations[0];

            // Get topic node to access response text
            var topicTreeViewModel = services?.GetService<IdeaBranch.App.ViewModels.TopicTreeViewModel>();
            if (topicTreeViewModel == null)
            {
                ErrorMessage = "Topic tree view model not available.";
                return;
            }

            // Find ITreeNode by domain node ID in the projected collection
            ITreeNode? treeNode = null;
            foreach (var node in topicTreeViewModel.ProjectedCollection)
            {
                var payload = IdeaBranch.App.ViewModels.TopicTreeViewModel.GetPayload(node);
                if (payload?.DomainNodeId == nodeId.Value)
                {
                    treeNode = node;
                    break;
                }
            }

            if (treeNode == null)
            {
                ErrorMessage = $"Node with ID {nodeId} not found.";
                return;
            }

            // Get domain node from ITreeNode
            var domainNode = topicTreeViewModel.GetDomainNode(treeNode);
            if (domainNode == null)
            {
                ErrorMessage = $"Node with ID {nodeId} not found.";
                return;
            }

            // Get selected text from node response
            var response = domainNode.Response ?? string.Empty;
            var startOffset = Math.Min(matchingAnnotation.StartOffset, response.Length);
            var endOffset = Math.Min(matchingAnnotation.EndOffset, response.Length);
            var selectedText = startOffset < endOffset && endOffset <= response.Length
                ? response.Substring(startOffset, endOffset - startOffset)
                : string.Empty;

            // Create annotation edit view model and navigate
            var annotationViewModel = new ViewModels.AnnotationEditViewModel(
                matchingAnnotation,
                selectedText,
                annotationsRepository,
                tagTaxonomyRepository);

            var annotationPage = new Views.AnnotationEditPage(annotationViewModel);
            
            // Get current page to navigate from
            var window = Application.Current?.Windows.FirstOrDefault();
            var currentPage = window?.Page;
            if (currentPage != null)
            {
                await currentPage.Navigation.PushAsync(annotationPage);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Navigation to annotation failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Annotation navigation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Exports timeline data to JSON with all required fields.
    /// </summary>
    public async Task ExportJsonAsync()
    {
        if (TimelineData == null)
            return;

        try
        {
            IsExporting = true;
            ErrorMessage = null;

            var json = await _exportService.ExportTimelineToJsonAsync(TimelineData, includeAllFields: true);
            
            var fileName = $"timeline_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllTextAsync(filePath, json);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Timeline",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
        }
        finally
        {
            IsExporting = false;
        }
    }

    /// <summary>
    /// Exports timeline data to CSV with all required fields.
    /// </summary>
    public async Task ExportCsvAsync()
    {
        if (TimelineData == null)
            return;

        try
        {
            IsExporting = true;
            ErrorMessage = null;

            var csv = await _exportService.ExportTimelineToCsvAsync(TimelineData, includeAllFields: true);
            
            var fileName = $"timeline_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllTextAsync(filePath, csv);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Timeline",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
        }
        finally
        {
            IsExporting = false;
        }
    }

    /// <summary>
    /// Exports timeline visualization to PNG.
    /// </summary>
    public async Task ExportPngAsync()
    {
        if (TimelineData == null)
            return;

        try
        {
            IsExporting = true;
            ErrorMessage = null;

            var (exportOptions, theme, connections, includeStatistics) = await BuildExportOptionsAsync();
            var pngBytes = await _exportService.ExportTimelineToPngAsync(
                TimelineData,
                exportOptions.Width,
                exportOptions.Height,
                exportOptions,
                theme,
                connections,
                includeStatistics);
            
            var fileName = $"timeline_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, pngBytes);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Timeline",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
        }
        finally
        {
            IsExporting = false;
        }
    }

    /// <summary>
    /// Exports timeline visualization to SVG.
    /// </summary>
    public async Task ExportSvgAsync()
    {
        if (TimelineData == null)
            return;

        try
        {
            IsExporting = true;
            ErrorMessage = null;

            var (exportOptions, theme, connections, includeStatistics) = await BuildExportOptionsAsync();
            var svg = await _exportService.ExportTimelineToSvgAsync(
                TimelineData,
                exportOptions.Width,
                exportOptions.Height,
                exportOptions,
                theme,
                connections,
                includeStatistics);
            
            var fileName = $"timeline_{DateTime.UtcNow:yyyyMMddHHmmss}.svg";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllTextAsync(filePath, svg);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Timeline",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
        }
        finally
        {
            IsExporting = false;
        }
    }

    /// <summary>
    /// Builds export options and theme from settings.
    /// </summary>
    private async Task<(ExportOptions options, VisualizationTheme theme, IReadOnlyList<(Guid fromEventId, Guid toEventId)>? connections, bool includeStatistics)> BuildExportOptionsAsync()
    {
        var dpiScale = await _settingsService.GetExportDpiScaleAsync();
        var backgroundColorHex = await _settingsService.GetExportBackgroundColorAsync();
        var transparentBackground = await _settingsService.GetExportTransparentBackgroundAsync();
        var fontFamily = await _settingsService.GetExportFontFamilyAsync();
        var palette = await _settingsService.GetExportPaletteAsync();
        var includeLegend = await _settingsService.GetExportIncludeLegendAsync();

        // Get timeline-specific settings
        var includeStatisticsValue = await SecureStorage.GetAsync("timeline_include_statistics");
        var includeStatistics = bool.TryParse(includeStatisticsValue, out var stats) && stats;

        var includeConnectionsValue = await SecureStorage.GetAsync("timeline_include_connections");
        var includeConnections = bool.TryParse(includeConnectionsValue, out var conn) && conn;

        var exportOptions = new ExportOptions
        {
            DpiScale = dpiScale,
            Width = 1200,
            Height = 600,
            IncludeLegend = includeLegend,
            IncludeTimelineConnections = includeConnections,
            IncludeTimelineStatistics = includeStatistics
        };

        if (!transparentBackground && !string.IsNullOrEmpty(backgroundColorHex))
        {
            if (SKColor.TryParse(backgroundColorHex, out var bgColor))
            {
                exportOptions.BackgroundColor = bgColor;
            }
        }

        var theme = new VisualizationTheme
        {
            ColorPalette = palette,
            FontFamily = fontFamily,
            BackgroundType = transparentBackground ? BackgroundType.Transparent : BackgroundType.Solid
        };

        if (!transparentBackground && !string.IsNullOrEmpty(backgroundColorHex))
        {
            if (SKColor.TryParse(backgroundColorHex, out var bgColor))
            {
                theme.BackgroundColor = bgColor;
            }
        }

        // Build connections list from TimelineData if enabled
        IReadOnlyList<(Guid fromEventId, Guid toEventId)>? connections = null;
        if (includeConnections && TimelineData != null)
        {
            // Simple heuristic: connect events with same NodeId
            var connectionsList = new List<(Guid, Guid)>();
            var eventsByNode = TimelineData.Events.Where(e => e.NodeId.HasValue)
                .GroupBy(e => e.NodeId!.Value)
                .ToList();

            foreach (var nodeGroup in eventsByNode)
            {
                var nodeEvents = nodeGroup.OrderBy(e => e.Timestamp).ToList();
                for (int i = 0; i < nodeEvents.Count - 1; i++)
                {
                    connectionsList.Add((nodeEvents[i].Id, nodeEvents[i + 1].Id));
                }
            }

            connections = connectionsList.Count > 0 ? connectionsList : null;
        }

        return (exportOptions, theme, connections, includeStatistics);
    }

    /// <summary>
    /// Applies "Last 7 days" date preset.
    /// </summary>
    private void ApplyLast7Days()
    {
        var now = DateTime.Now;
        EndDate = now;
        StartDate = now.AddDays(-7);
        ActiveDatePreset = "Last 7 days";
    }

    /// <summary>
    /// Applies "This month" date preset.
    /// </summary>
    private void ApplyThisMonth()
    {
        var now = DateTime.Now;
        EndDate = now;
        StartDate = new DateTime(now.Year, now.Month, 1);
        ActiveDatePreset = "This month";
    }

    /// <summary>
    /// Applies "This year" date preset.
    /// </summary>
    private void ApplyThisYear()
    {
        var now = DateTime.Now;
        EndDate = now;
        StartDate = new DateTime(now.Year, 1, 1);
        ActiveDatePreset = "This year";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


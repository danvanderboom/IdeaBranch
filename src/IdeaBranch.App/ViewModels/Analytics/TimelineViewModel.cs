using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using IdeaBranch.Domain.Timeline;
using IdeaBranch.Infrastructure.Export;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace IdeaBranch.App.ViewModels.Analytics;

/// <summary>
/// ViewModel for timeline analytics page.
/// </summary>
public class TimelineViewModel : INotifyPropertyChanged
{
    private readonly IAnalyticsService _analyticsService;
    private readonly AnalyticsExportService _exportService;
    private readonly ITagTaxonomyRepository _tagTaxonomyRepository;
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

    /// <summary>
    /// Initializes a new instance with required services.
    /// </summary>
    public TimelineViewModel(
        IAnalyticsService analyticsService,
        AnalyticsExportService exportService,
        ITagTaxonomyRepository tagTaxonomyRepository)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _tagTaxonomyRepository = tagTaxonomyRepository ?? throw new ArgumentNullException(nameof(tagTaxonomyRepository));

        SelectedTags = new ObservableCollection<TagTaxonomyNode>();
        GenerateCommand = new Command(async () => await GenerateTimelineAsync(), () => !IsLoading);
        ExportJsonCommand = new Command(async () => await ExportJsonAsync(), () => TimelineData != null);
        ExportCsvCommand = new Command(async () => await ExportCsvAsync(), () => TimelineData != null);
        ExportPngCommand = new Command(async () => await ExportPngAsync(), () => TimelineData != null);
    }

    /// <summary>
    /// Initializes a new instance from dependency injection (parameterless constructor for XAML).
    /// </summary>
    public TimelineViewModel() : this(
        GetService<IAnalyticsService>(),
        GetService<AnalyticsExportService>(),
        GetService<ITagTaxonomyRepository>())
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
    public ObservableCollection<TagTaxonomyNode> SelectedTags { get; }

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

            var options = new TimelineOptions
            {
                SourceTypes = sourceTypes,
                TagIds = SelectedTags.Select(t => t.Id).ToList(),
                IncludeTagDescendants = IncludeTagDescendants,
                StartDate = StartDate,
                EndDate = EndDate,
                Grouping = Grouping
            };

            TimelineData = await _analyticsService.GenerateTimelineAsync(options, cancellationToken);
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
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Exports timeline data to JSON.
    /// </summary>
    public async Task ExportJsonAsync()
    {
        if (TimelineData == null)
            return;

        try
        {
            var json = await _exportService.ExportTimelineToJsonAsync(TimelineData);
            
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
    }

    /// <summary>
    /// Exports timeline data to CSV.
    /// </summary>
    public async Task ExportCsvAsync()
    {
        if (TimelineData == null)
            return;

        try
        {
            var csv = await _exportService.ExportTimelineToCsvAsync(TimelineData);
            
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
            var pngBytes = await _exportService.ExportTimelineToPngAsync(TimelineData);
            
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
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


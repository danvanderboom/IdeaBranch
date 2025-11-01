using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Export;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace IdeaBranch.App.ViewModels.Analytics;

/// <summary>
/// ViewModel for word cloud analytics page.
/// </summary>
public class WordCloudViewModel : INotifyPropertyChanged
{
    private readonly IAnalyticsService _analyticsService;
    private readonly AnalyticsExportService _exportService;
    private readonly ITagTaxonomyRepository _tagTaxonomyRepository;
    private WordCloudData? _wordCloudData;
    private bool _isLoading;
    private string? _errorMessage;
    private bool _includePrompts = true;
    private bool _includeResponses = true;
    private bool _includeAnnotations;
    private bool _includeTopics;
    private bool _includeTagDescendants = true;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private int _minFrequency = 1;
    private int? _maxWords = 100;

    /// <summary>
    /// Initializes a new instance with required services.
    /// </summary>
    public WordCloudViewModel(
        IAnalyticsService analyticsService,
        AnalyticsExportService exportService,
        ITagTaxonomyRepository tagTaxonomyRepository)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _tagTaxonomyRepository = tagTaxonomyRepository ?? throw new ArgumentNullException(nameof(tagTaxonomyRepository));

        SelectedTags = new ObservableCollection<TagTaxonomyNode>();
        GenerateCommand = new Command(async () => await GenerateWordCloudAsync(), () => !IsLoading);
        ExportJsonCommand = new Command(async () => await ExportJsonAsync(), () => WordCloudData != null);
        ExportCsvCommand = new Command(async () => await ExportCsvAsync(), () => WordCloudData != null);
        ExportPngCommand = new Command(async () => await ExportPngAsync(), () => WordCloudData != null);
    }

    /// <summary>
    /// Initializes a new instance from dependency injection (parameterless constructor for XAML).
    /// </summary>
    public WordCloudViewModel() : this(
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
    /// Gets or sets the word cloud data.
    /// </summary>
    public WordCloudData? WordCloudData
    {
        get => _wordCloudData;
        set
        {
            if (_wordCloudData != value)
            {
                _wordCloudData = value;
                OnPropertyChanged(nameof(WordCloudData));
                OnPropertyChanged(nameof(HasData));
                OnPropertyChanged(nameof(WordCount));
                GenerateCommand.ChangeCanExecute();
                ExportJsonCommand.ChangeCanExecute();
                ExportCsvCommand.ChangeCanExecute();
                ExportPngCommand.ChangeCanExecute();
            }
        }
    }

    /// <summary>
    /// Gets whether there is word cloud data.
    /// </summary>
    public bool HasData => WordCloudData != null && WordCloudData.WordFrequencies.Count > 0;

    /// <summary>
    /// Gets the word count.
    /// </summary>
    public int WordCount => WordCloudData?.WordFrequencies.Count ?? 0;

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
    /// Gets or sets whether to include prompts.
    /// </summary>
    public bool IncludePrompts
    {
        get => _includePrompts;
        set
        {
            if (_includePrompts != value)
            {
                _includePrompts = value;
                OnPropertyChanged(nameof(IncludePrompts));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to include responses.
    /// </summary>
    public bool IncludeResponses
    {
        get => _includeResponses;
        set
        {
            if (_includeResponses != value)
            {
                _includeResponses = value;
                OnPropertyChanged(nameof(IncludeResponses));
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
    /// Gets or sets the minimum frequency threshold.
    /// </summary>
    public int MinFrequency
    {
        get => _minFrequency;
        set
        {
            if (_minFrequency != value)
            {
                _minFrequency = value;
                OnPropertyChanged(nameof(MinFrequency));
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of words.
    /// </summary>
    public int? MaxWords
    {
        get => _maxWords;
        set
        {
            if (_maxWords != value)
            {
                _maxWords = value;
                OnPropertyChanged(nameof(MaxWords));
            }
        }
    }

    /// <summary>
    /// Gets the selected tags for filtering.
    /// </summary>
    public ObservableCollection<TagTaxonomyNode> SelectedTags { get; }

    /// <summary>
    /// Gets the command to generate the word cloud.
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
    /// Generates the word cloud.
    /// </summary>
    public async Task GenerateWordCloudAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var sourceTypes = new HashSet<TextSourceType>();
            if (IncludePrompts)
                sourceTypes.Add(TextSourceType.Prompts);
            if (IncludeResponses)
                sourceTypes.Add(TextSourceType.Responses);
            if (IncludeAnnotations)
                sourceTypes.Add(TextSourceType.Annotations);
            if (IncludeTopics)
                sourceTypes.Add(TextSourceType.Topics);

            if (sourceTypes.Count == 0)
            {
                ErrorMessage = "Please select at least one source type.";
                return;
            }

            var options = new WordCloudOptions
            {
                SourceTypes = sourceTypes,
                TagIds = SelectedTags.Select(t => t.Id).ToList(),
                IncludeTagDescendants = IncludeTagDescendants,
                StartDate = StartDate,
                EndDate = EndDate,
                MinFrequency = MinFrequency,
                MaxWords = MaxWords
            };

            WordCloudData = await _analyticsService.GenerateWordCloudAsync(options, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Generation was cancelled, ignore
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Generation failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Word cloud generation error: {ex.Message}");
            WordCloudData = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Exports word cloud data to JSON.
    /// </summary>
    public async Task ExportJsonAsync()
    {
        if (WordCloudData == null)
            return;

        try
        {
            var json = await _exportService.ExportWordCloudToJsonAsync(WordCloudData);
            
            // Save to file using MAUI FileSystem API
            var fileName = $"wordcloud_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllTextAsync(filePath, json);

            // Share file
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Word Cloud",
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
    /// Exports word cloud data to CSV.
    /// </summary>
    public async Task ExportCsvAsync()
    {
        if (WordCloudData == null)
            return;

        try
        {
            var csv = await _exportService.ExportWordCloudToCsvAsync(WordCloudData);
            
            var fileName = $"wordcloud_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllTextAsync(filePath, csv);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Word Cloud",
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
    /// Exports word cloud visualization to PNG.
    /// </summary>
    public async Task ExportPngAsync()
    {
        if (WordCloudData == null)
            return;

        try
        {
            var pngBytes = await _exportService.ExportWordCloudToPngAsync(WordCloudData);
            
            var fileName = $"wordcloud_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, pngBytes);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Word Cloud",
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


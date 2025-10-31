using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.App.Services;
using IdeaBranch.App.Services.LLM;
using IdeaBranch.Domain;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for TopicNodeDetailPage that manages editing a single topic node.
/// </summary>
public class TopicNodeDetailViewModel : INotifyPropertyChanged
{
    private readonly TopicNode _node;
    private readonly Func<TopicNode, Task> _saveCallback;
    private readonly LLMClientFactory _llmFactory;
    private readonly Func<TopicNode, Task>? _onChildrenAdded;
    private readonly TelemetryService? _telemetry;
    private readonly IAnnotationsRepository? _annotationsRepository;
    private readonly Services.SettingsService? _settingsService;
    
    private string _title;
    private string _prompt;
    private string _response;
    private bool _isBusy;
    private string? _errorMessage;
    private string? _errorType; // "llm" or "repository"
    private Func<Task>? _retryAction;
    private IReadOnlyList<Annotation> _annotations = Array.Empty<Annotation>();
    private IReadOnlyList<Guid> _selectedTagFilters = Array.Empty<Guid>();
    private Dictionary<Guid, HashSet<Guid>> _annotationTags = new();

    /// <summary>
    /// Initializes a new instance with a topic node to edit.
    /// </summary>
    public TopicNodeDetailViewModel(
        TopicNode node, 
        Func<TopicNode, Task> saveCallback,
        LLMClientFactory llmFactory,
        Func<TopicNode, Task>? onChildrenAdded = null,
        TelemetryService? telemetry = null,
        IAnnotationsRepository? annotationsRepository = null,
        Services.SettingsService? settingsService = null)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        _saveCallback = saveCallback ?? throw new ArgumentNullException(nameof(saveCallback));
        _llmFactory = llmFactory ?? throw new ArgumentNullException(nameof(llmFactory));
        _onChildrenAdded = onChildrenAdded;
        _telemetry = telemetry;
        _annotationsRepository = annotationsRepository;
        _settingsService = settingsService;
        
        _title = node.Title ?? string.Empty;
        _prompt = node.Prompt ?? string.Empty;
        _response = node.Response ?? string.Empty;
        
        // Load annotations and settings asynchronously
        _ = LoadAnnotationsAsync();
        _ = LoadShowCommentsSettingAsync();
    }

    /// <summary>
    /// Gets or sets the title of the topic node.
    /// </summary>
    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    /// <summary>
    /// Gets or sets the prompt text for the topic node.
    /// </summary>
    public string Prompt
    {
        get => _prompt;
        set
        {
            if (_prompt != value)
            {
                _prompt = value;
                OnPropertyChanged(nameof(Prompt));
            }
        }
    }

    /// <summary>
    /// Gets or sets the response text for the topic node.
    /// </summary>
    public string Response
    {
        get => _response;
        set
        {
            if (_response != value)
            {
                _response = value;
                OnPropertyChanged(nameof(Response));
            }
        }
    }

    /// <summary>
    /// Gets the ID of the topic node.
    /// </summary>
    public Guid NodeId => _node.Id;

    /// <summary>
    /// Gets or sets whether an operation is in progress.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }

    /// <summary>
    /// Gets or sets the error message to display.
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
    /// Gets the error type (e.g., "llm" or "repository").
    /// </summary>
    public string? ErrorType
    {
        get => _errorType;
        private set
        {
            if (_errorType != value)
            {
                _errorType = value;
                OnPropertyChanged(nameof(ErrorType));
                OnPropertyChanged(nameof(HasRetry));
            }
        }
    }

    /// <summary>
    /// Gets whether a retry action is available.
    /// </summary>
    public bool HasRetry => _retryAction != null && HasError;

    /// <summary>
    /// Saves the changes to the topic node.
    /// </summary>
    public async Task SaveAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            ErrorType = null;
            _retryAction = null;

            _node.Title = string.IsNullOrWhiteSpace(Title) ? null : Title.Trim();
            _node.Prompt = Prompt?.Trim() ?? string.Empty;
            _node.SetResponse(Response?.Trim() ?? string.Empty, parseListItems: false);

            await _saveCallback(_node);
            
            // Emit telemetry
            _telemetry?.EmitCrudEvent("update", _node.Id.ToString());
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save changes: {ex.Message}";
            ErrorType = "repository";
            _retryAction = async () => await SaveAsync();
            
            // Note: No telemetry on failure - error is already logged via exception
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Gets the parent path breadcrumb for context.
    /// </summary>
    public string ParentPath
    {
        get
        {
            var path = new List<string>();
            var current = _node.Parent;
            
            while (current != null)
            {
                path.Insert(0, current.Title ?? current.Prompt);
                current = current.Parent;
            }

            return path.Count > 0 ? string.Join(" > ", path) : "Root";
        }
    }

    /// <summary>
    /// Generates a response using the LLM client.
    /// </summary>
    public async Task GenerateResponseAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(Prompt))
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            // Get LLM client from factory
            var client = await _llmFactory.CreateClientAsync();

            // Build context path from parent nodes
            var contextPath = BuildContextPath();

            // Generate response
            var generatedResponse = await client.GenerateResponseAsync(Prompt, contextPath);

            // Update response text
            Response = generatedResponse;

            // Parse list items if present and add as children
            if (_onChildrenAdded != null)
            {
                _node.SetResponse(generatedResponse, parseListItems: true);
                if (_node.Children.Count > 0)
                {
                    await _onChildrenAdded(_node);
                }
            }
            else
            {
                _node.SetResponse(generatedResponse, parseListItems: true);
            }

            // Save changes
            await SaveAsync();
            
            // Emit telemetry for success
            _telemetry?.EmitLlmEvent("generate_response", _node.Id.ToString(), success: true);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error generating response: {ex.Message}";
            ErrorType = "llm";
            _retryAction = async () => await GenerateResponseAsync();
            
            // Emit telemetry for failure
            _telemetry?.EmitLlmEvent("generate_response", _node.Id.ToString(), success: false, errorMessage: ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Generates a title suggestion using the LLM client.
    /// </summary>
    public async Task GenerateTitleAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(Prompt))
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            // Get LLM client from factory
            var client = await _llmFactory.CreateClientAsync();

            // Use current response if available, otherwise generate one
            var responseText = Response;
            if (string.IsNullOrWhiteSpace(responseText))
            {
                // Build context path from parent nodes
                var contextPath = BuildContextPath();
                responseText = await client.GenerateResponseAsync(Prompt, contextPath);
                Response = responseText;
            }

            // Generate title suggestion
            var suggestedTitle = await client.SuggestTitleAsync(Prompt, responseText);
            
            if (!string.IsNullOrWhiteSpace(suggestedTitle))
            {
                Title = suggestedTitle;
                await SaveAsync();
                
                // Emit telemetry for success
                _telemetry?.EmitLlmEvent("suggest_title", _node.Id.ToString(), success: true);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error generating title: {ex.Message}";
            ErrorType = "llm";
            _retryAction = async () => await GenerateTitleAsync();
            
            // Emit telemetry for failure
            _telemetry?.EmitLlmEvent("suggest_title", _node.Id.ToString(), success: false, errorMessage: ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Builds context path from parent nodes (root to parent).
    /// </summary>
    private IEnumerable<string> BuildContextPath()
    {
        var path = new List<string>();
        var current = _node.Parent;

        // Collect context from parent to root
        while (current != null)
        {
            var contextText = $"{current.Title ?? "Untitled"}\nPrompt: {current.Prompt}\nResponse: {current.Response}";
            path.Insert(0, contextText);
            current = current.Parent;
        }

        return path;
    }

    /// <summary>
    /// Retries the last failed operation.
    /// </summary>
    public async Task RetryAsync()
    {
        if (_retryAction != null)
        {
            await _retryAction();
        }
    }

    /// <summary>
    /// Gets the annotations for this topic node.
    /// </summary>
    public IReadOnlyList<Annotation> Annotations
    {
        get => _annotations;
        private set
        {
            if (_annotations != value)
            {
                _annotations = value;
                OnPropertyChanged(nameof(Annotations));
                OnPropertyChanged(nameof(FilteredAnnotations));
            }
        }
    }

    /// <summary>
    /// Gets the filtered annotations based on selected tag filters.
    /// </summary>
    public IReadOnlyList<Annotation> FilteredAnnotations
    {
        get
        {
            if (_selectedTagFilters.Count == 0)
                return _annotations;
            
            // Filter annotations that have all selected tags
            return _annotations.Where(a => HasAllTags(a, _selectedTagFilters)).ToList();
        }
    }

    /// <summary>
    /// Gets or sets the selected tag filter IDs.
    /// </summary>
    public IReadOnlyList<Guid> SelectedTagFilters
    {
        get => _selectedTagFilters;
        set
        {
            if (_selectedTagFilters != value)
            {
                _selectedTagFilters = value ?? Array.Empty<Guid>();
                OnPropertyChanged(nameof(SelectedTagFilters));
                OnPropertyChanged(nameof(FilteredAnnotations));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether comments should be visible.
    /// </summary>
    public bool ShowComments
    {
        get => _showComments;
        set
        {
            if (_showComments != value)
            {
                _showComments = value;
                OnPropertyChanged(nameof(ShowComments));
                // Persist setting
                if (_settingsService != null)
                {
                    _ = _settingsService.SetShowCommentsAsync(value);
                }
            }
        }
    }
    private bool _showComments = true;

    /// <summary>
    /// Loads the ShowComments setting from storage.
    /// </summary>
    private async Task LoadShowCommentsSettingAsync()
    {
        if (_settingsService == null)
            return;

        try
        {
            _showComments = await _settingsService.GetShowCommentsAsync();
            OnPropertyChanged(nameof(ShowComments));
        }
        catch
        {
            // Silently fail - use default value
        }
    }

    /// <summary>
    /// Gets or sets whether text is currently selected in the Editor.
    /// </summary>
    public bool HasTextSelection
    {
        get => _hasTextSelection;
        set
        {
            if (_hasTextSelection != value)
            {
                _hasTextSelection = value;
                OnPropertyChanged(nameof(HasTextSelection));
            }
        }
    }
    private bool _hasTextSelection;

    /// <summary>
    /// Gets or sets the start offset of the text selection.
    /// </summary>
    public int SelectionStart
    {
        get => _selectionStart;
        set
        {
            if (_selectionStart != value)
            {
                _selectionStart = value;
                OnPropertyChanged(nameof(SelectionStart));
                UpdateSelectionState();
            }
        }
    }
    private int _selectionStart;

    /// <summary>
    /// Gets or sets the length of the text selection.
    /// </summary>
    public int SelectionLength
    {
        get => _selectionLength;
        set
        {
            if (_selectionLength != value)
            {
                _selectionLength = value;
                OnPropertyChanged(nameof(SelectionLength));
                UpdateSelectionState();
            }
        }
    }
    private int _selectionLength;

    /// <summary>
    /// Gets the end offset of the text selection.
    /// </summary>
    public int SelectionEnd => SelectionStart + SelectionLength;

    /// <summary>
    /// Gets the selected text based on current selection.
    /// </summary>
    public string? SelectedText
    {
        get
        {
            if (SelectionStart < 0 || SelectionLength <= 0 || SelectionStart >= Response.Length)
                return null;

            var end = Math.Min(SelectionStart + SelectionLength, Response.Length);
            if (SelectionStart >= end)
                return null;

            return Response.Substring(SelectionStart, end - SelectionStart);
        }
    }

    /// <summary>
    /// Updates the selection state based on SelectionStart and SelectionLength.
    /// </summary>
    private void UpdateSelectionState()
    {
        HasTextSelection = SelectionStart >= 0 && SelectionLength > 0 && SelectionStart < Response.Length;
        OnPropertyChanged(nameof(SelectedText));
    }

    /// <summary>
    /// Gets or sets the selected annotation.
    /// </summary>
    public Annotation? SelectedAnnotation
    {
        get => _selectedAnnotation;
        set
        {
            if (_selectedAnnotation != value)
            {
                _selectedAnnotation = value;
                OnPropertyChanged(nameof(SelectedAnnotation));
            }
        }
    }
    private Annotation? _selectedAnnotation;

    /// <summary>
    /// Loads annotations for this topic node.
    /// </summary>
    public async Task LoadAnnotationsAsync()
    {
        if (_annotationsRepository == null || _node.Id == Guid.Empty)
            return;

        try
        {
            var annotations = await _annotationsRepository.GetByNodeIdAsync(_node.Id);
            Annotations = annotations;
            
            // Load tag IDs for each annotation for filtering
            await LoadAnnotationTagsAsync();
        }
        catch
        {
            // Silently fail - annotations are optional
            Annotations = Array.Empty<Annotation>();
        }
    }

    private async Task LoadAnnotationTagsAsync()
    {
        if (_annotationsRepository == null)
            return;

        _annotationTags.Clear();
        foreach (var annotation in Annotations)
        {
            try
            {
                var tagIds = await _annotationsRepository.GetTagIdsAsync(annotation.Id);
                _annotationTags[annotation.Id] = new HashSet<Guid>(tagIds);
            }
            catch
            {
                // Silently fail for individual annotations
                _annotationTags[annotation.Id] = new HashSet<Guid>();
            }
        }
    }

    /// <summary>
    /// Creates a new annotation for the specified text span.
    /// </summary>
    public Annotation CreateAnnotation(int startOffset, int endOffset, string? comment = null)
    {
        var annotation = new Annotation(_node.Id, startOffset, endOffset, comment);
        return annotation;
    }

    /// <summary>
    /// Saves an annotation.
    /// </summary>
    public async Task SaveAnnotationAsync(Annotation annotation)
    {
        if (_annotationsRepository == null)
            throw new InvalidOperationException("Annotations repository is not available.");

        await _annotationsRepository.SaveAsync(annotation);
        await LoadAnnotationsAsync();
    }

    /// <summary>
    /// Deletes an annotation.
    /// </summary>
    public async Task DeleteAnnotationAsync(Guid annotationId)
    {
        if (_annotationsRepository == null)
            throw new InvalidOperationException("Annotations repository is not available.");

        await _annotationsRepository.DeleteAsync(annotationId);
        await LoadAnnotationsAsync();
    }

    /// <summary>
    /// Checks if an annotation has all the specified tags.
    /// </summary>
    private bool HasAllTags(Annotation annotation, IReadOnlyList<Guid> tagIds)
    {
        if (tagIds.Count == 0)
            return true;

        if (!_annotationTags.TryGetValue(annotation.Id, out var annotationTagIds))
            return false;

        // Check if annotation has all requested tags
        return tagIds.All(tagId => annotationTagIds.Contains(tagId));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


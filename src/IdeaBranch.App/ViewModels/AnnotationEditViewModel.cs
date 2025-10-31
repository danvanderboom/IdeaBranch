using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.Domain;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for AnnotationEditPage that manages creating and editing annotations.
/// </summary>
public class AnnotationEditViewModel : INotifyPropertyChanged
{
    private readonly Annotation? _existingAnnotation;
    private readonly Guid _nodeId;
    private readonly int _startOffset;
    private readonly int _endOffset;
    private readonly IAnnotationsRepository? _annotationsRepository;
    private readonly ITagTaxonomyRepository? _tagTaxonomyRepository;
    private readonly Func<Task>? _onSaved;

    private string? _comment;
    private IReadOnlyList<Guid> _selectedTags = Array.Empty<Guid>();
    private double? _numericValue;
    private string? _geospatialValue;
    private DateTime? _temporalValue;
    private bool _isBusy;
    private string? _errorMessage;
    private string _selectedText = string.Empty;

    /// <summary>
    /// Initializes a new instance for creating a new annotation.
    /// </summary>
    public AnnotationEditViewModel(
        Guid nodeId,
        int startOffset,
        int endOffset,
        string selectedText,
        IAnnotationsRepository? annotationsRepository,
        ITagTaxonomyRepository? tagTaxonomyRepository,
        Func<Task>? onSaved = null)
    {
        _nodeId = nodeId;
        _startOffset = startOffset;
        _endOffset = endOffset;
        _selectedText = selectedText ?? string.Empty;
        _annotationsRepository = annotationsRepository;
        _tagTaxonomyRepository = tagTaxonomyRepository;
        _onSaved = onSaved;

        _ = LoadAvailableTagsAsync();
    }

    /// <summary>
    /// Initializes a new instance for editing an existing annotation.
    /// </summary>
    public AnnotationEditViewModel(
        Annotation annotation,
        string selectedText,
        IAnnotationsRepository annotationsRepository,
        ITagTaxonomyRepository? tagTaxonomyRepository,
        Func<Task>? onSaved = null)
    {
        _existingAnnotation = annotation ?? throw new ArgumentNullException(nameof(annotation));
        _nodeId = annotation.NodeId;
        _startOffset = annotation.StartOffset;
        _endOffset = annotation.EndOffset;
        _selectedText = selectedText ?? string.Empty;
        _annotationsRepository = annotationsRepository ?? throw new ArgumentNullException(nameof(annotationsRepository));
        _tagTaxonomyRepository = tagTaxonomyRepository;
        _onSaved = onSaved;

        _comment = annotation.Comment;
        _ = LoadAnnotationDataAsync();
    }

    /// <summary>
    /// Gets the selected text span.
    /// </summary>
    public string SelectedText
    {
        get => _selectedText;
        private set
        {
            if (_selectedText != value)
            {
                _selectedText = value;
                OnPropertyChanged(nameof(SelectedText));
            }
        }
    }

    /// <summary>
    /// Gets or sets the annotation comment.
    /// </summary>
    public string? Comment
    {
        get => _comment;
        set
        {
            if (_comment != value)
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }
    }

    /// <summary>
    /// Gets the available tags from the taxonomy as selectable items.
    /// </summary>
    public IReadOnlyList<SelectableTagItem> AvailableTags => _selectableTags;

    private IReadOnlyList<SelectableTagItem> _selectableTags = Array.Empty<SelectableTagItem>();

    /// <summary>
    /// Wrapper class for selectable tag items in the UI.
    /// </summary>
    public class SelectableTagItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public TagTaxonomyNode Tag { get; }
        public Guid Id => Tag.Id;
        public string Name => Tag.Name;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public SelectableTagItem(TagTaxonomyNode tag)
        {
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets or sets the selected tag IDs.
    /// </summary>
    public IReadOnlyList<Guid> SelectedTags
    {
        get => _selectedTags;
        set
        {
            if (_selectedTags != value)
            {
                _selectedTags = value ?? Array.Empty<Guid>();
                OnPropertyChanged(nameof(SelectedTags));
            }
        }
    }

    /// <summary>
    /// Updates selected tags based on SelectableTagItems selection state.
    /// </summary>
    public void UpdateSelectedTags()
    {
        SelectedTags = _selectableTags.Where(item => item.IsSelected).Select(item => item.Id).ToList();
    }

    /// <summary>
    /// Gets or sets the numeric value.
    /// </summary>
    public double? NumericValue
    {
        get => _numericValue;
        set
        {
            if (_numericValue != value)
            {
                _numericValue = value;
                OnPropertyChanged(nameof(NumericValue));
            }
        }
    }

    /// <summary>
    /// Gets or sets the geospatial value (JSON).
    /// </summary>
    public string? GeospatialValue
    {
        get => _geospatialValue;
        set
        {
            if (_geospatialValue != value)
            {
                _geospatialValue = value;
                OnPropertyChanged(nameof(GeospatialValue));
            }
        }
    }

    /// <summary>
    /// Gets or sets the temporal value.
    /// </summary>
    public DateTime? TemporalValue
    {
        get => _temporalValue;
        set
        {
            if (_temporalValue != value)
            {
                _temporalValue = value;
                OnPropertyChanged(nameof(TemporalValue));
            }
        }
    }

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
    /// Gets whether this is editing an existing annotation.
    /// </summary>
    public bool IsEditing => _existingAnnotation != null;

    /// <summary>
    /// Loads available tags from the taxonomy.
    /// </summary>
    private async Task LoadAvailableTagsAsync()
    {
        if (_tagTaxonomyRepository == null)
            return;

        try
        {
            var root = await _tagTaxonomyRepository.GetRootAsync();
            var allTags = new List<TagTaxonomyNode> { root };
            await LoadTagChildrenAsync(root.Id, allTags);
            
            // Convert to selectable items
            var selectableItems = allTags.Select(tag => new SelectableTagItem(tag)).ToList();
            
            // Mark selected tags
            foreach (var item in selectableItems)
            {
                item.IsSelected = _selectedTags.Contains(item.Id);
            }
            
            _selectableTags = selectableItems;
            OnPropertyChanged(nameof(AvailableTags));
        }
        catch
        {
            // Silently fail - tags are optional
            _selectableTags = Array.Empty<SelectableTagItem>();
            OnPropertyChanged(nameof(AvailableTags));
        }
    }

    /// <summary>
    /// Recursively loads tag children.
    /// </summary>
    private async Task LoadTagChildrenAsync(Guid parentId, List<TagTaxonomyNode> result)
    {
        if (_tagTaxonomyRepository == null)
            return;

        var children = await _tagTaxonomyRepository.GetChildrenAsync(parentId);
        foreach (var child in children)
        {
            result.Add(child);
            await LoadTagChildrenAsync(child.Id, result);
        }
    }

    /// <summary>
    /// Loads existing annotation data including tags and values.
    /// </summary>
    private async Task LoadAnnotationDataAsync()
    {
        if (_existingAnnotation == null || _annotationsRepository == null)
            return;

        try
        {
            // Load tags
            var tagIds = await _annotationsRepository.GetTagIdsAsync(_existingAnnotation.Id);
            _selectedTags = tagIds;
            SelectedTags = tagIds;
            
            // Update selectable tags selection state
            await LoadAvailableTagsAsync();

            // Load values
            var values = await _annotationsRepository.GetValuesAsync(_existingAnnotation.Id);
            foreach (var value in values)
            {
                switch (value.ValueType)
                {
                    case "numeric":
                        NumericValue = value.NumericValue;
                        break;
                    case "geospatial":
                        GeospatialValue = value.GeospatialValue;
                        break;
                    case "temporal":
                        if (DateTime.TryParse(value.TemporalValue, out var dt))
                            TemporalValue = dt;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading annotation data: {ex.Message}";
        }
    }

    /// <summary>
    /// Saves the annotation.
    /// </summary>
    public async Task SaveAsync()
    {
        if (IsBusy || _annotationsRepository == null)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            Annotation annotation;
            if (_existingAnnotation != null)
            {
                // Update existing annotation
                annotation = _existingAnnotation;
                annotation.Comment = Comment;
                annotation.UpdateSpan(_startOffset, _endOffset);
            }
            else
            {
                // Create new annotation
                annotation = new Annotation(_nodeId, _startOffset, _endOffset, Comment);
            }

            // Save annotation
            await _annotationsRepository.SaveAsync(annotation);

            // Update selected tags from UI
            UpdateSelectedTags();

            // Update tags
            var existingTagIds = _existingAnnotation != null
                ? await _annotationsRepository.GetTagIdsAsync(annotation.Id)
                : Array.Empty<Guid>();

            // Remove tags that are no longer selected
            foreach (var tagId in existingTagIds)
            {
                if (!SelectedTags.Contains(tagId))
                {
                    await _annotationsRepository.RemoveTagAsync(annotation.Id, tagId);
                }
            }

            // Add new tags
            foreach (var tagId in SelectedTags)
            {
                if (!existingTagIds.Contains(tagId))
                {
                    await _annotationsRepository.AddTagAsync(annotation.Id, tagId);
                }
            }

            // Save values
            if (_existingAnnotation != null)
            {
                // Delete existing values
                var existingValues = await _annotationsRepository.GetValuesAsync(annotation.Id);
                foreach (var value in existingValues)
                {
                    await _annotationsRepository.DeleteValueAsync(value.Id);
                }
            }

            // Add new values
            if (NumericValue.HasValue)
            {
                var value = new AnnotationValue(annotation.Id, "numeric")
                {
                    NumericValue = NumericValue.Value
                };
                await _annotationsRepository.SaveValueAsync(value);
            }

            if (!string.IsNullOrWhiteSpace(GeospatialValue))
            {
                var value = new AnnotationValue(annotation.Id, "geospatial")
                {
                    GeospatialValue = GeospatialValue
                };
                await _annotationsRepository.SaveValueAsync(value);
            }

            if (TemporalValue.HasValue)
            {
                var value = new AnnotationValue(annotation.Id, "temporal")
                {
                    TemporalValue = TemporalValue.Value.ToString("O") // ISO 8601
                };
                await _annotationsRepository.SaveValueAsync(value);
            }

            // Notify parent to refresh
            if (_onSaved != null)
            {
                await _onSaved();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving annotation: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Deletes the annotation if editing.
    /// </summary>
    public async Task DeleteAsync()
    {
        if (!IsEditing || _existingAnnotation == null || _annotationsRepository == null || IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await _annotationsRepository.DeleteAsync(_existingAnnotation.Id);

            // Notify parent to refresh
            if (_onSaved != null)
            {
                await _onSaved();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting annotation: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


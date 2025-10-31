using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.Domain;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for VersionHistoryPage that displays version history for a topic node.
/// </summary>
public class VersionHistoryViewModel : INotifyPropertyChanged
{
    private readonly IVersionHistoryRepository _repository;
    private readonly Guid _nodeId;
    private readonly string? _nodeTitle;
    
    private ObservableCollection<TopicNodeVersion> _versions = new();
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance with a node ID to load version history for.
    /// </summary>
    public VersionHistoryViewModel(
        Guid nodeId,
        IVersionHistoryRepository repository,
        string? nodeTitle = null)
    {
        _nodeId = nodeId;
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _nodeTitle = nodeTitle;
        
        // Load version history
        LoadVersionHistoryAsync();
    }

    /// <summary>
    /// Gets the node ID this history is for.
    /// </summary>
    public Guid NodeId => _nodeId;

    /// <summary>
    /// Gets the title of the node this history is for.
    /// </summary>
    public string? NodeTitle => _nodeTitle;

    /// <summary>
    /// Gets the collection of version history entries.
    /// </summary>
    public ObservableCollection<TopicNodeVersion> Versions
    {
        get => _versions;
        private set
        {
            if (_versions != value)
            {
                _versions = value;
                OnPropertyChanged(nameof(Versions));
            }
        }
    }

    /// <summary>
    /// Gets whether version history is currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    /// <summary>
    /// Gets the error message, if any.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
    }

    /// <summary>
    /// Gets whether there are any version history entries.
    /// </summary>
    public bool HasVersions => Versions.Any();

    /// <summary>
    /// Loads version history for the node.
    /// </summary>
    private async void LoadVersionHistoryAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var versions = await _repository.GetByNodeIdAsync(_nodeId);
            Versions = new ObservableCollection<TopicNodeVersion>(versions);
            OnPropertyChanged(nameof(HasVersions));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load version history: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refreshes the version history.
    /// </summary>
    public async Task RefreshAsync()
    {
        await Task.Run(() => LoadVersionHistoryAsync());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


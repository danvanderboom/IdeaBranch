using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
    
    private string _title;
    private string _prompt;
    private string _response;
    private bool _isBusy;
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance with a topic node to edit.
    /// </summary>
    public TopicNodeDetailViewModel(
        TopicNode node, 
        Func<TopicNode, Task> saveCallback,
        LLMClientFactory llmFactory,
        Func<TopicNode, Task>? onChildrenAdded = null)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        _saveCallback = saveCallback ?? throw new ArgumentNullException(nameof(saveCallback));
        _llmFactory = llmFactory ?? throw new ArgumentNullException(nameof(llmFactory));
        _onChildrenAdded = onChildrenAdded;
        
        _title = node.Title ?? string.Empty;
        _prompt = node.Prompt ?? string.Empty;
        _response = node.Response ?? string.Empty;
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
    /// Saves the changes to the topic node.
    /// </summary>
    public async Task SaveAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            _node.Title = string.IsNullOrWhiteSpace(Title) ? null : Title.Trim();
            _node.Prompt = Prompt?.Trim() ?? string.Empty;
            _node.SetResponse(Response?.Trim() ?? string.Empty, parseListItems: false);

            await _saveCallback(_node);
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
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error generating response: {ex.Message}";
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
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error generating title: {ex.Message}";
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

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


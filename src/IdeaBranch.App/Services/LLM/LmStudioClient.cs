using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

namespace IdeaBranch.App.Services.LLM;

/// <summary>
/// LLM client implementation for LM Studio (OpenAI-compatible local endpoint).
/// </summary>
public class LmStudioClient : ILLMClient
{
    private readonly OpenAIClient _client;
    private readonly string _model;
    private readonly ILogger<LmStudioClient>? _logger;

    /// <summary>
    /// Initializes a new instance with LM Studio endpoint and model.
    /// </summary>
    /// <param name="endpoint">The LM Studio server endpoint (e.g., http://localhost:1234/v1).</param>
    /// <param name="model">The model name/ID to use.</param>
    /// <param name="apiKey">Optional API key (defaults to "lm-studio" if not provided).</param>
    /// <param name="logger">Optional logger.</param>
    public LmStudioClient(string endpoint, string model, string? apiKey = null, ILogger<LmStudioClient>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty.", nameof(endpoint));
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model cannot be null or empty.", nameof(model));

        var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
        _client = new OpenAIClient(new ApiKeyCredential(apiKey ?? "lm-studio"), clientOptions);
        _model = model;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> GenerateResponseAsync(string prompt, IEnumerable<string> contextPath, CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(prompt, contextPath);
        
        var options = new ChatCompletionOptions
        {
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxTokens = 2000
        };

        try
        {
            var response = await _client.GetChatClient(_model).CompleteChatAsync(messages, options, cancellationToken: cancellationToken);
            var responseText = response.Value.Content[0].Text ?? "No response generated.";
            
            _logger?.LogInformation("LM Studio generated response (length: {Length})", responseText.Length);
            return responseText;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error generating response from LM Studio");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> SuggestTitleAsync(string prompt, string response, CancellationToken cancellationToken = default)
    {
        var titlePrompt = $"Based on the following prompt and response, suggest a concise title (5-10 words):\n\nPrompt: {prompt}\n\nResponse: {response}\n\nTitle:";
        
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateUserMessage(titlePrompt)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0.3f,
            TopP = 0.9f,
            MaxTokens = 50
        };

        try
        {
            var titleResponse = await _client.GetChatClient(_model).CompleteChatAsync(messages, options, cancellationToken: cancellationToken);
            var title = titleResponse.Value.Content[0].Text?.Trim();
            
            _logger?.LogInformation("LM Studio suggested title: {Title}", title);
            return string.IsNullOrWhiteSpace(title) ? null : title;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error generating title from LM Studio");
            return null;
        }
    }

    /// <summary>
    /// Builds chat messages from prompt and context path.
    /// </summary>
    private List<ChatMessage> BuildMessages(string prompt, IEnumerable<string> contextPath)
    {
        var messages = new List<ChatMessage>();

        // Add system message with context from parent nodes
        if (contextPath.Any())
        {
            var contextText = string.Join("\n\n", contextPath.Select((text, index) => $"Context {index + 1}:\n{text}"));
            var systemMessage = $"You are a research assistant. Use the following context to inform your response:\n\n{contextText}\n\nProvide a helpful, informative response to the user's prompt.";
            messages.Add(ChatMessage.CreateSystemMessage(systemMessage));
        }
        else
        {
            messages.Add(ChatMessage.CreateSystemMessage("You are a helpful research assistant. Provide informative and well-structured responses."));
        }

        // Add user prompt
        messages.Add(ChatMessage.CreateUserMessage(prompt));

        return messages;
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace IdeaBranch.App.Services.LLM;

/// <summary>
/// LLM client implementation for Azure OpenAI.
/// </summary>
public class AzureOpenAIClient : ILLMClient
{
    private readonly OpenAIClient _client;
    private readonly string _deployment;
    private readonly ILogger<AzureOpenAIClient>? _logger;

    /// <summary>
    /// Initializes a new instance with Azure OpenAI endpoint and deployment.
    /// </summary>
    /// <param name="endpoint">The Azure OpenAI endpoint (e.g., https://your-resource.openai.azure.com/openai/v1).</param>
    /// <param name="deployment">The deployment name.</param>
    /// <param name="apiKey">Optional API key. If not provided, uses DefaultAzureCredential.</param>
    /// <param name="logger">Optional logger.</param>
    public AzureOpenAIClient(string endpoint, string deployment, string? apiKey = null, ILogger<AzureOpenAIClient>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty.", nameof(endpoint));
        if (string.IsNullOrWhiteSpace(deployment))
            throw new ArgumentException("Deployment cannot be null or empty.", nameof(deployment));

        var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
        
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _client = new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions);
        }
        else
        {
            // Use AzureKeyCredential from OpenAI SDK for Azure authentication
            // Note: DefaultAzureCredential is not directly supported by OpenAI SDK
            // For now, require API key - Azure AD auth can be added later if needed
            throw new ArgumentException("API key is required for Azure OpenAI. Azure AD authentication not yet implemented.", nameof(apiKey));
        }
        
        _deployment = deployment;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> GenerateResponseAsync(string prompt, IEnumerable<string> contextPath, CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(prompt, contextPath);
        
        var options = new ChatCompletionOptions
        {
            Temperature = 0.7f,
            TopP = 0.9f
        };

        try
        {
            var response = await _client.GetChatClient(_deployment).CompleteChatAsync(messages, options, cancellationToken: cancellationToken);
            var responseText = response.Value.Content[0].Text ?? "No response generated.";
            
            _logger?.LogInformation("Azure OpenAI generated response (length: {Length})", responseText.Length);
            return responseText;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error generating response from Azure OpenAI");
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
            TopP = 0.9f
        };

        try
        {
            var titleResponse = await _client.GetChatClient(_deployment).CompleteChatAsync(messages, options, cancellationToken: cancellationToken);
            var title = titleResponse.Value.Content[0].Text?.Trim();
            
            _logger?.LogInformation("Azure OpenAI suggested title: {Title}", title);
            return string.IsNullOrWhiteSpace(title) ? null : title;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error generating title from Azure OpenAI");
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


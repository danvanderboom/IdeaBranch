using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using OpenAI;
using System.ClientModel;
using System.Collections.Generic;

namespace CriticalInsight.Data.IntegrationTests.Agents.Integration;

public interface IAgentRunner
{
    Task<string> RunAsync(string prompt, CancellationToken cancellationToken = default);
}

public static class AgentProvider
{
    public static bool IsLiveEnabled()
        => string.Equals(Environment.GetEnvironmentVariable("CI_AF_LIVE"), "1", StringComparison.OrdinalIgnoreCase);

    public static string GetProvider()
        => Environment.GetEnvironmentVariable("CI_AF_PROVIDER")?.Trim().ToLowerInvariant() switch
        {
            "lmstudio" => "lmstudio",
            _ => "azure"
        };

    public static IAgentRunner CreateAgentRunner()
    {
        string provider = GetProvider();
        return provider == "lmstudio" ? CreateLmStudioRunner() : CreateAzureRunner();
    }

    private static IAgentRunner CreateAzureRunner()
    {
        string? endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        string? deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");
        string? apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(endpoint))
            throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
        if (string.IsNullOrWhiteSpace(deployment))
            throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT is not set.");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set. Azure CLI authentication not yet supported in this version.");

        var client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
        
        // Use direct OpenAI client for Azure as well for consistency
        return new DirectOpenAIRunner(client, deployment);
    }

    private static IAgentRunner CreateLmStudioRunner()
    {
        string endpoint = Environment.GetEnvironmentVariable("LMSTUDIO_ENDPOINT") ?? "http://localhost:1234/v1";
        string model = Environment.GetEnvironmentVariable("LMSTUDIO_MODEL") ?? "lmstudio-model";
        string apiKey = Environment.GetEnvironmentVariable("LMSTUDIO_API_KEY") ?? "lm-studio";

        var client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
        
        // Use direct OpenAI client instead of Agent Framework for LM Studio compatibility
        return new DirectOpenAIRunner(client, model);
    }
}

public sealed class DirectOpenAIRunner : IAgentRunner
{
    private readonly OpenAIClient _client;
    private readonly string _model;

    public DirectOpenAIRunner(OpenAIClient client, string model)
    {
        _client = client;
        _model = model;
    }

    public async Task<string> RunAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var messages = new List<OpenAI.Chat.ChatMessage>
        {
            OpenAI.Chat.ChatMessage.CreateSystemMessage(
                "Think through the question using only the provided tree context. Then answer with ONE capital letter: A, B, C, or D only. Do not include explanation."),
            // lightweight one-shot to reinforce output constraint
            OpenAI.Chat.ChatMessage.CreateUserMessage("Question: Pick the correct choice. Choices: A) X B) Y C) Z D) W"),
            OpenAI.Chat.ChatMessage.CreateAssistantMessage("A"),
            OpenAI.Chat.ChatMessage.CreateUserMessage(prompt)
        };

        var tempStr = Environment.GetEnvironmentVariable("CI_AF_TEMP");
        float temperature = 0.2f;
        if (double.TryParse(tempStr, out var t))
        {
            temperature = (float)t;
        }

        var options = new OpenAI.Chat.ChatCompletionOptions
        {
            Temperature = temperature,
            TopP = 0.9f
        };

        var response = await _client.GetChatClient(_model).CompleteChatAsync(messages, options, cancellationToken: cancellationToken);
        return response.Value.Content[0].Text ?? "No response";
    }
}



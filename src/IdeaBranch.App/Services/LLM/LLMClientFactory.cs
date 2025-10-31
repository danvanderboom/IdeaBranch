using System;
using System.Threading.Tasks;
using IdeaBranch.App.Services;
using Microsoft.Extensions.Logging;

namespace IdeaBranch.App.Services.LLM;

/// <summary>
/// Factory for creating LLM client instances based on settings.
/// </summary>
public class LLMClientFactory
{
    private readonly SettingsService _settings;
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes a new instance with settings service and optional logger factory.
    /// </summary>
    public LLMClientFactory(SettingsService settings, ILoggerFactory? loggerFactory = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Creates an LLM client based on current settings.
    /// </summary>
    public async Task<ILLMClient> CreateClientAsync()
    {
        var provider = await _settings.GetProviderAsync();

        return provider switch
        {
            "lmstudio" => await CreateLmStudioClientAsync(),
            "azure" => await CreateAzureClientAsync(),
            _ => throw new InvalidOperationException($"Unknown LLM provider: {provider}")
        };
    }

    /// <summary>
    /// Creates an LM Studio client from settings.
    /// </summary>
    private async Task<ILLMClient> CreateLmStudioClientAsync()
    {
        var endpoint = await _settings.GetLmStudioEndpointAsync();
        var model = await _settings.GetLmStudioModelAsync();

        var logger = _loggerFactory?.CreateLogger<LmStudioClient>();
        return new LmStudioClient(endpoint, model, logger: logger);
    }

    /// <summary>
    /// Creates an Azure OpenAI client from settings.
    /// </summary>
    private async Task<ILLMClient> CreateAzureClientAsync()
    {
        var endpoint = await _settings.GetAzureEndpointAsync();
        var deployment = await _settings.GetAzureDeploymentAsync();
        var apiKey = await _settings.GetAzureApiKeyAsync();

        if (string.IsNullOrWhiteSpace(endpoint))
            throw new InvalidOperationException("Azure OpenAI endpoint is not configured. Please set it in settings.");
        if (string.IsNullOrWhiteSpace(deployment))
            throw new InvalidOperationException("Azure OpenAI deployment is not configured. Please set it in settings.");

        var logger = _loggerFactory?.CreateLogger<AzureOpenAIClient>();
        return new AzureOpenAIClient(endpoint, deployment, apiKey, logger);
    }
}


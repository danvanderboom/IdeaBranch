using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace IdeaBranch.App.Services;

/// <summary>
/// Service for managing application settings, including LLM provider configuration.
/// </summary>
public class SettingsService
{
    private const string ProviderKey = "llm_provider";
    private const string LmStudioEndpointKey = "lmstudio_endpoint";
    private const string LmStudioModelKey = "lmstudio_model";
    private const string AzureEndpointKey = "azure_endpoint";
    private const string AzureDeploymentKey = "azure_deployment";
    private const string AzureApiKeyKey = "azure_api_key";

    private const string DefaultProvider = "lmstudio";
    private const string DefaultLmStudioEndpoint = "http://localhost:1234/v1";
    private const string DefaultLmStudioModel = "phi-4";

    /// <summary>
    /// Gets or sets the current LLM provider name ("lmstudio" or "azure").
    /// </summary>
    public async Task<string> GetProviderAsync()
    {
        var provider = await SecureStorage.GetAsync(ProviderKey);
        return provider ?? DefaultProvider;
    }

    /// <summary>
    /// Sets the current LLM provider name.
    /// </summary>
    public async Task SetProviderAsync(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider cannot be null or empty.", nameof(provider));

        await SecureStorage.SetAsync(ProviderKey, provider.Trim().ToLowerInvariant());
    }

    /// <summary>
    /// Gets the LM Studio endpoint.
    /// </summary>
    public async Task<string> GetLmStudioEndpointAsync()
    {
        var endpoint = await SecureStorage.GetAsync(LmStudioEndpointKey);
        return endpoint ?? DefaultLmStudioEndpoint;
    }

    /// <summary>
    /// Sets the LM Studio endpoint.
    /// </summary>
    public async Task SetLmStudioEndpointAsync(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty.", nameof(endpoint));

        await SecureStorage.SetAsync(LmStudioEndpointKey, endpoint.Trim());
    }

    /// <summary>
    /// Gets the LM Studio model name.
    /// </summary>
    public async Task<string> GetLmStudioModelAsync()
    {
        var model = await SecureStorage.GetAsync(LmStudioModelKey);
        return model ?? DefaultLmStudioModel;
    }

    /// <summary>
    /// Sets the LM Studio model name.
    /// </summary>
    public async Task SetLmStudioModelAsync(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model cannot be null or empty.", nameof(model));

        await SecureStorage.SetAsync(LmStudioModelKey, model.Trim());
    }

    /// <summary>
    /// Gets the Azure OpenAI endpoint.
    /// </summary>
    public async Task<string?> GetAzureEndpointAsync()
    {
        return await SecureStorage.GetAsync(AzureEndpointKey);
    }

    /// <summary>
    /// Sets the Azure OpenAI endpoint.
    /// </summary>
    public async Task SetAzureEndpointAsync(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            SecureStorage.Remove(AzureEndpointKey);
        }
        else
        {
            await SecureStorage.SetAsync(AzureEndpointKey, endpoint.Trim());
        }
    }

    /// <summary>
    /// Gets the Azure OpenAI deployment name.
    /// </summary>
    public async Task<string?> GetAzureDeploymentAsync()
    {
        return await SecureStorage.GetAsync(AzureDeploymentKey);
    }

    /// <summary>
    /// Sets the Azure OpenAI deployment name.
    /// </summary>
    public async Task SetAzureDeploymentAsync(string? deployment)
    {
        if (string.IsNullOrWhiteSpace(deployment))
        {
            SecureStorage.Remove(AzureDeploymentKey);
        }
        else
        {
            await SecureStorage.SetAsync(AzureDeploymentKey, deployment.Trim());
        }
    }

    /// <summary>
    /// Gets the Azure OpenAI API key.
    /// </summary>
    public async Task<string?> GetAzureApiKeyAsync()
    {
        return await SecureStorage.GetAsync(AzureApiKeyKey);
    }

    /// <summary>
    /// Sets the Azure OpenAI API key.
    /// </summary>
    public async Task SetAzureApiKeyAsync(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            SecureStorage.Remove(AzureApiKeyKey);
        }
        else
        {
            await SecureStorage.SetAsync(AzureApiKeyKey, apiKey.Trim());
        }
    }
}


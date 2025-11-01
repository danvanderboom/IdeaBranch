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
    private const string LanguageKey = "app_language";
    private const string InAppNotificationsEnabledKey = "in_app_notifications_enabled";
    private const string PushNotificationsEnabledKey = "push_notifications_enabled";
    private const string ShowCommentsKey = "annotation_show_comments";

    // Export/Visualization settings
    private const string ExportDpiScaleKey = "export_dpi_scale";
    private const string ExportBackgroundColorKey = "export_background_color"; // hex like #FFFFFF
    private const string ExportIncludeLegendKey = "export_include_legend";
    private const string ExportFontFamilyKey = "export_font_family";
    private const string ExportPaletteKey = "export_palette"; // e.g., vibrant, pastel
    private const string ExportTransparentBackgroundKey = "export_transparent_bg";

    private const string DefaultProvider = "lmstudio";
    private const string DefaultLmStudioEndpoint = "http://localhost:1234/v1";
    private const string DefaultLmStudioModel = "phi-4";
    private const string DefaultLanguage = "system";
    private const int DefaultExportDpiScale = 1;
    private const string DefaultExportBackgroundColor = "#FFFFFF";
    private const bool DefaultExportIncludeLegend = true;
    private const string DefaultExportPalette = "vibrant";
    private const bool DefaultExportTransparentBackground = false;

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

    /// <summary>
    /// Gets the application language preference (values: "system", "en", "es", "fr").
    /// </summary>
    public async Task<string> GetLanguageAsync()
    {
        var language = await SecureStorage.GetAsync(LanguageKey);
        return language ?? DefaultLanguage;
    }

    /// <summary>
    /// Sets the application language preference.
    /// </summary>
    public async Task SetLanguageAsync(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be null or empty.", nameof(language));

        await SecureStorage.SetAsync(LanguageKey, language.Trim().ToLowerInvariant());
    }

    /// <summary>
    /// Gets whether in-app notifications are enabled (default: true).
    /// </summary>
    public async Task<bool> GetInAppNotificationsEnabledAsync()
    {
        var value = await SecureStorage.GetAsync(InAppNotificationsEnabledKey);
        if (value == null)
            return true; // Default to enabled
        return bool.TryParse(value, out var result) && result;
    }

    /// <summary>
    /// Sets whether in-app notifications are enabled.
    /// </summary>
    public async Task SetInAppNotificationsEnabledAsync(bool enabled)
    {
        await SecureStorage.SetAsync(InAppNotificationsEnabledKey, enabled.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Gets whether push notifications are enabled (default: false).
    /// </summary>
    public async Task<bool> GetPushNotificationsEnabledAsync()
    {
        var value = await SecureStorage.GetAsync(PushNotificationsEnabledKey);
        if (value == null)
            return false; // Default to disabled
        return bool.TryParse(value, out var result) && result;
    }

    /// <summary>
    /// Sets whether push notifications are enabled.
    /// </summary>
    public async Task SetPushNotificationsEnabledAsync(bool enabled)
    {
        await SecureStorage.SetAsync(PushNotificationsEnabledKey, enabled.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Gets whether annotation comments should be shown (default: true).
    /// </summary>
    public async Task<bool> GetShowCommentsAsync()
    {
        var value = await SecureStorage.GetAsync(ShowCommentsKey);
        if (value == null)
            return true; // Default to shown
        return bool.TryParse(value, out var result) && result;
    }

    /// <summary>
    /// Sets whether annotation comments should be shown.
    /// </summary>
    public async Task SetShowCommentsAsync(bool showComments)
    {
        await SecureStorage.SetAsync(ShowCommentsKey, showComments.ToString().ToLowerInvariant());
    }

    // Export/Visualization settings

    public async Task<int> GetExportDpiScaleAsync()
    {
        var value = await SecureStorage.GetAsync(ExportDpiScaleKey);
        return int.TryParse(value, out var scale) && scale >= 1 && scale <= 4 ? scale : DefaultExportDpiScale;
    }

    public async Task SetExportDpiScaleAsync(int scale)
    {
        if (scale < 1 || scale > 4) throw new ArgumentOutOfRangeException(nameof(scale), "DPI scale must be 1-4");
        await SecureStorage.SetAsync(ExportDpiScaleKey, scale.ToString());
    }

    public async Task<string> GetExportBackgroundColorAsync()
    {
        var value = await SecureStorage.GetAsync(ExportBackgroundColorKey);
        return string.IsNullOrWhiteSpace(value) ? DefaultExportBackgroundColor : value;
    }

    public async Task SetExportBackgroundColorAsync(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) throw new ArgumentException("Color cannot be empty", nameof(hex));
        await SecureStorage.SetAsync(ExportBackgroundColorKey, hex.Trim());
    }

    public async Task<bool> GetExportIncludeLegendAsync()
    {
        var value = await SecureStorage.GetAsync(ExportIncludeLegendKey);
        if (value == null) return DefaultExportIncludeLegend;
        return bool.TryParse(value, out var result) && result;
    }

    public async Task SetExportIncludeLegendAsync(bool includeLegend)
    {
        await SecureStorage.SetAsync(ExportIncludeLegendKey, includeLegend.ToString().ToLowerInvariant());
    }

    public async Task<string?> GetExportFontFamilyAsync()
    {
        return await SecureStorage.GetAsync(ExportFontFamilyKey);
    }

    public async Task SetExportFontFamilyAsync(string? fontFamily)
    {
        if (string.IsNullOrWhiteSpace(fontFamily))
        {
            SecureStorage.Remove(ExportFontFamilyKey);
        }
        else
        {
            await SecureStorage.SetAsync(ExportFontFamilyKey, fontFamily.Trim());
        }
    }

    public async Task<string> GetExportPaletteAsync()
    {
        var value = await SecureStorage.GetAsync(ExportPaletteKey);
        return string.IsNullOrWhiteSpace(value) ? DefaultExportPalette : value;
    }

    public async Task SetExportPaletteAsync(string palette)
    {
        if (string.IsNullOrWhiteSpace(palette)) throw new ArgumentException("Palette cannot be empty", nameof(palette));
        await SecureStorage.SetAsync(ExportPaletteKey, palette.Trim().ToLowerInvariant());
    }

    public async Task<bool> GetExportTransparentBackgroundAsync()
    {
        var value = await SecureStorage.GetAsync(ExportTransparentBackgroundKey);
        if (value == null) return DefaultExportTransparentBackground;
        return bool.TryParse(value, out var result) && result;
    }

    public async Task SetExportTransparentBackgroundAsync(bool isTransparent)
    {
        await SecureStorage.SetAsync(ExportTransparentBackgroundKey, isTransparent.ToString().ToLowerInvariant());
    }
}


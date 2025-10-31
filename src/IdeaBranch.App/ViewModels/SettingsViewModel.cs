using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.App.Resources;
using IdeaBranch.App.Services;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for SettingsPage that manages application settings across multiple categories.
/// </summary>
public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly SettingsService _settingsService;
    
    private string _selectedCategory;
    private string _selectedLanguage;
    private string _provider;
    private string _lmEndpoint;
    private string _lmModel;
    private string? _azureEndpoint;
    private string? _azureDeployment;
    private string? _azureApiKey;
    private bool _inAppNotificationsEnabled;
    private bool _pushNotificationsEnabled;
    private bool _isLoading;

    private static readonly IReadOnlyList<string> CategoryList = new[]
    {
        AppResources.SettingsCategory_User,
        AppResources.SettingsCategory_Project,
        AppResources.SettingsCategory_Display,
        AppResources.SettingsCategory_SearchFilter,
        AppResources.SettingsCategory_Integrations,
        AppResources.SettingsCategory_Notifications,
        AppResources.SettingsCategory_AiSafety,
        AppResources.SettingsCategory_ImportExport
    };
    
    // Internal category keys for converter matching (must match the original keys used in XAML)
    public static string GetCategoryKey(string localizedCategory)
    {
        return localizedCategory switch
        {
            var c when c == AppResources.SettingsCategory_User => "User",
            var c when c == AppResources.SettingsCategory_Project => "Project",
            var c when c == AppResources.SettingsCategory_Display => "Display",
            var c when c == AppResources.SettingsCategory_SearchFilter => "Search/Filter",
            var c when c == AppResources.SettingsCategory_Integrations => "Integrations",
            var c when c == AppResources.SettingsCategory_Notifications => "Notifications",
            var c when c == AppResources.SettingsCategory_AiSafety => "AI Safety",
            var c when c == AppResources.SettingsCategory_ImportExport => "Import/Export",
            _ => localizedCategory // Fallback to original value
        };
    }

    private static readonly IReadOnlyList<string> AvailableLanguages = new[]
    {
        "System",
        "English",
        "Spanish",
        "French"
    };

    private static readonly IReadOnlyList<string> ProviderList = new[]
    {
        "lmstudio",
        "azure"
    };

    /// <summary>
    /// Initializes a new instance with the settings service.
    /// </summary>
    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _selectedCategory = CategoryList[0]; // Default to first category
        
        // Initialize default values
        _selectedLanguage = AvailableLanguages[0];
        _provider = "lmstudio";
        _lmEndpoint = string.Empty;
        _lmModel = string.Empty;
        
        // Load settings asynchronously
        LoadSettingsAsync();
    }

    /// <summary>
    /// Gets the list of available setting categories.
    /// </summary>
    public IReadOnlyList<string> Categories => CategoryList;

    /// <summary>
    /// Gets the list of available language options.
    /// </summary>
    public IReadOnlyList<string> LanguageOptions => AvailableLanguages;

    /// <summary>
    /// Gets the list of available provider options.
    /// </summary>
    public IReadOnlyList<string> ProviderOptions => ProviderList;

    /// <summary>
    /// Gets or sets the currently selected category (localized display name).
    /// Internally converts to/from internal key for XAML converter matching.
    /// </summary>
    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                // Also notify about the internal key for converter matching
                OnPropertyChanged(nameof(SelectedCategoryKey));
            }
        }
    }
    
    /// <summary>
    /// Gets the internal key for the selected category (for XAML converter matching).
    /// </summary>
    public string SelectedCategoryKey => GetCategoryKey(_selectedCategory);

    /// <summary>
    /// Gets or sets the selected language.
    /// </summary>
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage != value)
            {
                _selectedLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
                SaveLanguageAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets the LLM provider ("lmstudio" or "azure").
    /// </summary>
    public string Provider
    {
        get => _provider;
        set
        {
            if (_provider != value)
            {
                _provider = value;
                OnPropertyChanged(nameof(Provider));
                SaveProviderAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets the LM Studio endpoint.
    /// </summary>
    public string LmEndpoint
    {
        get => _lmEndpoint;
        set
        {
            if (_lmEndpoint != value)
            {
                _lmEndpoint = value;
                OnPropertyChanged(nameof(LmEndpoint));
                SaveLmEndpointAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets the LM Studio model name.
    /// </summary>
    public string LmModel
    {
        get => _lmModel;
        set
        {
            if (_lmModel != value)
            {
                _lmModel = value;
                OnPropertyChanged(nameof(LmModel));
                SaveLmModelAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Azure OpenAI endpoint.
    /// </summary>
    public string? AzureEndpoint
    {
        get => _azureEndpoint;
        set
        {
            if (_azureEndpoint != value)
            {
                _azureEndpoint = value;
                OnPropertyChanged(nameof(AzureEndpoint));
                SaveAzureEndpointAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Azure OpenAI deployment name.
    /// </summary>
    public string? AzureDeployment
    {
        get => _azureDeployment;
        set
        {
            if (_azureDeployment != value)
            {
                _azureDeployment = value;
                OnPropertyChanged(nameof(AzureDeployment));
                SaveAzureDeploymentAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Azure OpenAI API key.
    /// </summary>
    public string? AzureApiKey
    {
        get => _azureApiKey;
        set
        {
            if (_azureApiKey != value)
            {
                _azureApiKey = value;
                OnPropertyChanged(nameof(AzureApiKey));
                SaveAzureApiKeyAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether in-app notifications are enabled.
    /// </summary>
    public bool InAppNotificationsEnabled
    {
        get => _inAppNotificationsEnabled;
        set
        {
            if (_inAppNotificationsEnabled != value)
            {
                _inAppNotificationsEnabled = value;
                OnPropertyChanged(nameof(InAppNotificationsEnabled));
                SaveInAppNotificationsEnabledAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether push notifications are enabled.
    /// </summary>
    public bool PushNotificationsEnabled
    {
        get => _pushNotificationsEnabled;
        set
        {
            if (_pushNotificationsEnabled != value)
            {
                _pushNotificationsEnabled = value;
                OnPropertyChanged(nameof(PushNotificationsEnabled));
                SavePushNotificationsEnabledAsync();
            }
        }
    }

    /// <summary>
    /// Gets whether settings are currently being loaded.
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
    /// Loads all settings from the service.
    /// </summary>
    private async void LoadSettingsAsync()
    {
        IsLoading = true;
        try
        {
            // Load language
            var languageCode = await _settingsService.GetLanguageAsync();
            _selectedLanguage = MapLanguageCodeToDisplay(languageCode);
            
            // Load provider settings
            _provider = await _settingsService.GetProviderAsync();
            _lmEndpoint = await _settingsService.GetLmStudioEndpointAsync();
            _lmModel = await _settingsService.GetLmStudioModelAsync();
            _azureEndpoint = await _settingsService.GetAzureEndpointAsync();
            _azureDeployment = await _settingsService.GetAzureDeploymentAsync();
            _azureApiKey = await _settingsService.GetAzureApiKeyAsync();
            _inAppNotificationsEnabled = await _settingsService.GetInAppNotificationsEnabledAsync();
            _pushNotificationsEnabled = await _settingsService.GetPushNotificationsEnabledAsync();
            
            // Notify property changes
            OnPropertyChanged(nameof(SelectedLanguage));
            OnPropertyChanged(nameof(Provider));
            OnPropertyChanged(nameof(LmEndpoint));
            OnPropertyChanged(nameof(LmModel));
            OnPropertyChanged(nameof(AzureEndpoint));
            OnPropertyChanged(nameof(AzureDeployment));
            OnPropertyChanged(nameof(AzureApiKey));
            OnPropertyChanged(nameof(InAppNotificationsEnabled));
            OnPropertyChanged(nameof(PushNotificationsEnabled));
        }
        catch
        {
            // Error handling - keep defaults
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Maps a language code to display name.
    /// </summary>
    private static string MapLanguageCodeToDisplay(string code)
    {
        return code?.ToLowerInvariant() switch
        {
            "system" => "System",
            "en" => "English",
            "es" => "Spanish",
            "fr" => "French",
            _ => "System"
        };
    }

    /// <summary>
    /// Maps a display name to language code.
    /// </summary>
    private static string MapDisplayToLanguageCode(string display)
    {
        return display switch
        {
            "System" => "system",
            "English" => "en",
            "Spanish" => "es",
            "French" => "fr",
            _ => "system"
        };
    }

    /// <summary>
    /// Saves the language preference.
    /// </summary>
    private async void SaveLanguageAsync()
    {
        try
        {
            var code = MapDisplayToLanguageCode(_selectedLanguage);
            await _settingsService.SetLanguageAsync(code);
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Saves the provider setting.
    /// </summary>
    private async void SaveProviderAsync()
    {
        try
        {
            await _settingsService.SetProviderAsync(_provider);
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Saves the LM Studio endpoint.
    /// </summary>
    private async void SaveLmEndpointAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_lmEndpoint))
            {
                await _settingsService.SetLmStudioEndpointAsync(_lmEndpoint);
            }
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Saves the LM Studio model.
    /// </summary>
    private async void SaveLmModelAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_lmModel))
            {
                await _settingsService.SetLmStudioModelAsync(_lmModel);
            }
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Saves the Azure endpoint.
    /// </summary>
    private async void SaveAzureEndpointAsync()
    {
        try
        {
            await _settingsService.SetAzureEndpointAsync(_azureEndpoint);
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Saves the Azure deployment.
    /// </summary>
    private async void SaveAzureDeploymentAsync()
    {
        try
        {
            await _settingsService.SetAzureDeploymentAsync(_azureDeployment);
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Saves the Azure API key.
    /// </summary>
    private async void SaveAzureApiKeyAsync()
    {
        try
        {
            await _settingsService.SetAzureApiKeyAsync(_azureApiKey);
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Saves the in-app notifications enabled setting.
    /// </summary>
    private async void SaveInAppNotificationsEnabledAsync()
    {
        try
        {
            await _settingsService.SetInAppNotificationsEnabledAsync(_inAppNotificationsEnabled);
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Saves the push notifications enabled setting.
    /// </summary>
    private async void SavePushNotificationsEnabledAsync()
    {
        try
        {
            await _settingsService.SetPushNotificationsEnabledAsync(_pushNotificationsEnabled);
        }
        catch
        {
            // Error handling
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


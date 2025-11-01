# Settings System

## Overview

The IdeaBranch application uses a centralized settings system managed by the `SettingsService` class. Settings are persisted securely using MAUI's `SecureStorage` API and organized into categories through the Settings UI.

## Architecture

### SettingsService

The `SettingsService` (`src/IdeaBranch.App/Services/SettingsService.cs`) is a singleton service that provides async methods for getting and setting application configuration values.

**Key characteristics:**
- All storage operations are asynchronous
- Sensitive data (API keys, endpoints) stored via `SecureStorage`
- Provides default values when settings are not set
- Input validation and normalization (trimming, case normalization)
- Throws `ArgumentException` for invalid inputs (non-nullable required fields)

### Registration

Registered in `MauiProgram.cs` as a singleton:

```csharp
builder.Services.AddSingleton<Services.SettingsService>();
```

### Dependencies

The `SettingsService` is consumed by:
- `SettingsViewModel` - For displaying and updating settings in the UI
- `LLMClientFactory` - For retrieving LLM provider configuration when creating clients

## Implemented Settings

### LLM Provider Settings

Settings related to Language Model integration configuration.

#### Provider Selection
- **Key**: `llm_provider`
- **Type**: `string`
- **Default**: `"lmstudio"`
- **Valid Values**: `"lmstudio"`, `"azure"`
- **Storage**: `SecureStorage`
- **Methods**: 
  - `GetProviderAsync()` - Returns current provider or default
  - `SetProviderAsync(string)` - Sets provider (throws if null/empty)

#### LM Studio Configuration
- **Endpoint**
  - **Key**: `lmstudio_endpoint`
  - **Type**: `string`
  - **Default**: `"http://localhost:1234/v1"`
  - **Storage**: `SecureStorage`
  - **Methods**: `GetLmStudioEndpointAsync()`, `SetLmStudioEndpointAsync(string)`
  
- **Model**
  - **Key**: `lmstudio_model`
  - **Type**: `string`
  - **Default**: `"phi-4"`
  - **Storage**: `SecureStorage`
  - **Methods**: `GetLmStudioModelAsync()`, `SetLmStudioModelAsync(string)`

#### Azure OpenAI Configuration
All Azure settings are optional (nullable).

- **Endpoint**
  - **Key**: `azure_endpoint`
  - **Type**: `string?`
  - **Default**: `null`
  - **Storage**: `SecureStorage`
  - **Methods**: `GetAzureEndpointAsync()`, `SetAzureEndpointAsync(string?)`
  - **Note**: Setting to null/empty removes the value

- **Deployment**
  - **Key**: `azure_deployment`
  - **Type**: `string?`
  - **Default**: `null`
  - **Storage**: `SecureStorage`
  - **Methods**: `GetAzureDeploymentAsync()`, `SetAzureDeploymentAsync(string?)`
  - **Note**: Setting to null/empty removes the value

- **API Key**
  - **Key**: `azure_api_key`
  - **Type**: `string?`
  - **Default**: `null`
  - **Storage**: `SecureStorage`
  - **Methods**: `GetAzureApiKeyAsync()`, `SetAzureApiKeyAsync(string?)`
  - **Note**: Setting to null/empty removes the value

### Application Language Settings

- **Key**: `app_language`
- **Type**: `string`
- **Default**: `"system"`
- **Valid Values**: `"system"`, `"en"`, `"es"`, `"fr"`
- **Storage**: `SecureStorage`
- **Methods**: 
  - `GetLanguageAsync()` - Returns language code or "system" default
  - `SetLanguageAsync(string)` - Sets language preference (throws if null/empty)
- **Display Mapping**: 
  - System → "System"
  - en → "English"
  - es → "Spanish"
  - fr → "French"

### Export / Visualization Settings

Settings controlling visualization exports (Word Cloud, Timeline, Map).

- **DPI Scale**
  - **Key**: `export_dpi_scale`
  - **Type**: `int` (1–4)
  - **Default**: `1`
  - **Methods**: `GetExportDpiScaleAsync()`, `SetExportDpiScaleAsync(int)`

- **Background Color**
  - **Key**: `export_background_color`
  - **Type**: `string` (HEX, e.g. `#FFFFFF`)
  - **Default**: `#FFFFFF`
  - **Methods**: `GetExportBackgroundColorAsync()`, `SetExportBackgroundColorAsync(string)`

- **Include Legend**
  - **Key**: `export_include_legend`
  - **Type**: `bool`
  - **Default**: `true`
  - **Methods**: `GetExportIncludeLegendAsync()`, `SetExportIncludeLegendAsync(bool)`

- **Font Family**
  - **Key**: `export_font_family`
  - **Type**: `string?`
  - **Default**: `null`
  - **Methods**: `GetExportFontFamilyAsync()`, `SetExportFontFamilyAsync(string?)`

- **Palette**
  - **Key**: `export_palette`
  - **Type**: `string` (e.g., `vibrant`, `pastel`)
  - **Default**: `vibrant`
  - **Methods**: `GetExportPaletteAsync()`, `SetExportPaletteAsync(string)`

- **Transparent Background**
  - **Key**: `export_transparent_bg`
  - **Type**: `bool`
  - **Default**: `false`
  - **Methods**: `GetExportTransparentBackgroundAsync()`, `SetExportTransparentBackgroundAsync(bool)`

## Settings UI

### SettingsPage

The settings UI (`src/IdeaBranch.App/Views/SettingsPage.xaml`) provides a category-based interface for managing settings.

**Layout:**
- Left sidebar: Category list (240px width)
- Right panel: Category-specific settings (scrollable)

**Categories:**
1. **User** - Placeholder (coming soon)
2. **Project** - Placeholder (coming soon)
3. **Display** - Language picker (implemented)
4. **Search/Filter** - Placeholder (coming soon)
5. **Integrations** - LLM provider configuration (implemented)
6. **AI Safety** - Placeholder (coming soon)
7. **Import/Export** - Placeholder (coming soon)

### SettingsViewModel

The `SettingsViewModel` (`src/IdeaBranch.App/ViewModels/SettingsViewModel.cs`) manages the UI state and two-way binding with the `SettingsService`.

**Features:**
- `INotifyPropertyChanged` for MVVM binding
- Auto-saves settings when properties change
- Loads settings asynchronously on initialization
- Maps between display names and storage codes (e.g., "English" ↔ "en")
- `IsLoading` property for UI feedback during async operations

**Property-to-Service Mapping:**
- `SelectedCategory` → UI state only
- `SelectedLanguage` → `SetLanguageAsync()` (auto-saves)
- `Provider` → `SetProviderAsync()` (auto-saves)
- `LmEndpoint` → `SetLmStudioEndpointAsync()` (auto-saves)
- `LmModel` → `SetLmStudioModelAsync()` (auto-saves)
- `AzureEndpoint` → `SetAzureEndpointAsync()` (auto-saves)
- `AzureDeployment` → `SetAzureDeploymentAsync()` (auto-saves)
- `AzureApiKey` → `SetAzureApiKeyAsync()` (auto-saves)

**Registration:**
Registered as transient in `MauiProgram.cs`:

```csharp
builder.Services.AddTransient<SettingsViewModel>(sp =>
{
    var settingsService = sp.GetRequiredService<Services.SettingsService>();
    return new SettingsViewModel(settingsService);
});
```

## Storage Mechanism

All settings are stored using MAUI's `SecureStorage` API (`Microsoft.Maui.Storage.SecureStorage`).

**Characteristics:**
- **Platform-specific secure storage**:
  - iOS: Keychain
  - Android: EncryptedSharedPreferences
  - Windows: DataProtection API
  - macOS: Keychain
- **Key-value storage**: Each setting has a unique string key
- **Async operations**: All storage operations are asynchronous
- **Data protection**: Sensitive data (API keys, endpoints) is encrypted at rest

**Storage Keys:**
- `llm_provider`
- `lmstudio_endpoint`
- `lmstudio_model`
- `azure_endpoint`
- `azure_deployment`
- `azure_api_key`
- `app_language`

## Usage Patterns

### Reading Settings

```csharp
var settingsService = serviceProvider.GetRequiredService<SettingsService>();
var provider = await settingsService.GetProviderAsync();
var endpoint = await settingsService.GetLmStudioEndpointAsync();
```

### Writing Settings

```csharp
var settingsService = serviceProvider.GetRequiredService<SettingsService>();
await settingsService.SetProviderAsync("azure");
await settingsService.SetLmStudioEndpointAsync("http://localhost:1234/v1");
await settingsService.SetLanguageAsync("en");
```

### In Dependency Injection

Services can receive `SettingsService` via constructor injection:

```csharp
public class MyService
{
    private readonly SettingsService _settings;
    
    public MyService(SettingsService settings)
    {
        _settings = settings;
    }
    
    public async Task DoWorkAsync()
    {
        var language = await _settings.GetLanguageAsync();
        // ...
    }
}
```

### From SettingsViewModel (UI)

The ViewModel automatically loads and saves settings. Properties can be bound in XAML:

```xml
<Picker SelectedItem="{Binding SelectedLanguage}" 
        ItemsSource="{Binding LanguageOptions}" />
```

## Default Values

When a setting is not configured, the service returns default values:

| Setting | Default Value |
|---------|---------------|
| Provider | `"lmstudio"` |
| LM Studio Endpoint | `"http://localhost:1234/v1"` |
| LM Studio Model | `"phi-4"` |
| Language | `"system"` |
| Azure Endpoint | `null` |
| Azure Deployment | `null` |
| Azure API Key | `null` |

## Integration Points

### LLMClientFactory

The `LLMClientFactory` (`src/IdeaBranch.App/Services/LLM/LLMClientFactory.cs`) uses `SettingsService` to create LLM clients:

1. Reads provider setting via `GetProviderAsync()`
2. Based on provider, reads provider-specific settings
3. Creates appropriate client (LmStudioClient or AzureOpenAIClient)
4. Throws `InvalidOperationException` if Azure settings are incomplete

**Error handling:**
- Missing Azure endpoint/deployment throws `InvalidOperationException` with descriptive message
- Unknown provider throws `InvalidOperationException`

## Testing

### UI Tests

UI tests (`tests/IdeaBranch.UITests/SettingsTests.cs`) cover:
- Settings page display and navigation
- Category selection
- Language picker presence and persistence
- Integrations settings display

### Test Coverage

Tests verify:
- **SETTINGS-001**: View settings categories
- **SETTINGS-002**: Navigate between categories
- **SETTINGS-003**: Language picker exists
- **SETTINGS-004**: Language selection persists
- **SETTINGS-005**: Integrations settings accessible

## Future Enhancements

Based on the specification (`openspec/specs/settings/spec.md`) and product documentation, the following settings categories are planned but not yet implemented:

### User Settings
- Account information
- Password management
- Notification preferences

### Project Settings
- Project details
- Collaborators and permissions
- Tag taxonomies
- Prompt templates

### Display Settings
- Theme and appearance (only language is implemented)
- Layout preferences
- Timeline display options
- Hierarchical view options

### Search/Filter Settings
- Default search parameters
- Tag-based filtering preferences
- Saved searches and filters

### AI Safety Settings
- Content filtering options
- Language model evaluation thresholds
- Custom blacklist/whitelist for language models

### Import/Export Settings
- Default import and export formats
- Document source configuration
- Automatic backup preferences

## Best Practices

1. **Always use async methods**: All settings operations are asynchronous
2. **Handle defaults**: Methods return defaults when values aren't set
3. **Validate inputs**: Service validates inputs, but consumers should handle exceptions
4. **Secure storage**: All settings use `SecureStorage` - no separate sensitive/non-sensitive distinction
5. **ViewModel auto-save**: SettingsViewModel auto-saves on property change - no manual save needed
6. **Language mapping**: Use display names in UI, storage uses codes (handled by ViewModel)

## Related Documentation

- **Product Specification**: `docs/product/IdeaBranch Product.txt` (Settings section)
- **Settings Spec**: `openspec/specs/settings/spec.md`
- **Settings Service**: `src/IdeaBranch.App/Services/SettingsService.cs`
- **Settings ViewModel**: `src/IdeaBranch.App/ViewModels/SettingsViewModel.cs`
- **Settings Page**: `src/IdeaBranch.App/Views/SettingsPage.xaml`
- **UI Tests**: `tests/IdeaBranch.UITests/SettingsTests.cs`


# Docs

- product/: primary product source doc
- testing/: UI automation guidance
- process/: build and dev setup

## Product Documentation

The primary product source document is located at:
- **`docs/product/IdeaBranch Product.txt`** - Complete product definition including features, requirements, and usage scenarios

## Application Settings

### LLM Provider Configuration

IdeaBranch supports multiple Large Language Model (LLM) providers:

#### LM Studio (Local)
- **Endpoint**: Default `http://localhost:1234/v1`
- **Model**: Configurable (default: `lmstudio-community/phi-2-ggml`)
- **API Key**: Optional (default: `lm-studio`)
- **Storage**: API key stored securely via `SecureStorage`

#### Azure OpenAI
- **Endpoint**: Your Azure OpenAI endpoint URL
- **Deployment**: Your Azure OpenAI deployment name
- **API Key**: Optional (uses Azure AD authentication if not provided)
- **Storage**: API key stored securely via `SecureStorage`

### Settings Management

Settings are managed via `SettingsService` in the app:
- LLM provider selection
- Endpoint configuration
- Model/deployment names
- API keys (stored securely using `SecureStorage`)

Settings are persisted using:
- **Preferences** (`Microsoft.Maui.Storage.Preferences`) for non-sensitive data
- **SecureStorage** (`Microsoft.Maui.Storage.SecureStorage`) for API keys and sensitive data

### Database

The application uses SQLite for local data persistence:
- **Database Path**: `{AppDataDirectory}/ideabranch.db`
- **Schema Versioning**: Managed by `TopicDb` with automatic migrations
- **Repository**: `SqliteTopicTreeRepository` implements `ITopicTreeRepository`

# Docs

- product/: primary product source doc
- testing/: UI automation guidance
- process/: build and dev setup
- development/: developer documentation for implemented features

## Product Documentation

The primary product source document is located at:
- **`docs/product/IdeaBranch Product.txt`** - Complete product definition including features, requirements, and usage scenarios

## Application Settings

For detailed documentation on the settings system, see:
- **`docs/development/settings.md`** - Complete settings architecture, implementation details, and usage patterns

### Quick Reference

Settings are managed via `SettingsService` and persisted using `SecureStorage`:
- LLM provider selection (LM Studio or Azure OpenAI)
- Provider-specific configuration (endpoints, models, API keys)
- Application language preference

All settings are stored securely via `Microsoft.Maui.Storage.SecureStorage`.

### Database

The application uses SQLite for local data persistence:
- **Database Path**: `{AppDataDirectory}/ideabranch.db`
- **Schema Versioning**: Managed by `TopicDb` with automatic migrations
- **Repository**: `SqliteTopicTreeRepository` implements `ITopicTreeRepository`

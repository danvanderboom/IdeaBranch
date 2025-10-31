## Why
The settings spec currently only specifies basic categories, language settings, and notification preferences. The product documentation describes comprehensive settings categories and subcategories including User Settings, Project Settings, Display Settings, Search and Filter Settings, Integration Settings, AI Safety Settings, and Import/Export Settings that need to be fully specified.

## What Changes
- Expand `settings` spec with ADDED requirements
- Specify requirements for all User Settings subcategories (account, password, notifications, language)
- Specify requirements for all Project Settings subcategories (details, collaborators, taxonomies, templates)
- Specify requirements for all Display Settings subcategories (theme, layout, timeline options, hierarchical view)
- Specify requirements for all Search/Filter Settings subcategories (defaults, preferences, saved searches)
- Specify requirements for all Integration Settings subcategories (external tools, calendar, LLM API)
- Specify requirements for all AI Safety Settings subcategories (content filtering, thresholds, blacklist/whitelist)
- Specify requirements for all Import/Export Settings subcategories (formats, document sources, backups)

## Impact
- Affected specs: settings (ADDED requirements)
- Affected code: Settings UI (partially implemented - this expands spec coverage)


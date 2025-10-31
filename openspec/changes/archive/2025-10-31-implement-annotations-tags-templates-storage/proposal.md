## Why
Implement storage persistence for annotations, tag taxonomies, and prompt templates as specified in the data-storage spec. These features are currently missing and are required for the application to support annotation tagging, hierarchical tag taxonomies, and prompt template collections.

## What Changes
- Create domain models: Annotation, TagTaxonomyNode, PromptTemplate
- Create repository interfaces: IAnnotationsRepository, ITagTaxonomyRepository, IPromptTemplateRepository
- Implement SQLite repositories: SqliteAnnotationsRepository, SqliteTagTaxonomyRepository, SqlitePromptTemplateRepository
- Add database migrations (v4, v5, v6) in TopicDb for:
  - annotations table (with text span references)
  - annotation_tags junction table (many-to-many)
  - annotation_values table (for numeric/geospatial/temporal data)
  - tag_taxonomy_nodes table (hierarchical)
  - prompt_templates table (hierarchical)
- Register repositories in MauiProgram.cs dependency injection

## Impact
- Affected specs: data-storage (implementing ADDED requirements)
- Affected code:
  - `src/IdeaBranch.Domain/` (new domain models and repository interfaces)
  - `src/IdeaBranch.Infrastructure/Storage/` (new SQLite repository implementations)
  - `src/IdeaBranch.Infrastructure/Storage/TopicDb.cs` (migrations v4-v6)
  - `src/IdeaBranch.App/MauiProgram.cs` (repository registration)
  - Tests: integration tests for new repositories


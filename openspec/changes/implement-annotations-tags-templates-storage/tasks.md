## 1. Domain Models
- [x] 1.1 Create Annotation domain model with NodeId, text span (StartOffset/EndOffset), Comment, timestamps
- [x] 1.2 Create TagTaxonomyNode domain model with hierarchical structure (ParentId, Name, Order)
- [x] 1.3 Create PromptTemplate domain model with hierarchical structure (ParentId, Name, Body/Content, Order)

## 2. Repository Interfaces
- [x] 2.1 Create IAnnotationsRepository interface with Save, GetByNodeId, GetById, Delete, QueryByTag methods
- [x] 2.2 Create ITagTaxonomyRepository interface with Save, GetRoot, GetById, GetChildren, Delete methods
- [x] 2.3 Create IPromptTemplateRepository interface with Save, GetRoot, GetById, GetChildren, GetByPath, Delete methods

## 3. Database Migrations
- [x] 3.1 Add migration v4 in TopicDb: Create annotations, annotation_tags, annotation_values tables
- [x] 3.2 Add migration v5 in TopicDb: Create tag_taxonomy_nodes table (hierarchical)
- [x] 3.3 Add migration v6 in TopicDb: Create prompt_templates table (hierarchical)
- [x] 3.4 Update CurrentSchemaVersion to 6 in TopicDb

## 4. SQLite Repository Implementations
- [x] 4.1 Implement SqliteAnnotationsRepository with all CRUD operations and tag querying
- [x] 4.2 Implement SqliteTagTaxonomyRepository with hierarchical tree operations
- [x] 4.3 Implement SqlitePromptTemplateRepository with hierarchical tree operations

## 5. Dependency Injection
- [x] 5.1 Register IAnnotationsRepository in MauiProgram.cs
- [x] 5.2 Register ITagTaxonomyRepository in MauiProgram.cs
- [x] 5.3 Register IPromptTemplateRepository in MauiProgram.cs

## 6. Testing
- [x] 6.1 Write integration tests for SqliteAnnotationsRepository
- [x] 6.2 Write integration tests for SqliteTagTaxonomyRepository
- [x] 6.3 Write integration tests for SqlitePromptTemplateRepository


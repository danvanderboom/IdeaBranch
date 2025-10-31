## 1. Domain Models
- [ ] 1.1 Create Annotation domain model with NodeId, text span (StartOffset/EndOffset), Comment, timestamps
- [ ] 1.2 Create TagTaxonomyNode domain model with hierarchical structure (ParentId, Name, Order)
- [ ] 1.3 Create PromptTemplate domain model with hierarchical structure (ParentId, Name, Body/Content, Order)

## 2. Repository Interfaces
- [ ] 2.1 Create IAnnotationsRepository interface with Save, GetByNodeId, GetById, Delete, QueryByTag methods
- [ ] 2.2 Create ITagTaxonomyRepository interface with Save, GetRoot, GetById, GetChildren, Delete methods
- [ ] 2.3 Create IPromptTemplateRepository interface with Save, GetRoot, GetById, GetChildren, GetByPath, Delete methods

## 3. Database Migrations
- [ ] 3.1 Add migration v4 in TopicDb: Create annotations, annotation_tags, annotation_values tables
- [ ] 3.2 Add migration v5 in TopicDb: Create tag_taxonomy_nodes table (hierarchical)
- [ ] 3.3 Add migration v6 in TopicDb: Create prompt_templates table (hierarchical)
- [ ] 3.4 Update CurrentSchemaVersion to 6 in TopicDb

## 4. SQLite Repository Implementations
- [ ] 4.1 Implement SqliteAnnotationsRepository with all CRUD operations and tag querying
- [ ] 4.2 Implement SqliteTagTaxonomyRepository with hierarchical tree operations
- [ ] 4.3 Implement SqlitePromptTemplateRepository with hierarchical tree operations

## 5. Dependency Injection
- [ ] 5.1 Register IAnnotationsRepository in MauiProgram.cs
- [ ] 5.2 Register ITagTaxonomyRepository in MauiProgram.cs
- [ ] 5.3 Register IPromptTemplateRepository in MauiProgram.cs

## 6. Testing
- [ ] 6.1 Write integration tests for SqliteAnnotationsRepository
- [ ] 6.2 Write integration tests for SqliteTagTaxonomyRepository
- [ ] 6.3 Write integration tests for SqlitePromptTemplateRepository


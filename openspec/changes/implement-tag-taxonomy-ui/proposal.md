## Why
Implement the Tag Taxonomy UI as specified in the tag-taxonomy-ui spec. The storage layer is complete, but users need a UI to view, create, edit, delete, reorder, and manage tag taxonomies. This is foundational for the annotations feature, which requires tags from the taxonomy.

## What Changes
- Create TagTaxonomyAdapter to bridge TagTaxonomyNode domain model to hierarchical tree system (ITreeNode)
- Create TagTaxonomyViewProvider similar to TopicTreeViewProvider for tree view management
- Create TagTaxonomyViewModel with CRUD operations:
  - Load taxonomy tree from repository
  - Create/edit/delete categories and tags
  - Reorder nodes (up/down controls for MVP)
  - Move nodes between categories
  - Check for annotation references before deletion
- Create TagTaxonomyPage.xaml with hierarchical tree view (similar to TopicTreePage)
- Create TagTaxonomyEditDialog for create/edit operations
- Add TagTaxonomyPage to AppShell navigation
- Register TagTaxonomyViewModel in MauiProgram.cs
- Implement basic import/export functionality (JSON format)
- Add AI-assisted taxonomy generation (if LLM service available)

## Impact
- Affected specs: tag-taxonomy-ui (implementing existing requirements)
- Affected code:
  - `src/IdeaBranch.App/Adapters/` (new TagTaxonomyAdapter)
  - `src/IdeaBranch.App/ViewModels/TagTaxonomyViewModel.cs` (new)
  - `src/IdeaBranch.App/Views/TagTaxonomyPage.xaml` (new)
  - `src/IdeaBranch.App/Views/TagTaxonomyPage.xaml.cs` (new)
  - `src/IdeaBranch.App/Views/TagTaxonomyEditDialog.xaml` (new, optional ContentView)
  - `src/IdeaBranch.App/AppShell.xaml` (add navigation item)
  - `src/IdeaBranch.App/MauiProgram.cs` (register ViewModel)
  - Tests: UI tests for Tag Taxonomy page (future work)


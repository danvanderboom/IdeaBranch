## 1. Adapter and View Provider
- [x] 1.1 Create TagTaxonomyAdapter to convert TagTaxonomyNode to ITreeNode with payload
- [x] 1.2 Create TagTaxonomyViewProvider similar to TopicTreeViewProvider

## 2. ViewModel
- [x] 2.1 Create TagTaxonomyViewModel with ITagTaxonomyRepository dependency
- [x] 2.2 Implement LoadTaxonomyAsync to load root and build tree
- [x] 2.3 Implement CreateCategoryAsync and CreateTagAsync methods
- [x] 2.4 Implement EditNodeAsync method (update name)
- [x] 2.5 Implement DeleteNodeAsync with annotation reference check
- [x] 2.6 Implement ReorderNodeAsync (up/down within siblings)
- [x] 2.7 Implement MoveNodeAsync (change parent)

## 3. UI - Main Page
- [x] 3.1 Create TagTaxonomyPage.xaml with CollectionView for tree display
- [x] 3.2 Create TagTaxonomyPage.xaml.cs with ViewModel binding
- [x] 3.3 Add expand/collapse visual indicators
- [x] 3.4 Add depth-based indentation for hierarchy
- [x] 3.5 Add context menu or buttons for create/edit/delete/reorder/move

## 4. UI - Edit Dialog
- [x] 4.1 Create TagTaxonomyEditDialog (ContentView or separate page) - using DisplayPromptAsync
- [x] 4.2 Add name input field
- [x] 4.3 Add save/cancel buttons
- [x] 4.4 Integrate with ViewModel for create/edit

## 5. Navigation and Registration
- [x] 5.1 Add TagTaxonomyPage to AppShell.xaml navigation
- [x] 5.2 Register TagTaxonomyViewModel in MauiProgram.cs DI container

## 6. Advanced Features
- [x] 6.1 Implement export to JSON functionality
- [x] 6.2 Implement import from JSON functionality
- [ ] 6.3 Add AI-assisted taxonomy generation (if LLM service available) - Future enhancement

## 7. Testing and Polish
- [x] 7.1 Test create category/tag flow
- [x] 7.2 Test edit node flow
- [x] 7.3 Test delete with annotation reference check
- [x] 7.4 Test reorder functionality
- [x] 7.5 Test move between categories
- [x] 7.6 Add AutomationIds for UI testing


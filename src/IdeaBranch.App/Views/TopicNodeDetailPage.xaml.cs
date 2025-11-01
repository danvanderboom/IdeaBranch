using System;
using System.Linq;
using IdeaBranch.App.ViewModels;
using IdeaBranch.Domain;
using Microsoft.Maui.Controls;

namespace IdeaBranch.App.Views;

public partial class TopicNodeDetailPage : ContentPage
{
    private readonly TopicNodeDetailViewModel _viewModel = null!;

    public TopicNodeDetailPage(TopicNodeDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Add property to ViewModel for text selection tracking
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TopicNodeDetailViewModel.Response))
                {
                    UpdateTextSelection();
                }
            };
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        await _viewModel.SaveAsync();
        
        // Navigate back
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
        else
        {
            // If no navigation stack, navigate to TopicTreePage
            await Shell.Current.GoToAsync("//TopicTreePage");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        // Navigate back without saving
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
        else
        {
            // If no navigation stack, navigate to TopicTreePage
            await Shell.Current.GoToAsync("//TopicTreePage");
        }
    }

    private async void OnGenerateResponseClicked(object? sender, EventArgs e)
    {
        await _viewModel.GenerateResponseAsync();
    }

    private async void OnGenerateTitleClicked(object? sender, EventArgs e)
    {
        await _viewModel.GenerateTitleAsync();
    }

    private async void OnRetryClicked(object? sender, EventArgs e)
    {
        await _viewModel.RetryAsync();
    }

    private async void OnViewHistoryClicked(object? sender, EventArgs e)
    {
        // Get node ID from ViewModel
        var nodeId = _viewModel.NodeId;
        if (nodeId == Guid.Empty)
            return;

        // Get version history repository from services
        var versionHistoryRepository = Handler?.MauiContext?.Services?.GetService<Domain.IVersionHistoryRepository>();
        if (versionHistoryRepository == null)
            return;

        // Create ViewModel and navigate to VersionHistoryPage
        var historyViewModel = new VersionHistoryViewModel(nodeId, versionHistoryRepository, _viewModel.Title);
        var historyPage = new VersionHistoryPage(historyViewModel);
        
        await Navigation.PushAsync(historyPage);
    }

    private void OnResponseTextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateTextSelection();
    }

    private void OnResponseFocused(object? sender, FocusEventArgs e)
    {
        // Check for text selection when editor gains focus
        // Note: Selection tracking requires platform-specific implementation
        UpdateTextSelection();
    }

    private void UpdateTextSelection()
    {
        // Update selection from Editor using platform handler if available
        // For now, we'll use a platform handler approach
        // The handler will set SelectionStart and SelectionLength on the ViewModel
        if (ResponseEditor?.Handler?.PlatformView != null)
        {
            // Platform-specific code would go here
            // For now, we'll provide a fallback that checks if text is selected via a different method
        }
    }

    private async void OnAnnotateSelectionClicked(object? sender, EventArgs e)
    {
        var response = _viewModel.Response ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(response))
        {
            await DisplayAlert("No Text Selected", "Please select text in the response field to annotate.", "OK");
            return;
        }

        // Use the selection from ViewModel if available
        var startOffset = _viewModel.SelectionStart;
        var endOffset = _viewModel.SelectionEnd;
        var selectedText = _viewModel.SelectedText;

        // If no selection, prompt user to select text first
        if (string.IsNullOrWhiteSpace(selectedText) || startOffset < 0 || endOffset <= startOffset)
        {
            await DisplayAlert("No Text Selected", "Please select text in the response field to annotate.", "OK");
            return;
        }

        // Get services
        var services = Handler?.MauiContext?.Services;
        var annotationsRepository = services?.GetService<Domain.IAnnotationsRepository>();
        var tagTaxonomyRepository = services?.GetService<Domain.ITagTaxonomyRepository>();

        if (annotationsRepository == null)
        {
            await DisplayAlert("Error", "Annotations repository is not available.", "OK");
            return;
        }

        // Create new annotation
        var annotationViewModel = new AnnotationEditViewModel(
            _viewModel.NodeId,
            startOffset,
            endOffset,
            selectedText,
            annotationsRepository,
            tagTaxonomyRepository,
            async () => await _viewModel.LoadAnnotationsAsync());

        var annotationPage = new AnnotationEditPage(annotationViewModel);
        await Navigation.PushAsync(annotationPage);
    }

    private void OnAnnotationSelected(object? sender, SelectionChangedEventArgs e)
    {
        // Handle annotation selection if needed
    }

    private async void OnTagFilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (TagFilterEntry?.Text == null || string.IsNullOrWhiteSpace(TagFilterEntry.Text))
        {
            // Clear filters if text is empty
            _viewModel.SelectedTagFilters = Array.Empty<Guid>();
            return;
        }

        // Find tags matching the filter text
        var services = Handler?.MauiContext?.Services;
        var tagTaxonomyRepository = services?.GetService<Domain.ITagTaxonomyRepository>();
        if (tagTaxonomyRepository == null)
            return;

        try
        {
            var root = await tagTaxonomyRepository.GetRootAsync();
            var matchingTagIds = new List<Guid>();
            await FindMatchingTagsAsync(root, TagFilterEntry.Text, matchingTagIds, tagTaxonomyRepository);
            _viewModel.SelectedTagFilters = matchingTagIds;
        }
        catch
        {
            // Silently fail - filtering is optional
        }
    }

    private async Task FindMatchingTagsAsync(
        Domain.TagTaxonomyNode node,
        string filterText,
        List<Guid> results,
        Domain.ITagTaxonomyRepository repository)
    {
        if (node.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase))
        {
            results.Add(node.Id);
        }

        var children = await repository.GetChildrenAsync(node.Id);
        foreach (var child in children)
        {
            await FindMatchingTagsAsync(child, filterText, results, repository);
        }
    }

    private void OnClearFiltersClicked(object? sender, EventArgs e)
    {
        TagFilterEntry.Text = string.Empty;
        _viewModel.SelectedTagFilters = Array.Empty<Guid>();
    }

    private async void OnAnnotationTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not Annotation annotation)
            return;

        // Get selected text span
        var response = _viewModel.Response ?? string.Empty;
        var startOffset = Math.Min(annotation.StartOffset, response.Length);
        var endOffset = Math.Min(annotation.EndOffset, response.Length);
        var selectedText = startOffset < endOffset && endOffset <= response.Length
            ? response.Substring(startOffset, endOffset - startOffset)
            : string.Empty;

        // Get services
        var services = Handler?.MauiContext?.Services;
        var annotationsRepository = services?.GetService<Domain.IAnnotationsRepository>();
        var tagTaxonomyRepository = services?.GetService<Domain.ITagTaxonomyRepository>();

        if (annotationsRepository == null)
        {
            await DisplayAlert("Error", "Annotations repository is not available.", "OK");
            return;
        }

        // Edit existing annotation
        var annotationViewModel = new AnnotationEditViewModel(
            annotation,
            selectedText,
            annotationsRepository,
            tagTaxonomyRepository,
            async () => await _viewModel.LoadAnnotationsAsync());

        var annotationPage = new AnnotationEditPage(annotationViewModel);
        await Navigation.PushAsync(annotationPage);
    }
}


using System;
using System.Linq;
using IdeaBranch.App.ViewModels;
using IdeaBranch.Domain;
using Microsoft.Maui.Controls;

namespace IdeaBranch.App.Views;

public partial class TopicNodeDetailPage : ContentPage
{
    private readonly TopicNodeDetailViewModel _viewModel;

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
        // Note: .NET MAUI Editor doesn't expose SelectionStart/SelectionLength directly
        // This is a simplified implementation - full selection tracking would require
        // platform-specific handlers or a custom control
        // For now, we'll detect selection changes through a workaround or platform handlers
    }

    private async void OnAnnotateSelectionClicked(object? sender, EventArgs e)
    {
        // Get selection from Editor
        // Note: This requires platform-specific code to get SelectionStart/SelectionLength
        // For now, we'll show a prompt to select text
        var response = _viewModel.Response ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(response))
        {
            await DisplayAlert("No Text Selected", "Please select text in the response field to annotate.", "OK");
            return;
        }

        // For now, we'll use the entire response length as a placeholder
        // In a real implementation, we'd get SelectionStart and SelectionLength from the Editor
        var startOffset = 0;
        var endOffset = Math.Min(10, response.Length); // Placeholder: first 10 characters
        
        var selectedText = response.Substring(startOffset, Math.Min(endOffset - startOffset, response.Length - startOffset));

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


using System;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.App.Services.Search;
using IdeaBranch.App.ViewModels;
using IdeaBranch.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace IdeaBranch.App.Views;

public partial class AdvancedSearchPage : ContentPage
{
    private readonly AdvancedSearchViewModel _viewModel;

    public AdvancedSearchPage()
    {
        InitializeComponent();
        
        // Get ViewModel from DI container
        var services = Handler?.MauiContext?.Services ?? throw new InvalidOperationException("Services not available");
        _viewModel = services.GetRequiredService<AdvancedSearchViewModel>();
        BindingContext = _viewModel;
    }

    public AdvancedSearchPage(AdvancedSearchViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Sync checkboxes with viewmodel content types
        CheckTopicNodes.IsChecked = _viewModel.SelectedContentTypes.Contains(SearchContentType.TopicNodes);
        CheckAnnotations.IsChecked = _viewModel.SelectedContentTypes.Contains(SearchContentType.Annotations);
        CheckTags.IsChecked = _viewModel.SelectedContentTypes.Contains(SearchContentType.Tags);
        CheckTemplates.IsChecked = _viewModel.SelectedContentTypes.Contains(SearchContentType.PromptTemplates);
        
        // Sync DatePickers with viewmodel
        if (_viewModel.UpdatedAtFrom.HasValue)
            UpdatedAtFromPicker.Date = _viewModel.UpdatedAtFrom.Value;
        if (_viewModel.UpdatedAtTo.HasValue)
            UpdatedAtToPicker.Date = _viewModel.UpdatedAtTo.Value;
        if (_viewModel.TemporalStart.HasValue)
            TemporalStartPicker.Date = _viewModel.TemporalStart.Value;
        if (_viewModel.TemporalEnd.HasValue)
            TemporalEndPicker.Date = _viewModel.TemporalEnd.Value;
    }

    private void OnUpdatedAtFromDateSelected(object? sender, DateChangedEventArgs e)
    {
        _viewModel.UpdatedAtFrom = e.NewDate;
    }

    private void OnUpdatedAtToDateSelected(object? sender, DateChangedEventArgs e)
    {
        _viewModel.UpdatedAtTo = e.NewDate;
    }

    private void OnTemporalStartDateSelected(object? sender, DateChangedEventArgs e)
    {
        _viewModel.TemporalStart = e.NewDate;
    }

    private void OnTemporalEndDateSelected(object? sender, DateChangedEventArgs e)
    {
        _viewModel.TemporalEnd = e.NewDate;
    }

    private void OnContentTypeCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;

        var contentType = checkBox.AutomationId switch
        {
            "AdvancedSearchPage_CheckTopicNodes" => SearchContentType.TopicNodes,
            "AdvancedSearchPage_CheckAnnotations" => SearchContentType.Annotations,
            "AdvancedSearchPage_CheckTags" => SearchContentType.Tags,
            "AdvancedSearchPage_CheckTemplates" => SearchContentType.PromptTemplates,
            _ => (SearchContentType?)null
        };

        if (contentType.HasValue)
        {
            if (e.Value)
            {
                _viewModel.AddContentType(contentType.Value);
            }
            else
            {
                _viewModel.RemoveContentType(contentType.Value);
            }
        }
    }

    private async void OnIncludeTagsClicked(object? sender, EventArgs e)
    {
        // TODO: Open TagPicker dialog
        // For now, just show a placeholder
        await DisplayAlert("Tag Picker", "Tag picker dialog will be implemented", "OK");
    }

    private async void OnExcludeTagsClicked(object? sender, EventArgs e)
    {
        // TODO: Open TagPicker dialog
        // For now, just show a placeholder
        await DisplayAlert("Tag Picker", "Tag picker dialog will be implemented", "OK");
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        _viewModel.ClearFilters();
        
        // Reset checkboxes
        CheckTopicNodes.IsChecked = false;
        CheckAnnotations.IsChecked = false;
        CheckTags.IsChecked = false;
        CheckTemplates.IsChecked = false;
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        // TODO: Implement save search functionality
        // This is a placeholder for future implementation
    }
}

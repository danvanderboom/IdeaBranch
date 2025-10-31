using IdeaBranch.App.ViewModels;
using Microsoft.Maui.Controls;

namespace IdeaBranch.App.Views;

public partial class AnnotationEditPage : ContentPage
{
    private readonly AnnotationEditViewModel _viewModel;

    public AnnotationEditPage(AnnotationEditViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        await _viewModel.SaveAsync();
        
        // Navigate back
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        // Navigate back without saving
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        // Confirm deletion
        var confirmed = await DisplayAlert(
            "Delete Annotation",
            "Are you sure you want to delete this annotation?",
            "Delete",
            "Cancel");

        if (confirmed)
        {
            await _viewModel.DeleteAsync();
            
            // Navigate back
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
        }
    }

    private void OnTagTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is AnnotationEditViewModel.SelectableTagItem tagItem)
        {
            tagItem.IsSelected = !tagItem.IsSelected;
            _viewModel.UpdateSelectedTags();
        }
    }
}


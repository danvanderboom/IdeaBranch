using IdeaBranch.App.ViewModels;
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
}


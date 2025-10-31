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
}


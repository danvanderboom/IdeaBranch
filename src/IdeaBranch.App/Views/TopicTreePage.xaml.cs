using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;
using IdeaBranch.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace IdeaBranch.App.Views;

public partial class TopicTreePage : ContentPage
{
    private readonly TopicTreeViewModel _viewModel;

    public TopicTreePage()
    {
        InitializeComponent();
        
        // Get ViewModel from DI container
        var services = Handler?.MauiContext?.Services ?? throw new InvalidOperationException("Services not available");
        _viewModel = services.GetRequiredService<TopicTreeViewModel>();
        BindingContext = _viewModel;
    }

    public TopicTreePage(TopicTreeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void OnNodeTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is ITreeNode node)
        {
            _viewModel.ToggleExpansion(node);
        }
    }
}


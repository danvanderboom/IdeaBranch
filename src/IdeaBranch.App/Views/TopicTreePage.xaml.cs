using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;
using IdeaBranch.App.ViewModels;

namespace IdeaBranch.App.Views;

public partial class TopicTreePage : ContentPage
{
    private readonly TopicTreeViewModel _viewModel;

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


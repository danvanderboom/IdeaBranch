using IdeaBranch.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace IdeaBranch.App.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage()
    {
        InitializeComponent();
        
        // Get ViewModel from DI container
        var services = Handler?.MauiContext?.Services ?? throw new InvalidOperationException("Services not available");
        _viewModel = services.GetRequiredService<SettingsViewModel>();
        BindingContext = _viewModel;
    }

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    private void OnCategorySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Category change is handled by binding automatically
    }
}


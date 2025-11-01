using System;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.App.Services.Search;
using IdeaBranch.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace IdeaBranch.App.Controls;

public partial class SimpleSearchPanel : ContentView
{
    private readonly SimpleSearchViewModel _viewModel;
    private CancellationTokenSource? _debounceCts;

    public SimpleSearchPanel()
    {
        InitializeComponent();
        
        // Get ViewModel from DI container
        var services = Handler?.MauiContext?.Services ?? throw new InvalidOperationException("Services not available");
        _viewModel = services.GetRequiredService<SimpleSearchViewModel>();
        BindingContext = _viewModel;
    }

    public SimpleSearchPanel(SimpleSearchViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void OnExpandToggleClicked(object? sender, EventArgs e)
    {
        _viewModel.IsExpanded = !_viewModel.IsExpanded;
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Cancel previous debounce
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();

        // Debounce search (300ms)
        _debounceCts = new CancellationTokenSource();
        try
        {
            await Task.Delay(300, _debounceCts.Token);
            if (_viewModel.SearchText != null && _viewModel.HasAnyContentTypeSelected())
            {
                await _viewModel.ExecuteSearchAsync(_debounceCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Debounce was cancelled, ignore
        }
    }

    private void OnContentTypeChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;

        var propertyName = checkBox.AutomationId switch
        {
            "SimpleSearchPanel_CheckTopicNodes" => nameof(_viewModel.SearchTopicNodes),
            "SimpleSearchPanel_CheckAnnotations" => nameof(_viewModel.SearchAnnotations),
            "SimpleSearchPanel_CheckTags" => nameof(_viewModel.SearchTags),
            "SimpleSearchPanel_CheckTemplates" => nameof(_viewModel.SearchTemplates),
            _ => null
        };

        // The binding should handle this, but we can trigger search if needed
    }

    private void OnUpdatedAtFromDateSelected(object? sender, DateChangedEventArgs e)
    {
        _viewModel.UpdatedAtFrom = e.NewDate;
    }

    private async void OnAdvancedSearchClicked(object? sender, EventArgs e)
    {
        // Navigate to Advanced Search page
        await Shell.Current.GoToAsync("//AdvancedSearchPage");
    }
}

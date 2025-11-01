using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.App.ViewModels;
using IdeaBranch.Domain;
using Microsoft.Maui.Controls;

namespace IdeaBranch.App.Controls;

public partial class TagPickerPopup : ContentView
{
    private TagPickerViewModel? _viewModel;
    private TaskCompletionSource<IReadOnlyList<TagSelection>>? _completionSource;

    public TagPickerPopup()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Shows the tag picker as a modal dialog and returns the selected tag selections.
    /// </summary>
    public static async Task<IReadOnlyList<TagSelection>?> ShowAsync(
        Page parentPage,
        IReadOnlyList<TagSelection>? initialSelections = null)
    {
        var popup = new TagPickerPopup();
        var viewModel = new TagPickerViewModel();
        
        popup._viewModel = viewModel;
        popup.BindingContext = viewModel;

        // Load tags
        await viewModel.LoadTagsAsync();

        // Set initial selections
        if (initialSelections != null && initialSelections.Count > 0)
        {
            viewModel.SetSelectedSelections(initialSelections);
        }

        // Create modal page with centered popup
        var modalPage = new ContentPage
        {
            BackgroundColor = new Color(0, 0, 0, 128), // Semi-transparent background
            Content = new Grid
            {
                Children = { popup },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(20)
            }
        };

        popup._completionSource = new TaskCompletionSource<IReadOnlyList<TagSelection>?>();

        // Show modal
        await parentPage.Navigation.PushModalAsync(modalPage);

        // Wait for result
        var result = await popup._completionSource.Task;

        // Close modal
        await parentPage.Navigation.PopModalAsync();

        return result;
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        _completionSource?.SetResult(null);
    }

    private void OnOkClicked(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            var selections = _viewModel.GetSelectedSelections();
            _completionSource?.SetResult(selections);
        }
        else
        {
            _completionSource?.SetResult(null);
        }
    }

}

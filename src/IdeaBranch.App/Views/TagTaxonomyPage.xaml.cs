using System;
using System.IO;
using System.Threading.Tasks;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace IdeaBranch.App.Views;

public partial class TagTaxonomyPage : ContentPage
{
    private readonly TagTaxonomyViewModel _viewModel;

    public TagTaxonomyPage()
    {
        InitializeComponent();
        
        // Get ViewModel from DI container
        var services = Handler?.MauiContext?.Services ?? throw new InvalidOperationException("Services not available");
        _viewModel = services.GetRequiredService<TagTaxonomyViewModel>();
        BindingContext = _viewModel;
    }

    public TagTaxonomyPage(TagTaxonomyViewModel viewModel)
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

    private async void OnMenuButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not ITreeNode node)
            return;

        var payload = TagTaxonomyViewModel.GetPayload(node);
        if (payload == null)
            return;

        var currentName = payload.Name;

        // Show context menu
        var action = await DisplayActionSheet(
            $"Options for '{currentName}'",
            "Cancel",
            null,
            "Create Category",
            "Create Tag",
            "Edit Name",
            "Move Up",
            "Move Down",
            "Delete");

        switch (action)
        {
            case "Create Category":
                await CreateCategoryAsync(node);
                break;
            case "Create Tag":
                await CreateTagAsync(node);
                break;
            case "Edit Name":
                await EditNodeNameAsync(node, currentName);
                break;
            case "Move Up":
                await _viewModel.MoveNodeUpAsync(node);
                break;
            case "Move Down":
                await _viewModel.MoveNodeDownAsync(node);
                break;
            case "Delete":
                await DeleteNodeAsync(node, currentName);
                break;
        }
    }

    private async Task CreateCategoryAsync(ITreeNode parentNode)
    {
        var name = await DisplayPromptAsync(
            "Create Category",
            "Enter category name:",
            "Create",
            "Cancel",
            "New Category",
            -1,
            Keyboard.Default,
            "Category name");

        if (!string.IsNullOrWhiteSpace(name))
        {
            await _viewModel.CreateCategoryAsync(parentNode, name);
        }
    }

    private async Task CreateTagAsync(ITreeNode parentNode)
    {
        var name = await DisplayPromptAsync(
            "Create Tag",
            "Enter tag name:",
            "Create",
            "Cancel",
            "New Tag",
            -1,
            Keyboard.Default,
            "Tag name");

        if (!string.IsNullOrWhiteSpace(name))
        {
            await _viewModel.CreateTagAsync(parentNode, name);
        }
    }

    private async Task EditNodeNameAsync(ITreeNode node, string currentName)
    {
        var newName = await DisplayPromptAsync(
            "Edit Name",
            "Enter new name:",
            "Save",
            "Cancel",
            currentName,
            -1,
            Keyboard.Default,
            "Name");

        if (!string.IsNullOrWhiteSpace(newName) && newName != currentName)
        {
            await _viewModel.UpdateNodeNameAsync(node, newName);
        }
    }

    private async Task DeleteNodeAsync(ITreeNode node, string nodeName)
    {
        var confirmed = await DisplayAlert(
            "Delete Node",
            $"Are you sure you want to delete '{nodeName}'? This will also delete all child nodes.",
            "Delete",
            "Cancel");

        if (confirmed)
        {
            var deleted = await _viewModel.DeleteNodeAsync(node);
            if (!deleted)
            {
                await DisplayAlert("Error", "Failed to delete node. It may be the root node or referenced by annotations.", "OK");
            }
        }
    }

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        try
        {
            var json = await _viewModel.ExportToJsonAsync();
            
            if (string.IsNullOrEmpty(json))
            {
                await DisplayAlert("Export", "No taxonomy data to export.", "OK");
                return;
            }

            // Save to file
            var fileName = $"tag_taxonomy_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, json);

            // For now, just show success message
            // In a full implementation, you'd use FilePicker to let user choose location
            await DisplayAlert("Export", $"Taxonomy exported successfully to:\n{filePath}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Export Error", $"Failed to export taxonomy: {ex.Message}", "OK");
        }
    }

    private async void OnImportClicked(object? sender, EventArgs e)
    {
        try
        {
            // For MVP, we'll use a simple file picker or prompt for JSON text
            // In a full implementation, you'd use FilePicker
            var json = await DisplayPromptAsync(
                "Import Taxonomy",
                "Paste JSON data:",
                "Import",
                "Cancel",
                "",
                -1,
                Keyboard.Default,
                "JSON data");

            if (!string.IsNullOrWhiteSpace(json))
            {
                var success = await _viewModel.ImportFromJsonAsync(json);
                if (success)
                {
                    await DisplayAlert("Import", "Taxonomy imported successfully.", "OK");
                }
                else
                {
                    await DisplayAlert("Import Error", _viewModel.ErrorMessage ?? "Failed to import taxonomy.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Import Error", $"Failed to import taxonomy: {ex.Message}", "OK");
        }
    }
}


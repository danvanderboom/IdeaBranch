using IdeaBranch.App.ViewModels;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Storage;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.App.Views;

public partial class VersionHistoryPage : ContentPage
{
    public VersionHistoryPage(VersionHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Parameterless constructor for XAML designer (creates empty viewModel)
    public VersionHistoryPage()
    {
        InitializeComponent();
        // Will be set via binding or navigation
        BindingContext = null;
    }
}


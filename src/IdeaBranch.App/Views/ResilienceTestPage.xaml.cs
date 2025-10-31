using IdeaBranch.App.Services;
using IdeaBranch.App.ViewModels;

namespace IdeaBranch.App.Views;

public partial class ResilienceTestPage : ContentPage
{
    public ResilienceTestPage(ExampleApiService apiService)
    {
        InitializeComponent();
        BindingContext = new ResilienceTestViewModel(apiService);
    }
}


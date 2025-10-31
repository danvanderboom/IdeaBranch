using IdeaBranch.App.ViewModels;

namespace IdeaBranch.App.Views;

public partial class MapPage : ContentPage
{
    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public MapPage() : this(new MapViewModel())
    {
    }
}

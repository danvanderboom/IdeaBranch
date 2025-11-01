using IdeaBranch.App.ViewModels.Analytics;

namespace IdeaBranch.App.Views.Analytics;

public partial class WordCloudPage : ContentPage
{
    public WordCloudPage(WordCloudViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public WordCloudPage() : this(new WordCloudViewModel())
    {
    }
}


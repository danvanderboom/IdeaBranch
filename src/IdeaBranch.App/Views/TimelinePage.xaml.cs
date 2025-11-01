using IdeaBranch.App.ViewModels.Analytics;

namespace IdeaBranch.App.Views;

public partial class TimelinePage : ContentPage
{
    public TimelinePage(TimelineViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public TimelinePage() : this(new TimelineViewModel())
    {
    }
}

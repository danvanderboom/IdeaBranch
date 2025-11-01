using IdeaBranch.App.ViewModels.Analytics;

namespace IdeaBranch.App.Views;

public partial class TimelinePage : ContentPage
{
    public TimelinePage(TimelineViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Wire up event selection from timeline view
        if (BindingContext is TimelineViewModel vm && TimelineView != null)
        {
            TimelineView.SelectedEventChanged += (s, eventView) =>
            {
                vm.SelectedEvent = eventView;
            };
        }
    }

    public TimelinePage() : this(new TimelineViewModel())
    {
    }

    private void OnTagPickerClicked(object? sender, EventArgs e)
    {
        // TODO: Show TagPickerPopup when enhanced with TagSelection support
        if (BindingContext is TimelineViewModel vm)
        {
            // Placeholder: Show alert for now
            DisplayAlert("Tag Picker", "Tag picker with hierarchical selection and per-tag 'Include descendants' toggle will be implemented here.", "OK");
        }
    }

}

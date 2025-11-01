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

    private void OnStatItemTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is TimelineViewModel vm && sender is Label label)
        {
            // Extract event type from label text
            // Format is multiple lines like:
            // "TopicCreated: 5"
            // "AnnotationCreated: 3"
            // or from trends like "TopicCreated: ▁▂▃▄▅▆▇█ (max: 10)"
            var text = label.Text;
            if (!string.IsNullOrEmpty(text))
            {
                // Parse lines and extract event types
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                // For simplicity, use the first line that has a valid event type
                // In a more advanced implementation, we could use e.GetPosition to determine which line was tapped
                foreach (var line in lines)
                {
                    // Format: "TypeName: count" or "TypeName: sparkline (max: value)"
                    var colonIndex = line.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        var eventType = line.Substring(0, colonIndex).Trim();
                        if (!string.IsNullOrEmpty(eventType) && vm.EventTypeCounts?.ContainsKey(eventType) == true)
                        {
                            // Toggle highlight for this type
                            vm.HighlightEventTypeCommand.Execute(eventType);
                            break;
                        }
                    }
                }
            }
        }
    }

}

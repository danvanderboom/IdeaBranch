using IdeaBranch.App.ViewModels.Analytics;
using IdeaBranch.App.Controls;
using IdeaBranch.Domain;
using System.Linq;

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

    private async void OnTagPickerClicked(object? sender, EventArgs e)
    {
        if (BindingContext is TimelineViewModel vm)
        {
            // Get current selections
            var currentSelections = vm.SelectedTagSelections.ToList();
            
            // Show tag picker dialog
            var selections = await TagPickerPopup.ShowAsync(this, currentSelections);
            
            // Update selections if not null (user clicked OK)
            if (selections != null)
            {
                vm.SelectedTagSelections.Clear();
                foreach (var selection in selections)
                {
                    vm.SelectedTagSelections.Add(selection);
                }
            }
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

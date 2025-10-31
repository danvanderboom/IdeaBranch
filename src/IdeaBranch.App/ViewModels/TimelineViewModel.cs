using System.ComponentModel;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for TimelinePage.
/// </summary>
public class TimelineViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

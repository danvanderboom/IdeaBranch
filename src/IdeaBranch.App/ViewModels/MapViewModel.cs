using System.ComponentModel;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for MapPage.
/// </summary>
public class MapViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

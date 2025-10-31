using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriticalInsight.Data;

public class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void Set<T>(string propertyName, ref T backingField, T newValue)
    {
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException(nameof(propertyName));

        if (backingField == null && newValue == null)
            return;

        if (backingField != null && backingField.Equals(newValue))
            return;

        backingField = newValue;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
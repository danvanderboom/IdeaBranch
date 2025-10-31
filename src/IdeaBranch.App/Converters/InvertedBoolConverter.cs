using System.Globalization;

namespace IdeaBranch.App.Converters;

/// <summary>
/// Converts bool to inverted bool: true becomes false, false becomes true.
/// Useful for disabling buttons when IsBusy is true.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        
        return false;
    }
}


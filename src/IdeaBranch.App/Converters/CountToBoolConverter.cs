using System.Globalization;

namespace IdeaBranch.App.Converters;

/// <summary>
/// Converts count (int) to bool: true if count > 0, false otherwise.
/// </summary>
public class CountToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
            return count > 0;
        
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


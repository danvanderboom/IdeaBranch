using System.Globalization;

namespace IdeaBranch.App.Converters;

/// <summary>
/// Converts string equality to bool: returns true if the value equals the parameter (case-insensitive).
/// Useful for conditional visibility based on selected category or option.
/// </summary>
public class StringEqualityToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null && parameter == null)
            return true;
        
        if (value == null || parameter == null)
            return false;

        var valueStr = value.ToString();
        var paramStr = parameter.ToString();
        
        return string.Equals(valueStr, paramStr, StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


using System.Globalization;

namespace IdeaBranch.App.Converters;

/// <summary>
/// Converts bool to FontAttributes: false becomes Bold, true becomes None.
/// Used to show unread notifications in bold.
/// </summary>
public class InvertedBoolToFontAttributesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue ? FontAttributes.Bold : FontAttributes.None;
        
        return FontAttributes.None;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


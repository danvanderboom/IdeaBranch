using System.Globalization;

namespace IdeaBranch.App.Converters;

/// <summary>
/// Converts Depth (int) to Thickness for left margin indentation in CollectionView items.
/// ConverterParameter specifies the indent amount per level (default: 16).
/// </summary>
public class DepthToThicknessConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int depth)
            return new Thickness(0);

        var indentPerLevel = 16.0; // Default indent per depth level
        if (parameter is double indent)
        {
            indentPerLevel = indent;
        }
        else if (parameter is string indentStr && double.TryParse(indentStr, out var parsedIndent))
        {
            indentPerLevel = parsedIndent;
        }

        var leftMargin = depth * indentPerLevel;
        return new Thickness(leftMargin, 0, 0, 0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


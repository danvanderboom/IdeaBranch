using System.Globalization;

namespace IdeaBranch.App.Converters;

/// <summary>
/// Converts word weight (0.0-1.0) to font size for word cloud display.
/// </summary>
public class WeightToFontSizeConverter : IValueConverter
{
    private const double MinFontSize = 12.0;
    private const double MaxFontSize = 48.0;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double weight)
        {
            // Scale weight (0.0-1.0) to font size range
            var fontSize = MinFontSize + (weight * (MaxFontSize - MinFontSize));
            return fontSize;
        }

        return MinFontSize;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


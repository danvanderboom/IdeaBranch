using System.Globalization;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;

namespace IdeaBranch.App.Converters;

/// <summary>
/// Extracts Name from TagTaxonomyPayload stored in ITreeNode.PayloadObject.
/// </summary>
public class TagTaxonomyNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ITreeNode<TagTaxonomyPayload> node)
        {
            return node.Payload?.Name ?? "Untitled";
        }
        
        if (value is ITreeNode node2 && node2.PayloadObject is TagTaxonomyPayload payload)
        {
            return payload.Name ?? "Untitled";
        }
        
        return "Untitled";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


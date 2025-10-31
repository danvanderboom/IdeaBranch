using System.Globalization;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;

namespace IdeaBranch.App.Converters;

/// <summary>
/// Extracts Title from TopicNodePayload stored in ITreeNode.PayloadObject.
/// </summary>
public class NodePayloadTitleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ITreeNode<TopicNodePayload> node)
        {
            return node.Payload?.Title ?? "Untitled";
        }
        
        if (value is ITreeNode node2 && node2.PayloadObject is TopicNodePayload payload)
        {
            return payload.Title ?? "Untitled";
        }
        
        return "Untitled";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


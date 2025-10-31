using System.Globalization;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;

namespace IdeaBranch.App.Converters;

/// <summary>
/// Extracts Prompt from TopicNodePayload stored in ITreeNode.PayloadObject.
/// </summary>
public class NodePayloadPromptConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ITreeNode<TopicNodePayload> node)
        {
            return node.Payload?.Prompt ?? string.Empty;
        }
        
        if (value is ITreeNode node2 && node2.PayloadObject is TopicNodePayload payload)
        {
            return payload.Prompt ?? string.Empty;
        }
        
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


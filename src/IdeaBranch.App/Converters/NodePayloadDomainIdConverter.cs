using System.Globalization;
using CriticalInsight.Data.Hierarchical;
using IdeaBranch.App.Adapters;

namespace IdeaBranch.App.Converters;

/// <summary>
/// Extracts DomainNodeId from TopicNodePayload stored in ITreeNode.PayloadObject.
/// </summary>
public class NodePayloadDomainIdConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ITreeNode node && node.PayloadObject is TopicNodePayload payload)
        {
            return payload.DomainNodeId;
        }
        
        return Guid.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


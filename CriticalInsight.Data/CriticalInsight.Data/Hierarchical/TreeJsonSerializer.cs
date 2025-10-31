using System.Text.Json;
using System.Text.Json.Serialization;

namespace CriticalInsight.Data.Hierarchical;

public class TreeJsonSerializer
{
    public static string Serialize(ITreeNode node, Dictionary<string, Type> payloadTypes, bool writeIndented = true)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = writeIndented,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters =
            {
                new TreeJsonConverter
                {
                    PayloadTypes = payloadTypes
                }
            }
        };

        return JsonSerializer.Serialize(node, options);
    }

    public static TResult? Deserialize<TResult>(string json, Dictionary<string, Type> payloadTypes)
        where TResult : class
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters =
            {
                new TreeJsonConverter
                {
                    PayloadTypes = payloadTypes
                }
            }
        };

        return JsonSerializer.Deserialize<ITreeNode>(json, options) as TResult;
    }
}
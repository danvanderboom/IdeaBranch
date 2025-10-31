using System.Text.Json;
using System.Text.Json.Serialization;

namespace CriticalInsight.Data.Hierarchical;

public class TreeViewJsonSerializer
{
    public static string Serialize(TreeView view, Dictionary<string, Type> payloadTypes, bool includeViewRoot = false, bool writeIndented = true)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = writeIndented,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters =
            {
                new TreeViewJsonConverter
                {
                    PayloadTypes = payloadTypes,
                    IncludeViewRoot = includeViewRoot
                }
            }
        };

        return JsonSerializer.Serialize(view, options);
    }

    public static TreeView? Deserialize(string json, Dictionary<string, Type> payloadTypes, Func<string, ITreeNode?>? nodeLookup)
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters =
            {
                new TreeViewJsonConverter
                {
                    PayloadTypes = payloadTypes,
                    NodeLookup = nodeLookup
                }
            }
        };

        return JsonSerializer.Deserialize<TreeView>(json, options);
    }
}
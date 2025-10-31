using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CriticalInsight.Data.Hierarchical;

public class TreeJsonConverter : JsonConverter<ITreeNode>
{
    public Dictionary<string, Type> PayloadTypes { get; set; } = new();

    // Reverse lookup: maps payload type to friendly name.
    private Dictionary<Type, string> PayloadTypeNames =>
        PayloadTypes.ToDictionary(pt => pt.Value, pt => pt.Key);

    public override ITreeNode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("PayloadType", out JsonElement payloadTypeEl))
            throw new JsonException("PayloadType property not found.");
        string payloadTypeName = payloadTypeEl.GetString() ?? "";
        if (string.IsNullOrEmpty(payloadTypeName))
            throw new JsonException("PayloadType property is empty.");

        Type? payloadType = PayloadTypes.ContainsKey(payloadTypeName)
            ? PayloadTypes[payloadTypeName]
            : Type.GetType(payloadTypeName);
        if (payloadType == null)
            throw new JsonException($"Invalid payload type: {payloadTypeName}");

        bool payloadIsTreeNode = typeof(ITreeNode).IsAssignableFrom(payloadType);
        ITreeNode? node;
        if (payloadIsTreeNode)
        {
            node = Activator.CreateInstance(payloadType) as ITreeNode;
            if (node == null)
                throw new JsonException("Unable to instantiate tree node.");
        }
        else
        {
            Type genericType = typeof(TreeNode<>).MakeGenericType(payloadType);
            node = Activator.CreateInstance(genericType) as ITreeNode;
            if (node == null)
                throw new JsonException("Unable to instantiate tree node.");
        }

        if (root.TryGetProperty("NodeId", out JsonElement nodeIdEl))
            node.NodeId = nodeIdEl.GetString() ?? Guid.NewGuid().ToString();
        node.PayloadType = payloadTypeName;

        if (root.TryGetProperty("Payload", out JsonElement payloadEl))
        {
            // Use the JsonElement.Deserialize extension method rather than GetRawText.
            object? payload = payloadEl.Deserialize(payloadType, options);
            if (payload == null)
                throw new JsonException($"Unable to deserialize payload for type {payloadType}");
            node.PayloadObject = payload;
        }
        else
        {
            node.PayloadObject = node;
            foreach (JsonProperty prop in root.EnumerateObject())
            {
                if (prop.Name is "NodeId" or "PayloadType" or "Children")
                    continue;
                PropertyInfo? pi = payloadType.GetProperty(prop.Name);
                if (pi != null)
                {
                    object? val = prop.Value.Deserialize(pi.PropertyType, options);
                    pi.SetValue(node, val);
                }
            }
        }

        if (root.TryGetProperty("Children", out JsonElement childrenEl) && childrenEl.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement childEl in childrenEl.EnumerateArray())
            {
                ITreeNode? child = childEl.Deserialize<ITreeNode>(options);
                if (child != null)
                    node.Children.Add(child);
            }
        }

        return node;
    }

    public override void Write(Utf8JsonWriter writer, ITreeNode value, JsonSerializerOptions options)
    {
        Debug.WriteLine(value.GetType().Name);

        // Determine the actual payload type.
        // For self-referencing nodes (where PayloadObject == value) we use value.GetType(),
        // so that friendly names for inherited node types (e.g. "Forest") are returned.
        Type actualPayloadType = ReferenceEquals(value.PayloadObject, value)
            ? value.GetType()
            : value.PayloadObject.GetType();

        // Look up the friendly name, if any; if not, fall back to the type's Name.
        string payloadTypeToWrite = PayloadTypeNames.TryGetValue(actualPayloadType, out string friendly)
            ? friendly
            : actualPayloadType.Name;
        // We'll force this value into our output.
        // (We do not update the node instance so as not to affect in-memory state.)

        // Determine if the node is self-referencing.
        bool selfPayload = ReferenceEquals(value.PayloadObject, value);

        // Build an ordered JsonObject.
        var json = new JsonObject();

        // 1. NodeId first.
        json.Add("NodeId", JsonSerializer.SerializeToNode(value.NodeId, typeof(string), options));
        // 2. PayloadType second (using our forced friendly name).
        json.Add("PayloadType", JsonSerializer.SerializeToNode(payloadTypeToWrite, typeof(string), options));

        if (selfPayload)
        {
            // For self-referencing nodes, merge the payload's extra properties.
            // Collect all public instance properties (from this type and its base types)
            // except NodeId, PayloadType, Children, and (if any) Payload.
            var payloadProps = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pi => !new[] { "NodeId", "PayloadType", "Children", "Payload" }.Contains(pi.Name)
                             && pi.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                .OrderBy(pi => pi.GetCustomAttribute<JsonPropertyOrderAttribute>()?.Order ?? int.MaxValue)
                .ThenBy(pi => pi.Name);
            foreach (var prop in payloadProps)
            {
                object? propVal = prop.GetValue(value);
                if (propVal != null)
                {
                    JsonNode? propNode = JsonSerializer.SerializeToNode(propVal, prop.PropertyType, options);
                    if (propNode != null)
                        json.Add(prop.Name, propNode.DeepClone());
                }
            }
        }
        else
        {
            // For non-self-referencing nodes, include a separate "Payload" property.
            JsonNode payloadNode = JsonSerializer.SerializeToNode(value.PayloadObject, actualPayloadType, options)
                ?? new JsonObject();
            json.Add("Payload", payloadNode.DeepClone());
        }

        // 3. Finally, add Children last.
        var childrenArray = new JsonArray();
        if (value.Children != null)
        {
            foreach (var child in value.Children)
            {
                JsonNode? childNode = JsonSerializer.SerializeToNode(child, typeof(ITreeNode), options);
                childrenArray.Add(childNode);
            }
        }
        json.Add("Children", childrenArray);

        json.WriteTo(writer);
    }
}
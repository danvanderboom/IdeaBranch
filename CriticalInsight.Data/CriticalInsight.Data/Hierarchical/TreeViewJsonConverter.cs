using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CriticalInsight.Data.Hierarchical;

public class TreeViewJsonConverter : JsonConverter<TreeView>
{
    public Func<string, ITreeNode?>? NodeLookup { get; set; }

    public Dictionary<string, Type> PayloadTypes { get; set; } = new();

    public bool IncludeViewRoot { get; set; } = false;

    private Dictionary<Type, string> PayloadTypeNames =>
        PayloadTypes.ToDictionary(pt => pt.Value, pt => pt.Key);

    public override TreeView? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Parse the outer JSON object.
        JsonObject rootObj = JsonNode.Parse(ref reader)?.AsObject()
            ?? throw new JsonException("Expected JSON object for TreeView.");

        // Read TreeView-level settings.
        string? rootNodeId = rootObj["RootNodeId"]?.GetValue<string>();
        if (string.IsNullOrEmpty(rootNodeId))
            throw new JsonException("Missing RootNodeId.");

        List<string> included = rootObj["IncludedProperties"]?.Deserialize<List<string>>(options) ?? [];
        List<string> excluded = rootObj["ExcludedProperties"]?.Deserialize<List<string>>(options) ?? [];
        bool defaultExpanded = rootObj["DefaultExpanded"]?.GetValue<bool>() ?? true;

        // Use the lookup delegate to retrieve the root node.
        if (NodeLookup == null)
            throw new InvalidOperationException("NodeLookup delegate is not set.");

        ITreeNode? rootNode = NodeLookup(rootNodeId);
        if (rootNode == null)
            throw new JsonException($"No ITreeNode found for NodeId {rootNodeId}.");

        // Create a new TreeView instance based on the provided root node.
        var treeView = new TreeView(rootNode, defaultExpanded)
        {
            IncludedProperties = included,
            ExcludedProperties = excluded,
            DefaultExpanded = defaultExpanded
        };

        // Traverse the "View" JSON to update each node's expanded state.
        JsonObject viewObj = rootObj["View"]?.AsObject() ?? throw new JsonException("Missing View object.");
        UpdateExpandedStates(viewObj, treeView);

        // Update the projected collection as needed.
        treeView.UpdateProjectedCollection();

        return treeView;
    }

    private void UpdateExpandedStates(JsonObject nodeJson, TreeView treeView)
    {
        // Get the node id.
        string nodeId = nodeJson["NodeId"]?.GetValue<string>()
            ?? throw new JsonException("Missing NodeId in node.");

        // Lookup the actual node.
        ITreeNode? node = NodeLookup?.Invoke(nodeId);
        if (node != null)
        {
            bool isExpanded = nodeJson["IsExpanded"]?.GetValue<bool>() ?? true;
            treeView.SetIsExpanded(node, isExpanded);
        }

        // If a "Children" property exists, process recursively.
        if (nodeJson.TryGetPropertyValue("Children", out JsonNode? childrenNode) && childrenNode is JsonArray childrenArray)
        {
            foreach (JsonNode? child in childrenArray)
            {
                if (child is JsonObject childObj)
                {
                    UpdateExpandedStates(childObj, treeView);
                }
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, TreeView value, JsonSerializerOptions options)
    {
        // First, generate the view content by serializing the root node.
        JsonObject viewObj = WriteNode(value.Root, value, options);

        // Build the outer JSON wrapper that contains TreeView settings and the view.
        var rootWrapper = new JsonObject
        {
            ["RootNodeId"] = value.Root.NodeId,
            ["IncludedProperties"] = JsonSerializer.SerializeToNode(value.IncludedProperties, options),
            ["ExcludedProperties"] = JsonSerializer.SerializeToNode(value.ExcludedProperties, options),
            ["DefaultExpanded"] = value.DefaultExpanded,
            ["View"] = viewObj
        };

        if (IncludeViewRoot)
            rootWrapper.WriteTo(writer);
        else
            viewObj.WriteTo(writer);
    }

    private JsonObject WriteNode(ITreeNode node, TreeView treeView, JsonSerializerOptions options)
    {
        // Set a friendlier PayloadType if available.
        if (PayloadTypeNames.TryGetValue(node.PayloadObject.GetType(), out string? friendlyType))
        {
            node.PayloadType = friendlyType ?? string.Empty;
        }

        var obj = new JsonObject();

        // Get all serializable properties except the Children collection.
        var properties = node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(pi => pi.Name != nameof(ITreeNode.Children)
                         && pi.CanRead && pi.CanWrite
                         && pi.GetCustomAttribute<JsonIgnoreAttribute>() is null)
            .Select(pi => new { pi, order = pi.GetCustomAttribute<JsonPropertyOrderAttribute>()?.Order ?? int.MaxValue })
            .OrderBy(x => x.order);

        // Cache any collection properties to avoid serializing empty ones.
        var collectionProperties = properties
            .Where(p => typeof(ICollection).IsAssignableFrom(p.pi.PropertyType))
            .ToDictionary(p => p.pi.Name, p => p.pi.GetValue(node) as ICollection);

        foreach (var p in properties)
        {
            bool isInheritedNodeType = node == node.PayloadObject;
            bool isPayloadProperty = p.pi.Name == "Payload";
            if (isInheritedNodeType && isPayloadProperty)
                continue;

            object? propValue = p.pi.GetValue(node);
            if (propValue != null)
            {
                JsonNode? jsonNode = JsonSerializer.SerializeToNode(propValue, p.pi.PropertyType, options);
                if (jsonNode is not null)
                {
                    obj[p.pi.Name] = jsonNode;
                }
            }
        }

        // Merge in all payload properties.
        JsonNode? payloadNode = JsonSerializer.SerializeToNode(node.PayloadObject, node.PayloadObject.GetType(), options);
        if (payloadNode != null)
        {
            foreach (var kv in payloadNode.AsObject())
            {
                if (kv.Value != null)
                    obj[kv.Key] = kv.Value.DeepClone();
            }
        }

        // Assemble the final JsonObject in the desired order.
        var ordered = new JsonObject();

        void AddIfExists(string propName)
        {
            if (obj.TryGetPropertyValue(propName, out JsonNode? value) && value != null)
                ordered[propName] = value.DeepClone();
        }
        AddIfExists("NodeId");
        AddIfExists("PayloadType");

        foreach (var kv in obj)
        {
            if (kv.Key == "NodeId" ||
                kv.Key == "PayloadType" ||
                kv.Key == "Payload" ||
                kv.Key == "IsExpanded" ||
                kv.Key == "ChildrenCount" ||
                kv.Key == nameof(ITreeNode.Children))
                continue;

            // skip serializing empty collections
            if (collectionProperties.TryGetValue(kv.Key, out ICollection? collection)
                && (collection == null || collection.Count == 0))
                continue;

            // skip serializing null properties
            if (kv.Value != null)
                ordered[kv.Key] = kv.Value.DeepClone();
        }

        // Add extra properties.
        bool isExpanded = treeView.GetIsExpanded(node);
        ordered["IsExpanded"] = isExpanded;
        int childrenCount = node.Children.Count;
        ordered["ChildrenCount"] = childrenCount;

        // Add children array if expanded.
        var childrenArray = new JsonArray();
        if (isExpanded && childrenCount > 0)
        {
            foreach (var child in node.Children)
            {
                JsonObject childObj = WriteNode(child, treeView, options);
                childrenArray.Add(childObj);
            }
            ordered[nameof(ITreeNode.Children)] = childrenArray;
        }

        // Apply property filtering based on IncludedProperties and ExcludedProperties.
        JsonObject filteredOrdered = FilterJsonObject(ordered, string.Empty, treeView.IncludedProperties, treeView.ExcludedProperties);
        return filteredOrdered;
    }

    private JsonObject FilterJsonObject(JsonObject obj, string parentPath, List<string> included, List<string> excluded)
    {
        var result = new JsonObject();

        foreach (var kv in obj)
        {
            string propName = kv.Key;
            string fullPath = string.IsNullOrEmpty(parentPath) ? propName : parentPath + "." + propName;

            // Exclude property if its full path is explicitly in the excluded list.
            if (excluded.Any(e => string.Equals(e, fullPath, StringComparison.OrdinalIgnoreCase)))
                continue;

            // If an inclusion list exists, include only if the property matches exactly or is a prefix for nested properties.
            if (included.Count > 0)
            {
                bool allowed = included.Any(inc =>
                    string.Equals(inc, fullPath, StringComparison.OrdinalIgnoreCase) ||
                    inc.StartsWith(fullPath + ".", StringComparison.OrdinalIgnoreCase));
                if (!allowed)
                    continue;
            }

            // If the property is a JsonObject, filter it recursively.
            if (kv.Value is JsonObject childObj)
            {
                result[propName] = FilterJsonObject(childObj, fullPath, included, excluded);
            }
            else if (kv.Value is JsonArray arr)
            {
                var newArr = new JsonArray();
                foreach (var element in arr)
                {
                    if (element is JsonObject elementObj)
                    {
                        newArr.Add(FilterJsonObject(elementObj, fullPath, included, excluded));
                    }
                    else
                    {
                        if (element != null)
                            newArr.Add(element.DeepClone());
                    }
                }
                result[propName] = newArr;
            }
            else
            {
                if (kv.Value != null)
                    result[propName] = kv.Value.DeepClone();
            }
        }
        return result;
    }
}
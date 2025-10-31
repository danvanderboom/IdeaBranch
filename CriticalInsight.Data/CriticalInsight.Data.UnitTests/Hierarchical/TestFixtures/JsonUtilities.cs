using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;

public static class JsonUtilities
{
    public static List<string> GetPropertyNamesForNodeObject(string json, string nodeId, string parentPropertyName)
    {
        var root = JsonNode.Parse(json) as JsonObject;
        if (root == null)
            return new List<string>();

        return FindProperties(root, nodeId, parentPropertyName);
    }

    private static List<string> FindProperties(JsonObject node, string nodeId, string parentPropertyName)
    {
        if (node.TryGetPropertyValue("NodeId", out var idNode) && idNode?.ToString() == nodeId)
        {
            if (node.TryGetPropertyValue(parentPropertyName, out var parentNode) && parentNode is JsonObject parentObj)
            {
                return parentObj.Select(prop => prop.Key).ToList();
            }
            return new List<string>();
        }

        if (node.TryGetPropertyValue("Children", out var childrenNode) && childrenNode is JsonArray childrenArray)
        {
            foreach (var child in childrenArray)
            {
                if (child is JsonObject childObj)
                {
                    var result = FindProperties(childObj, nodeId, parentPropertyName);
                    if (result.Any())
                        return result;
                }
            }
        }

        return new List<string>();
    }

    public static string? GetSiblingPropertyValueForNode(string json, string nodeId, string siblingPropertyName)
    {
        var root = JsonNode.Parse(json) as JsonObject;
        if (root == null)
            return null;

        return FindSiblingPropertyValue(root, nodeId, siblingPropertyName);
    }

    private static string? FindSiblingPropertyValue(JsonObject node, string nodeId, string siblingPropertyName)
    {
        if (node.TryGetPropertyValue("NodeId", out var idNode) && idNode?.ToString() == nodeId)
        {
            if (node.TryGetPropertyValue(siblingPropertyName, out var siblingValue))
                return siblingValue?.ToString();
            return null;
        }

        if (node.TryGetPropertyValue("Children", out var childrenNode) && childrenNode is JsonArray childrenArray)
        {
            foreach (var child in childrenArray)
            {
                if (child is JsonObject childObj)
                {
                    var result = FindSiblingPropertyValue(childObj, nodeId, siblingPropertyName);
                    if (result != null)
                        return result;
                }
            }
        }

        return null;
    }

    public static bool DoesSiblingPropertyExistForNode(string json, string nodeId, string siblingPropertyName)
    {
        var root = JsonNode.Parse(json) as JsonObject;
        if (root == null)
            return false;

        return FindSiblingPropertyExists(root, nodeId, siblingPropertyName);
    }

    private static bool FindSiblingPropertyExists(JsonObject node, string nodeId, string siblingPropertyName)
    {
        if (node.TryGetPropertyValue("NodeId", out var idNode) && idNode?.ToString() == nodeId)
        {
            // Check if the sibling property exists
            return node.TryGetPropertyValue(siblingPropertyName, out _);
        }

        // Recursively search in "Children"
        if (node.TryGetPropertyValue("Children", out var childrenNode) && childrenNode is JsonArray childrenArray)
        {
            foreach (var child in childrenArray)
            {
                if (child is JsonObject childObj)
                {
                    if (FindSiblingPropertyExists(childObj, nodeId, siblingPropertyName))
                        return true;
                }
            }
        }

        return false;
    }
}
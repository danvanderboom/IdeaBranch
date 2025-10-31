using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;

namespace CriticalInsight.Data.IntegrationTests.Agents.Integration;

public static class HierarchicalContextBuilder
{
    public sealed class ViewOptions
    {
        public bool IncludeViewRoot { get; set; } = false;
        public bool DefaultExpanded { get; set; } = true;
        public int? DepthLimit { get; set; }
        public List<string> IncludedProperties { get; set; } = new();
        public List<string> ExcludedProperties { get; set; } = new();
    }

    public static Dictionary<string, Type> GetDefaultPayloadTypes()
    {
        return new Dictionary<string, Type>
        {
            { nameof(Space), typeof(Space) },
            { nameof(Substance), typeof(Substance) }
        };
    }

    public static (TreeView view, string json) BuildViewJson(ITreeNode root, Action<TreeView>? configureView = null, ViewOptions? options = null)
    {
        var view = new TreeView(root);
        if (options != null)
        {
            view.DefaultExpanded = options.DefaultExpanded;
            if (options.IncludedProperties.Any())
                view.IncludedProperties = options.IncludedProperties.ToList();
            if (options.ExcludedProperties.Any())
                view.ExcludedProperties = options.ExcludedProperties.ToList();
            // Note: TreeView doesn't have DepthLimit property - depth filtering handled in serialization
        }

        configureView?.Invoke(view);

        string json = TreeViewJsonSerializer.Serialize(view, GetDefaultPayloadTypes(), includeViewRoot: options?.IncludeViewRoot ?? false, writeIndented: true);
        return (view, json);
    }

    public static string BuildMcqPrompt(string title, string narrative, string viewJson, string question, string[] options)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Title: {title}");
        sb.AppendLine();
        sb.AppendLine(narrative);
        sb.AppendLine();
        sb.AppendLine("Here is the hierarchical view in JSON (read-only):");
        sb.AppendLine("```json");
        sb.AppendLine(viewJson);
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine(question);
        for (int i = 0; i < options.Length; i++)
        {
            char letter = (char)('A' + i);
            sb.AppendLine($"{letter}) {options[i]}");
        }
        sb.AppendLine();
        sb.AppendLine("Answer with only a single capital letter: A, B, C, or D.");
        return sb.ToString();
    }
}



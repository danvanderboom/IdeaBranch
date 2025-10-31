using System;

namespace IdeaBranch.App.Adapters;

/// <summary>
/// Payload class for TreeNode&lt;TagTaxonomyPayload&gt; to bind in MAUI CollectionView.
/// Represents domain TagTaxonomyNode data without coupling to CriticalInsight.Data in Domain layer.
/// </summary>
public class TagTaxonomyPayload
{
    public Guid DomainNodeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


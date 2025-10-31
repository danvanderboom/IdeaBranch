namespace CriticalInsight.Data.Hierarchical;

public class ArtifactNode : TreeNode<ArtifactNode>
{
    string artifactType = string.Empty;
    public string ArtifactType
    {
        get { return Parent == null ? artifactType : (Parent as ArtifactNode)?.ArtifactType ?? string.Empty; }
        set
        {
            if (Parent == null)
                artifactType = value;
            else
                (Parent as ArtifactNode)!.ArtifactType = value;
        }
    }

    public Dictionary<string, string> StringProperties { get; set; } = [];
    public Dictionary<string, long> LongProperties { get; set; } = [];
    public Dictionary<string, double> DoubleProperties { get; set; } = [];
    public Dictionary<string, decimal> DecimalProperties { get; set; } = [];
    public Dictionary<string, DateTime> DateTimeProperties { get; set; } = [];
    public Dictionary<string, TimeSpan> TimeSpanProperties { get; set; } = [];
    public Dictionary<string, double[]> VectorProperties { get; set; } = [];

    public ArtifactNode() : base()
    {
    }

    public ArtifactNode SetArtifactType(string artifactType)
    {
        ArtifactType = artifactType;
        return this;
    }

    public ArtifactNode Set(string propertyName, string stringValue)
    {
        if (StringProperties == null)
            StringProperties = new Dictionary<string, string>();

        StringProperties[propertyName] = stringValue;
        return this;
    }

    public ArtifactNode Set(string propertyName, long longValue)
    {
        if (LongProperties == null)
            LongProperties = new Dictionary<string, long>();

        LongProperties[propertyName] = longValue;
        return this;
    }

    public ArtifactNode Set(string propertyName, double doubleValue)
    {
        if (DoubleProperties == null)
            DoubleProperties = new Dictionary<string, double>();

        DoubleProperties[propertyName] = doubleValue;
        return this;
    }

    public ArtifactNode Set(string propertyName, decimal decimalValue)
    {
        if (DecimalProperties == null)
            DecimalProperties = new Dictionary<string, decimal>();

        DecimalProperties[propertyName] = decimalValue;
        return this;
    }

    public ArtifactNode Set(string propertyName, DateTime dateTimeValue)
    {
        if (DateTimeProperties == null)
            DateTimeProperties = new Dictionary<string, DateTime>();

        DateTimeProperties[propertyName] = dateTimeValue;
        return this;
    }

    public ArtifactNode Set(string propertyName, TimeSpan timeSpanValue)
    {
        if (TimeSpanProperties == null)
            TimeSpanProperties = new Dictionary<string, TimeSpan>();

        TimeSpanProperties[propertyName] = timeSpanValue;
        return this;
    }

    public ArtifactNode Set(string propertyName, double[] vectorValue)
    {
        if (VectorProperties == null)
            VectorProperties = new Dictionary<string, double[]>();

        VectorProperties[propertyName] = vectorValue;
        return this;
    }

    public ArtifactNode SetParent(ArtifactNode parent)
    {
        Parent = parent;
        return this;
    }

    public ArtifactNode AddChild(ArtifactNode child)
    {
        Children.Add(child);
        return this;
    }

    public string Serialize(bool writeIndented = true) => 
        Serialize(Artifact.PayloadTypes, writeIndented);

    public TreeView CreateView() => new(this)
    {
        ExcludedProperties = [ nameof(PayloadType), nameof(VectorProperties) ]
    };
}
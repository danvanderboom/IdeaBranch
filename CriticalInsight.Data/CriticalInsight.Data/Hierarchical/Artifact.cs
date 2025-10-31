using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CriticalInsight.Data.Hierarchical;

public class Artifact : ArtifactNode
{
    public Artifact() : base()
    {
    }

    public Artifact(string artifactType) : this()
    {
        ArtifactType = artifactType;
    }

    public static JsonSerializerOptions JsonSerializerOptions =>
        new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters =
            {
                new TreeViewJsonConverter
                {
                    PayloadTypes = new Dictionary<string, Type>
                    {
                        { nameof(Artifact), typeof(Artifact) },
                        { nameof(ArtifactNode), typeof(ArtifactNode) }
                    }
                }
            }
        };

    public static Dictionary<string, Type> PayloadTypes => new()
    {
        { nameof(Artifact), typeof(Artifact) },
        { nameof(ArtifactNode), typeof(ArtifactNode) }
    };

    public static Artifact? Deserialize(string json) =>
        TreeJsonSerializer.Deserialize<Artifact>(json, PayloadTypes);
}
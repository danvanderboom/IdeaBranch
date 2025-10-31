using CriticalInsight.Data.Hierarchical;
using CriticalInsight.Data.UnitTests.Hierarchical.TestFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace CriticalInsight.Data.UnitTests.Hierarchical;

public class ArtifactNodeTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ArtifactNode_SetProperties()
    {
        var embeddings = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 };
        var createdAt = new DateTime(2025, 2, 19, 5, 30, 0);
        var artifact = new Artifact()
            .SetArtifactType("TestArtifact")
            .Set("Title", "Artifact Root Title")
            .Set("Description", "Artifact Root Description")
            .Set("CreatedAt", createdAt)
            .Set("Duration", TimeSpan.FromHours(1))
            .Set("Size", 1024L)
            .Set("Price", 99.99M)
            .Set("Weight", 1.5D)
            .Set("TitleEmbeddings", embeddings);

        var json = artifact.Serialize();

        var deserializedArtifact = Artifact.Deserialize(json);
        Assert.That(deserializedArtifact, Is.Not.Null);
        Assert.That(deserializedArtifact.ArtifactType, Is.EqualTo("TestArtifact"));
        Assert.That(deserializedArtifact.StringProperties, Does.ContainKey("Title"));
        Assert.That(deserializedArtifact.StringProperties["Title"], Is.EqualTo("Artifact Root Title"));
        Assert.That(deserializedArtifact.StringProperties, Does.ContainKey("Description"));
        Assert.That(deserializedArtifact.StringProperties["Description"], Is.EqualTo("Artifact Root Description"));
        Assert.That(deserializedArtifact.DateTimeProperties, Does.ContainKey("CreatedAt"));
        Assert.That(deserializedArtifact.DateTimeProperties["CreatedAt"], Is.EqualTo(createdAt));
        Assert.That(deserializedArtifact.TimeSpanProperties, Does.ContainKey("Duration"));
        Assert.That(deserializedArtifact.TimeSpanProperties["Duration"], Is.EqualTo(TimeSpan.FromHours(1)));
        Assert.That(deserializedArtifact.LongProperties, Does.ContainKey("Size"));
        Assert.That(deserializedArtifact.LongProperties["Size"], Is.EqualTo(1024L));
        Assert.That(deserializedArtifact.DecimalProperties, Does.ContainKey("Price"));
        Assert.That(deserializedArtifact.DecimalProperties["Price"], Is.EqualTo(99.99M));
        Assert.That(deserializedArtifact.DoubleProperties, Does.ContainKey("Weight"));
        Assert.That(deserializedArtifact.DoubleProperties["Weight"], Is.EqualTo(1.5D));
        Assert.That(deserializedArtifact.VectorProperties, Does.ContainKey("TitleEmbeddings"));
        Assert.That(deserializedArtifact.VectorProperties["TitleEmbeddings"], Is.EquivalentTo(embeddings));
    }

    [Test]
    public void ArtifactNode_SetParent()
    {
        var artifact = new Artifact()
            .Set("Title", "Artifact Root Title")
            .Set("Description", "Artifact Root Description");

        var child1 = new ArtifactNode()
            .SetParent(artifact)
            .Set("Title", "Child 1 Title")
            .Set("Description", "Child 1 Description")
            .Set("CreatedAt", DateTime.UtcNow)
            .Set("Duration", TimeSpan.FromHours(1));

        var child2 = new ArtifactNode()
            .SetParent(artifact)
            .Set("Title", "Child 2 Title")
            .Set("Description", "Child 2 Description")
            .Set("CreatedAt", DateTime.UtcNow.AddHours(1))
            .Set("Duration", TimeSpan.FromHours(2));

        var json = artifact.Serialize();
        Assert.That(json, Does.Contain("Title"));
        Assert.That(json, Does.Contain("Description"));
        Assert.That(json, Does.Contain("CreatedAt"));
        Assert.That(json, Does.Contain("Duration"));

        var deserializedArtifact = Artifact.Deserialize(json);
        Assert.That(deserializedArtifact, Is.Not.Null);
        Assert.That(deserializedArtifact.StringProperties, Does.ContainKey("Title"));
        Assert.That(deserializedArtifact.StringProperties, Does.ContainKey("Description"));

        var deserializedChild1 = deserializedArtifact.Children[0] as ArtifactNode;
        Assert.That(deserializedChild1, Is.Not.Null);
        Assert.That(deserializedChild1.StringProperties, Does.ContainKey("Title"));
        Assert.That(deserializedChild1.StringProperties, Does.ContainKey("Description"));
        Assert.That(deserializedChild1.DateTimeProperties, Does.ContainKey("CreatedAt"));
        Assert.That(deserializedChild1.TimeSpanProperties, Does.ContainKey("Duration"));

        var deserializedChild2 = deserializedArtifact.Children[0] as ArtifactNode;
        Assert.That(deserializedChild2, Is.Not.Null);
        Assert.That(deserializedChild2.StringProperties, Does.ContainKey("Title"));
        Assert.That(deserializedChild2.StringProperties, Does.ContainKey("Description"));
        Assert.That(deserializedChild2.DateTimeProperties, Does.ContainKey("CreatedAt"));
        Assert.That(deserializedChild2.TimeSpanProperties, Does.ContainKey("Duration"));
    }

    [Test]
    public void ArtifactNode_AddChild()
    {
        var child1 = new ArtifactNode()
            .Set("Title", "Child 1 Title")
            .Set("Description", "Child 1 Description")
            .Set("CreatedAt", DateTime.UtcNow)
            .Set("Duration", TimeSpan.FromHours(1));

        var child2 = new ArtifactNode()
            .Set("Title", "Child 2 Title")
            .Set("Description", "Child 2 Description")
            .Set("CreatedAt", DateTime.UtcNow.AddHours(1))
            .Set("Duration", TimeSpan.FromHours(2));

        var artifact = new Artifact()
            .Set("Title", "Artifact Root Title")
            .Set("Description", "Artifact Root Description")
            .AddChild(child1)
            .AddChild(child2);

        var json = artifact.Serialize();
        Assert.That(json, Does.Contain("Title"));
        Assert.That(json, Does.Contain("Description"));
        Assert.That(json, Does.Contain("CreatedAt"));
        Assert.That(json, Does.Contain("Duration"));

        var deserializedArtifact = Artifact.Deserialize(json);
        Assert.That(deserializedArtifact, Is.Not.Null);
        Assert.That(deserializedArtifact.StringProperties, Does.ContainKey("Title"));
        Assert.That(deserializedArtifact.StringProperties, Does.ContainKey("Description"));

        var deserializedChild1 = deserializedArtifact.Children[0] as ArtifactNode;
        Assert.That(deserializedChild1, Is.Not.Null);
        Assert.That(deserializedChild1.StringProperties, Does.ContainKey("Title"));
        Assert.That(deserializedChild1.StringProperties, Does.ContainKey("Description"));
        Assert.That(deserializedChild1.DateTimeProperties, Does.ContainKey("CreatedAt"));
        Assert.That(deserializedChild1.TimeSpanProperties, Does.ContainKey("Duration"));

        var deserializedChild2 = deserializedArtifact.Children[0] as ArtifactNode;
        Assert.That(deserializedChild2, Is.Not.Null);
        Assert.That(deserializedChild2.StringProperties, Does.ContainKey("Title"));
        Assert.That(deserializedChild2.StringProperties, Does.ContainKey("Description"));
        Assert.That(deserializedChild2.DateTimeProperties, Does.ContainKey("CreatedAt"));
        Assert.That(deserializedChild2.TimeSpanProperties, Does.ContainKey("Duration"));
    }

    [Test]
    public void Artifact_ArtifactType_Properties()
    {
        var artifactId = Guid.NewGuid().ToString();
        var artifactType = "TestArtifact";

        var artifact = new Artifact()
            .SetArtifactType(artifactType)
            .Set("Title", "Artifact Root Title")
            .Set("Description", "Artifact Root Description");

        var child1 = new ArtifactNode()
            .SetParent(artifact)
            .Set("Title", "Child 1 Title")
            .Set("Description", "Child 1 Description")
            .Set("CreatedAt", DateTime.UtcNow)
            .Set("Duration", TimeSpan.FromHours(1));

        var child2 = new ArtifactNode()
            .SetParent(artifact)
            .Set("Title", "Child 2 Title")
            .Set("Description", "Child 2 Description")
            .Set("CreatedAt", DateTime.UtcNow.AddHours(1))
            .Set("Duration", TimeSpan.FromHours(2));

        Assert.That(artifact.ArtifactType, Is.EqualTo(artifactType));
        Assert.That(child1.ArtifactType, Is.EqualTo(artifactType));
        Assert.That(child2.ArtifactType, Is.EqualTo(artifactType));
    }

    [Test]
    public void Artifact_TreeView_DoesntContainEmptyCollectionProperties()
    {
        var artifactId = Guid.NewGuid().ToString();
        var artifactType = "TestArtifact";
        var embeddings = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 };

        var artifact = new Artifact()
            .SetArtifactType(artifactType)
            .Set("Title", "Artifact Root Title")
            .Set("Description", "Artifact Root Description")
            .Set("TitleEmbeddings", embeddings);

        var child1 = new ArtifactNode()
            .SetParent(artifact)
            .Set("Title", "Child 1 Title")
            .Set("Description", "Child 1 Description")
            .Set("CreatedAt", DateTime.UtcNow)
            .Set("Duration", TimeSpan.FromHours(1))
            .Set("TitleEmbeddings", embeddings);

        var child2 = new ArtifactNode()
            .SetParent(artifact)
            .Set("Title", "Child 2 Title")
            .Set("Description", "Child 2 Description")
            .Set("CreatedAt", DateTime.UtcNow.AddHours(1))
            .Set("Duration", TimeSpan.FromHours(2))
            .Set("TitleEmbeddings", embeddings);

        var view = artifact.CreateView();
        var json = TreeViewJsonSerializer.Serialize(view, Artifact.PayloadTypes);

        var propertyExists = JsonUtilities.DoesSiblingPropertyExistForNode(json, child1.NodeId, nameof(child1.DecimalProperties));
        Assert.That(propertyExists, Is.EqualTo(false));
    }

    [Test]
    public void Artifact_ExcludesWholeDictionaryProperty_VectorProperties()
    {
        // Arrange
        var embeddings = new double[] { 0.1, 0.2, 0.3 };
        var artifact = new Artifact()
            .SetArtifactType("TestArtifact")
            .Set("Title", "Root Title")
            .Set("Description", "Root Description")
            .Set("TitleEmbeddings", embeddings); // This goes into VectorProperties

        // Build a tree view with the artifact as root.
        var view = new TreeView(artifact)
        {
            // Exclude the whole dictionary "VectorProperties"
            ExcludedProperties = new List<string> { "VectorProperties" }
        };

        // Act
        var json = TreeViewJsonSerializer.Serialize(view, Artifact.PayloadTypes);
        JsonObject jsonObj = JsonNode.Parse(json)!.AsObject();

        // Assert: The output should not contain "VectorProperties"
        Assert.That(jsonObj.ContainsKey("VectorProperties"), Is.False);
    }

    [Test]
    public void Artifact_ExcludesNestedProperty_StringProperties_Title()
    {
        // Arrange
        var artifact = new Artifact()
            .SetArtifactType("TestArtifact")
            .Set("Title", "Root Title")
            .Set("Subtitle", "Root Subtitle");

        // Build a tree view with the artifact as root.
        var view = new TreeView(artifact)
        {
            // Exclude only the "Title" entry within StringProperties.
            ExcludedProperties = new List<string> { "StringProperties.Title" }
        };

        // Act
        var json = TreeViewJsonSerializer.Serialize(view, Artifact.PayloadTypes);
        JsonObject jsonObj = JsonNode.Parse(json)!.AsObject();

        // Assert:
        // The top-level should contain StringProperties.
        Assert.That(jsonObj.ContainsKey("StringProperties"), Is.True);
        var stringProps = jsonObj["StringProperties"]!.AsObject();

        // The "Title" key should be excluded.
        Assert.That(stringProps.ContainsKey("Title"), Is.False);
        // The "Subtitle" key should remain.
        Assert.That(stringProps.ContainsKey("Subtitle"), Is.True);
    }

    [Test]
    public void Artifact_IncludedProperties_OnlyIncludesSpecifiedProperties()
    {
        // Arrange
        var embeddings = new double[] { 0.1, 0.2, 0.3 };
        var artifact = new Artifact()
            .SetArtifactType("TestArtifact")
            .Set("Title", "Root Title")
            .Set("Subtitle", "Root Subtitle")
            .Set("TitleEmbeddings", embeddings);

        // Build a tree view with the artifact as root.
        // Here we only want NodeId, StringProperties.Title, and IsExpanded/ChildrenCount.
        var view = new TreeView(artifact)
        {
            IncludedProperties = new List<string>
            {
                "NodeId",
                "StringProperties.Title",
                "IsExpanded",
                "ChildrenCount"
            }
        };

        // Act
        var json = TreeViewJsonSerializer.Serialize(view, Artifact.PayloadTypes);
        JsonObject jsonObj = JsonNode.Parse(json)!.AsObject();

        // Assert:
        // Always present because of inclusion list.
        Assert.That(jsonObj.ContainsKey("NodeId"), Is.True);
        Assert.That(jsonObj.ContainsKey("IsExpanded"), Is.True);
        Assert.That(jsonObj.ContainsKey("ChildrenCount"), Is.True);

        // For StringProperties, only "Title" should be included.
        Assert.That(jsonObj.ContainsKey("StringProperties"), Is.True);
        var stringProps = jsonObj["StringProperties"]!.AsObject();
        Assert.That(stringProps.ContainsKey("Title"), Is.True);
        Assert.That(stringProps.ContainsKey("Subtitle"), Is.False);

        // Other dictionary properties (like VectorProperties) should be excluded.
        Assert.That(jsonObj.ContainsKey("VectorProperties"), Is.False);
    }

    [Test]
    public void ArtifactType_PropagatesFromRootToDescendants()
    {
        // Arrange: Create a root artifact with ArtifactType
        var rootArtifact = new Artifact()
            .SetArtifactType("Document")
            .Set("Title", "Root Document");

        // Create child nodes at different levels
        var child1 = new ArtifactNode()
            .SetParent(rootArtifact)
            .Set("Title", "Chapter 1");

        var child2 = new ArtifactNode()
            .SetParent(rootArtifact)
            .Set("Title", "Chapter 2");

        var grandchild1 = new ArtifactNode()
            .SetParent(child1)
            .Set("Title", "Section 1.1");

        var grandchild2 = new ArtifactNode()
            .SetParent(child1)
            .Set("Title", "Section 1.2");

        var greatGrandchild = new ArtifactNode()
            .SetParent(grandchild1)
            .Set("Title", "Subsection 1.1.1");

        // Act & Assert: All descendants should return the same ArtifactType as root
        Assert.That(rootArtifact.ArtifactType, Is.EqualTo("Document"));
        Assert.That(child1.ArtifactType, Is.EqualTo("Document"));
        Assert.That(child2.ArtifactType, Is.EqualTo("Document"));
        Assert.That(grandchild1.ArtifactType, Is.EqualTo("Document"));
        Assert.That(grandchild2.ArtifactType, Is.EqualTo("Document"));
        Assert.That(greatGrandchild.ArtifactType, Is.EqualTo("Document"));
    }

    [Test]
    public void ArtifactType_SettingOnRootUpdatesAllDescendants()
    {
        // Arrange: Create a tree structure first
        var rootArtifact = new Artifact()
            .Set("Title", "Root Document");

        var child1 = new ArtifactNode()
            .SetParent(rootArtifact)
            .Set("Title", "Chapter 1");

        var child2 = new ArtifactNode()
            .SetParent(rootArtifact)
            .Set("Title", "Chapter 2");

        var grandchild = new ArtifactNode()
            .SetParent(child1)
            .Set("Title", "Section 1.1");

        // Initially, ArtifactType should be empty
        Assert.That(rootArtifact.ArtifactType, Is.EqualTo(string.Empty));
        Assert.That(child1.ArtifactType, Is.EqualTo(string.Empty));
        Assert.That(child2.ArtifactType, Is.EqualTo(string.Empty));
        Assert.That(grandchild.ArtifactType, Is.EqualTo(string.Empty));

        // Act: Set ArtifactType on root
        rootArtifact.SetArtifactType("ResearchPaper");

        // Assert: All nodes should now have the new ArtifactType
        Assert.That(rootArtifact.ArtifactType, Is.EqualTo("ResearchPaper"));
        Assert.That(child1.ArtifactType, Is.EqualTo("ResearchPaper"));
        Assert.That(child2.ArtifactType, Is.EqualTo("ResearchPaper"));
        Assert.That(grandchild.ArtifactType, Is.EqualTo("ResearchPaper"));
    }

    [Test]
    public void ArtifactType_SettingOnDescendantUpdatesRoot()
    {
        // Arrange: Create a tree structure
        var rootArtifact = new Artifact()
            .Set("Title", "Root Document");

        var child1 = new ArtifactNode()
            .SetParent(rootArtifact)
            .Set("Title", "Chapter 1");

        var grandchild = new ArtifactNode()
            .SetParent(child1)
            .Set("Title", "Section 1.1");

        // Act: Set ArtifactType on a descendant
        grandchild.SetArtifactType("TechnicalManual");

        // Assert: Root and all descendants should have the new ArtifactType
        Assert.That(rootArtifact.ArtifactType, Is.EqualTo("TechnicalManual"));
        Assert.That(child1.ArtifactType, Is.EqualTo("TechnicalManual"));
        Assert.That(grandchild.ArtifactType, Is.EqualTo("TechnicalManual"));
    }

    [Test]
    public void CreateView_ExcludesPayloadTypeAndVectorProperties()
    {
        // Arrange: Create an artifact with various properties
        var embeddings = new double[] { 0.1, 0.2, 0.3 };
        var artifact = new Artifact()
            .SetArtifactType("TestArtifact")
            .Set("Title", "Test Title")
            .Set("Description", "Test Description")
            .Set("Embeddings", embeddings);

        // Act: Create a view
        var view = artifact.CreateView();

        // Assert: ExcludedProperties should contain PayloadType and VectorProperties
        Assert.That(view.ExcludedProperties, Does.Contain(nameof(ArtifactNode.PayloadType)));
        Assert.That(view.ExcludedProperties, Does.Contain(nameof(ArtifactNode.VectorProperties)));
        Assert.That(view.ExcludedProperties.Count, Is.EqualTo(2));
    }

    [Test]
    public void CreateView_ExcludesOnlySpecifiedProperties()
    {
        // Arrange: Create an artifact with various properties
        var embeddings = new double[] { 0.1, 0.2, 0.3 };
        var artifact = new Artifact()
            .SetArtifactType("TestArtifact")
            .Set("Title", "Test Title")
            .Set("Description", "Test Description")
            .Set("CreatedAt", DateTime.UtcNow)
            .Set("Embeddings", embeddings);

        // Act: Create a view
        var view = artifact.CreateView();

        // Assert: Only PayloadType and VectorProperties should be excluded
        // Other properties like StringProperties, DateTimeProperties should not be excluded
        Assert.That(view.ExcludedProperties, Does.Not.Contain(nameof(ArtifactNode.StringProperties)));
        Assert.That(view.ExcludedProperties, Does.Not.Contain(nameof(ArtifactNode.DateTimeProperties)));
        Assert.That(view.ExcludedProperties, Does.Not.Contain(nameof(ArtifactNode.LongProperties)));
        Assert.That(view.ExcludedProperties, Does.Not.Contain(nameof(ArtifactNode.DoubleProperties)));
        Assert.That(view.ExcludedProperties, Does.Not.Contain(nameof(ArtifactNode.DecimalProperties)));
        Assert.That(view.ExcludedProperties, Does.Not.Contain(nameof(ArtifactNode.TimeSpanProperties)));
    }
}
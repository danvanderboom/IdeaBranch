using System;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Storage;
using IdeaBranch.IntegrationTests.Storage;
using NUnit.Framework;

namespace IdeaBranch.IntegrationTests.Storage;

[TestFixture]
public class TagTaxonomyRepositoryTests
{
    private SqliteTestDatabase _testDb = null!;
    private SqliteTagTaxonomyRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _testDb = new SqliteTestDatabase();
        // Use TopicDb to apply all migrations
        using var topicDb = new TopicDb($"Data Source={_testDb.DbPath}");
        _repository = new SqliteTagTaxonomyRepository(topicDb.Connection);
        
        // Reopen connection after TopicDb is disposed
        _testDb.ReopenConnection();
        _repository = new SqliteTagTaxonomyRepository(_testDb.Connection);
    }

    [TearDown]
    public void TearDown()
    {
        _testDb?.Dispose();
    }

    [Test]
    public async Task GetRootAsync_ShouldCreateDefaultRootIfNoneExists()
    {
        // Act
        var root = await _repository.GetRootAsync();

        // Assert
        Assert.That(root, Is.Not.Null);
        Assert.That(root.ParentId, Is.Null);
        Assert.That(root.Name, Is.EqualTo("Root"));
    }

    [Test]
    public async Task GetRootAsync_ShouldReturnExistingRoot()
    {
        // Arrange - Get root first time (creates default)
        var root1 = await _repository.GetRootAsync();
        
        // Act - Get root second time
        var root2 = await _repository.GetRootAsync();

        // Assert
        Assert.That(root2.Id, Is.EqualTo(root1.Id));
        Assert.That(root2.Name, Is.EqualTo(root1.Name));
    }

    [Test]
    public async Task SaveAsync_ShouldPersistTagTaxonomyNode()
    {
        // Arrange
        var node = new TagTaxonomyNode("Test Tag", null);

        // Act
        await _repository.SaveAsync(node);

        // Assert
        var retrieved = await _repository.GetByIdAsync(node.Id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Name, Is.EqualTo("Test Tag"));
        Assert.That(retrieved.ParentId, Is.Null);
    }

    [Test]
    public async Task SaveAsync_ShouldSaveHierarchicalStructure()
    {
        // Arrange
        var root = new TagTaxonomyNode("Root", null);
        var category = new TagTaxonomyNode("Category", root.Id);
        var tag = new TagTaxonomyNode("Tag", category.Id);
        
        root.AddChild(category);
        category.AddChild(tag);

        // Act
        await _repository.SaveAsync(root);

        // Assert
        var retrievedRoot = await _repository.GetByIdAsync(root.Id);
        Assert.That(retrievedRoot, Is.Not.Null);
        Assert.That(retrievedRoot!.Name, Is.EqualTo("Root"));

        var retrievedCategory = await _repository.GetByIdAsync(category.Id);
        Assert.That(retrievedCategory, Is.Not.Null);
        Assert.That(retrievedCategory!.Name, Is.EqualTo("Category"));
        Assert.That(retrievedCategory.ParentId, Is.EqualTo(root.Id));

        var retrievedTag = await _repository.GetByIdAsync(tag.Id);
        Assert.That(retrievedTag, Is.Not.Null);
        Assert.That(retrievedTag!.Name, Is.EqualTo("Tag"));
        Assert.That(retrievedTag.ParentId, Is.EqualTo(category.Id));
    }

    [Test]
    public async Task GetChildrenAsync_ShouldReturnChildrenOrderedByOrder()
    {
        // Arrange
        var root = await _repository.GetRootAsync();
        var child1 = new TagTaxonomyNode("Child 1", root.Id) { Order = 2 };
        var child2 = new TagTaxonomyNode("Child 2", root.Id) { Order = 1 };
        root.AddChild(child1);
        root.AddChild(child2);
        await _repository.SaveAsync(root);

        // Act
        var children = await _repository.GetChildrenAsync(root.Id);

        // Assert
        Assert.That(children.Count, Is.EqualTo(2));
        Assert.That(children[0].Name, Is.EqualTo("Child 2")); // Order 1 comes first
        Assert.That(children[1].Name, Is.EqualTo("Child 1")); // Order 2 comes second
    }

    [Test]
    public async Task GetChildrenAsync_WithRootId_ShouldReturnRootChildren()
    {
        // Arrange
        var root = await _repository.GetRootAsync();
        var child = new TagTaxonomyNode("Child", root.Id);
        root.AddChild(child);
        await _repository.SaveAsync(root);

        // Act
        var children = await _repository.GetChildrenAsync(root.Id);

        // Assert
        var testChild = children.FirstOrDefault(c => c.Name == "Child");
        Assert.That(testChild, Is.Not.Null);
        Assert.That(testChild!.ParentId, Is.EqualTo(root.Id));
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveTagTaxonomyNode()
    {
        // Arrange
        var node = new TagTaxonomyNode("Test Tag", null);
        await _repository.SaveAsync(node);

        // Act
        var deleted = await _repository.DeleteAsync(node.Id);

        // Assert
        Assert.That(deleted, Is.True);
        var retrieved = await _repository.GetByIdAsync(node.Id);
        Assert.That(retrieved, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldCascadeDeleteChildren()
    {
        // Arrange
        var root = new TagTaxonomyNode("Root", null);
        var child1 = new TagTaxonomyNode("Child 1", root.Id);
        var child2 = new TagTaxonomyNode("Child 2", root.Id);
        root.AddChild(child1);
        root.AddChild(child2);
        await _repository.SaveAsync(root);

        // Verify children exist
        var childrenBefore = await _repository.GetChildrenAsync(root.Id);
        Assert.That(childrenBefore.Count, Is.EqualTo(2));

        // Act - Delete root
        var deleted = await _repository.DeleteAsync(root.Id);

        // Assert - Root deleted
        Assert.That(deleted, Is.True);
        var retrievedRoot = await _repository.GetByIdAsync(root.Id);
        Assert.That(retrievedRoot, Is.Null);

        // Assert - Children should be deleted by cascade (foreign key constraint)
        var childrenAfter = await _repository.GetChildrenAsync(root.Id);
        Assert.That(childrenAfter.Count, Is.EqualTo(0));
        var retrievedChild1 = await _repository.GetByIdAsync(child1.Id);
        var retrievedChild2 = await _repository.GetByIdAsync(child2.Id);
        Assert.That(retrievedChild1, Is.Null);
        Assert.That(retrievedChild2, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldCascadeRemoveFromAnnotationTags()
    {
        // Arrange
        var tag = new TagTaxonomyNode("Test Tag", null);
        await _repository.SaveAsync(tag);

        // Create a test topic node and annotation
        var testNodeId = Guid.NewGuid();
        using var command = _testDb.Connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO topic_nodes (NodeId, ParentId, Title, Prompt, Response, Ordinal, CreatedAt, UpdatedAt)
            VALUES (@NodeId, NULL, @Title, @Prompt, @Response, 0, @CreatedAt, @UpdatedAt)
        ";
        command.Parameters.AddWithValue("@NodeId", testNodeId.ToString());
        command.Parameters.AddWithValue("@Title", "Test Node");
        command.Parameters.AddWithValue("@Prompt", "Test Prompt");
        command.Parameters.AddWithValue("@Response", "Test Response");
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();

        var annotationId = Guid.NewGuid();
        command.CommandText = @"
            INSERT INTO annotations (Id, NodeId, StartOffset, EndOffset, Comment, CreatedAt, UpdatedAt)
            VALUES (@Id, @NodeId, 0, 10, NULL, @CreatedAt, @UpdatedAt)
        ";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@Id", annotationId.ToString());
        command.Parameters.AddWithValue("@NodeId", testNodeId.ToString());
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();

        // Associate tag with annotation
        command.CommandText = @"
            INSERT INTO annotation_tags (AnnotationId, TagId)
            VALUES (@AnnotationId, @TagId)
        ";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@AnnotationId", annotationId.ToString());
        command.Parameters.AddWithValue("@TagId", tag.Id.ToString());
        command.ExecuteNonQuery();

        // Verify association exists
        command.CommandText = "SELECT COUNT(*) FROM annotation_tags WHERE TagId = @TagId";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@TagId", tag.Id.ToString());
        var countBefore = Convert.ToInt32(command.ExecuteScalar());
        Assert.That(countBefore, Is.EqualTo(1));

        // Act - Delete tag
        var deleted = await _repository.DeleteAsync(tag.Id);

        // Assert - Tag deleted
        Assert.That(deleted, Is.True);
        var retrievedTag = await _repository.GetByIdAsync(tag.Id);
        Assert.That(retrievedTag, Is.Null);

        // Assert - Association removed by cascade
        command.CommandText = "SELECT COUNT(*) FROM annotation_tags WHERE TagId = @TagId";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@TagId", tag.Id.ToString());
        var countAfter = Convert.ToInt32(command.ExecuteScalar());
        Assert.That(countAfter, Is.EqualTo(0));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNullForNonExistentNode()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SearchAsync_WithNameContains_ReturnsMatchingTags()
    {
        // Arrange
        var tag1 = new TagTaxonomyNode("Kitchen", null);
        var tag2 = new TagTaxonomyNode("Bedroom", null);
        var tag3 = new TagTaxonomyNode("Bathroom", null);
        await _repository.SaveAsync(tag1);
        await _repository.SaveAsync(tag2);
        await _repository.SaveAsync(tag3);

        // Act
        var results = await _repository.SearchAsync(nameContains: "room");

        // Assert
        Assert.That(results.Count, Is.EqualTo(2));
        var names = results.Select(r => r.Name).ToHashSet();
        Assert.That(names, Contains.Item("Bedroom"));
        Assert.That(names, Contains.Item("Bathroom"));
        Assert.That(names, Does.Not.Contain("Kitchen"));
    }

    [Test]
    public async Task SearchAsync_WithUpdatedAtRange_ReturnsTagsInTimeRange()
    {
        // Arrange
        var tag1 = new TagTaxonomyNode("Tag1", null);
        await _repository.SaveAsync(tag1);

        // Capture time after first save, then save tag2
        var cutoffTime = DateTime.UtcNow;
        await Task.Delay(10); // Small delay to ensure different timestamps
        var tag2 = new TagTaxonomyNode("Tag2", null);
        await _repository.SaveAsync(tag2);

        // Act
        var results = await _repository.SearchAsync(updatedAtFrom: cutoffTime); // Match items updated after the cutoff time

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Name, Is.EqualTo("Tag2"));
    }

    [Test]
    public async Task SearchAsync_WithNoFilters_ReturnsAllTags()
    {
        // Arrange
        var tag1 = new TagTaxonomyNode("Tag1", null);
        var tag2 = new TagTaxonomyNode("Tag2", null);
        await _repository.SaveAsync(tag1);
        await _repository.SaveAsync(tag2);

        // Act
        var results = await _repository.SearchAsync();

        // Assert - Should return all tags (including default root if it exists)
        Assert.That(results.Count, Is.GreaterThanOrEqualTo(2));
        var names = results.Select(r => r.Name).ToHashSet();
        Assert.That(names, Contains.Item("Tag1"));
        Assert.That(names, Contains.Item("Tag2"));
    }
}


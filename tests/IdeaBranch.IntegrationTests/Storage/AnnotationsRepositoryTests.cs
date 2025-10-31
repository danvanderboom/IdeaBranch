using System;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Storage;
using IdeaBranch.IntegrationTests.Storage;
using NUnit.Framework;

namespace IdeaBranch.IntegrationTests.Storage;

[TestFixture]
public class AnnotationsRepositoryTests
{
    private SqliteTestDatabase _testDb = null!;
    private SqliteAnnotationsRepository _repository = null!;
    private Guid _testNodeId;

    [SetUp]
    public void SetUp()
    {
        _testDb = new SqliteTestDatabase();
        // Use TopicDb to apply all migrations
        using var topicDb = new TopicDb($"Data Source={_testDb.DbPath}");
        _repository = new SqliteAnnotationsRepository(topicDb.Connection);
        
        // Create a test topic node to reference
        _testNodeId = Guid.NewGuid();
        using var command = topicDb.Connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO topic_nodes (NodeId, ParentId, Title, Prompt, Response, Ordinal, CreatedAt, UpdatedAt)
            VALUES (@NodeId, NULL, @Title, @Prompt, @Response, 0, @CreatedAt, @UpdatedAt)
        ";
        command.Parameters.AddWithValue("@NodeId", _testNodeId.ToString());
        command.Parameters.AddWithValue("@Title", "Test Node");
        command.Parameters.AddWithValue("@Prompt", "Test Prompt");
        command.Parameters.AddWithValue("@Response", "Test Response Text");
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();
        
        // Reopen connection after TopicDb is disposed
        _testDb.ReopenConnection();
        _repository = new SqliteAnnotationsRepository(_testDb.Connection);
    }

    [TearDown]
    public void TearDown()
    {
        _testDb?.Dispose();
    }

    [Test]
    public async Task SaveAsync_ShouldPersistAnnotation()
    {
        // Arrange
        var annotation = new Annotation(_testNodeId, 5, 15, "Test comment");

        // Act
        await _repository.SaveAsync(annotation);

        // Assert
        var retrieved = await _repository.GetByIdAsync(annotation.Id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.NodeId, Is.EqualTo(_testNodeId));
        Assert.That(retrieved.StartOffset, Is.EqualTo(5));
        Assert.That(retrieved.EndOffset, Is.EqualTo(15));
        Assert.That(retrieved.Comment, Is.EqualTo("Test comment"));
    }

    [Test]
    public async Task GetByNodeIdAsync_ShouldReturnAllAnnotationsForNode()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10, "First annotation");
        var annotation2 = new Annotation(_testNodeId, 20, 30, "Second annotation");
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);

        // Act
        var annotations = await _repository.GetByNodeIdAsync(_testNodeId);

        // Assert
        Assert.That(annotations.Count, Is.EqualTo(2));
        Assert.That(annotations.Any(a => a.Id == annotation1.Id), Is.True);
        Assert.That(annotations.Any(a => a.Id == annotation2.Id), Is.True);
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveAnnotation()
    {
        // Arrange
        var annotation = new Annotation(_testNodeId, 0, 10);
        await _repository.SaveAsync(annotation);

        // Act
        var deleted = await _repository.DeleteAsync(annotation.Id);

        // Assert
        Assert.That(deleted, Is.True);
        var retrieved = await _repository.GetByIdAsync(annotation.Id);
        Assert.That(retrieved, Is.Null);
    }

    [Test]
    public async Task AddTagAsync_ShouldAssociateTagWithAnnotation()
    {
        // Arrange
        var annotation = new Annotation(_testNodeId, 0, 10);
        await _repository.SaveAsync(annotation);
        
        // Create a test tag taxonomy node
        var tagId = Guid.NewGuid();
        using var command = _testDb.Connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO tag_taxonomy_nodes (Id, ParentId, Name, ""Order"", CreatedAt, UpdatedAt)
            VALUES (@Id, NULL, @Name, 0, @CreatedAt, @UpdatedAt)
        ";
        command.Parameters.AddWithValue("@Id", tagId.ToString());
        command.Parameters.AddWithValue("@Name", "Test Tag");
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();

        // Act
        await _repository.AddTagAsync(annotation.Id, tagId);

        // Assert
        var tagIds = await _repository.GetTagIdsAsync(annotation.Id);
        Assert.That(tagIds.Count, Is.EqualTo(1));
        Assert.That(tagIds[0], Is.EqualTo(tagId));
    }

    [Test]
    public async Task SaveValueAsync_ShouldPersistAnnotationValue()
    {
        // Arrange
        var annotation = new Annotation(_testNodeId, 0, 10);
        await _repository.SaveAsync(annotation);
        var value = new AnnotationValue(annotation.Id, "numeric")
        {
            NumericValue = 42.5
        };

        // Act
        await _repository.SaveValueAsync(value);

        // Assert
        var values = await _repository.GetValuesAsync(annotation.Id);
        Assert.That(values.Count, Is.EqualTo(1));
        Assert.That(values[0].ValueType, Is.EqualTo("numeric"));
        Assert.That(values[0].NumericValue, Is.EqualTo(42.5));
    }
}


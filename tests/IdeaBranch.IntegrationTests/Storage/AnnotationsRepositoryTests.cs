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

    [Test]
    public async Task SearchAsync_WithIncludeTags_ReturnsAnnotationsWithAllTags()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10, "Annotation 1");
        var annotation2 = new Annotation(_testNodeId, 20, 30, "Annotation 2");
        var annotation3 = new Annotation(_testNodeId, 40, 50, "Annotation 3");
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);
        await _repository.SaveAsync(annotation3);

        var tag1 = CreateTestTag("Tag1");
        var tag2 = CreateTestTag("Tag2");

        // Annotation1 has both tags
        await _repository.AddTagAsync(annotation1.Id, tag1);
        await _repository.AddTagAsync(annotation1.Id, tag2);

        // Annotation2 has only tag1
        await _repository.AddTagAsync(annotation2.Id, tag1);

        // Annotation3 has no tags

        var options = new AnnotationsSearchOptions
        {
            IncludeTags = new[] { tag1, tag2 }
        };

        // Act
        var results = await _repository.SearchAsync(_testNodeId, options);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(annotation1.Id));
    }

    [Test]
    public async Task SearchAsync_WithExcludeTags_ExcludesAnnotationsWithThoseTags()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10, "Annotation 1");
        var annotation2 = new Annotation(_testNodeId, 20, 30, "Annotation 2");
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);

        var tag1 = CreateTestTag("Tag1");
        var tag2 = CreateTestTag("Tag2");

        await _repository.AddTagAsync(annotation1.Id, tag1);
        await _repository.AddTagAsync(annotation2.Id, tag2);

        var options = new AnnotationsSearchOptions
        {
            ExcludeTags = new[] { tag2 }
        };

        // Act
        var results = await _repository.SearchAsync(_testNodeId, options);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(annotation1.Id));
    }

    [Test]
    public async Task SearchAsync_WithTagWeightFilters_ReturnsAnnotationsMatchingWeightCriteria()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10);
        var annotation2 = new Annotation(_testNodeId, 20, 30);
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);

        var tag1 = CreateTestTag("Tag1");

        await _repository.AddTagAsync(annotation1.Id, tag1);
        await _repository.AddTagAsync(annotation2.Id, tag1);

        // Set weights
        await _repository.SetTagWeightAsync(annotation1.Id, tag1, 5.0);
        await _repository.SetTagWeightAsync(annotation2.Id, tag1, 2.0);

        var options = new AnnotationsSearchOptions
        {
            TagWeightFilters = new[]
            {
                new TagWeightFilter { TagId = tag1, Op = "gt", Value = 3.0 }
            }
        };

        // Act
        var results = await _repository.SearchAsync(_testNodeId, options);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(annotation1.Id));
    }

    [Test]
    public async Task SearchAsync_WithCommentContains_ReturnsAnnotationsWithMatchingComment()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10, "This is a test comment");
        var annotation2 = new Annotation(_testNodeId, 20, 30, "Another annotation");
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);

        var options = new AnnotationsSearchOptions
        {
            CommentContains = "test"
        };

        // Act
        var results = await _repository.SearchAsync(_testNodeId, options);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(annotation1.Id));
    }

    [Test]
    public async Task SearchAsync_WithUpdatedAtRange_ReturnsAnnotationsInTimeRange()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10);
        var annotation2 = new Annotation(_testNodeId, 20, 30);
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);

        // Capture time after both saves, then update annotation2
        var cutoffTime = DateTime.UtcNow;
        await Task.Delay(10); // Small delay to ensure different timestamps
        annotation2 = new Annotation(annotation2.Id, annotation2.NodeId, annotation2.StartOffset, annotation2.EndOffset, annotation2.CreatedAt, DateTime.UtcNow, annotation2.Comment);
        await _repository.SaveAsync(annotation2);

        var options = new AnnotationsSearchOptions
        {
            UpdatedAtFrom = cutoffTime // Match items updated after the cutoff time
        };

        // Act
        var results = await _repository.SearchAsync(_testNodeId, options);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(annotation2.Id));
    }

    [Test]
    public async Task SearchAsync_WithTemporalRange_ReturnsAnnotationsWithTemporalValuesInRange()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10);
        var annotation2 = new Annotation(_testNodeId, 20, 30);
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);

        var temporal1 = new AnnotationValue(annotation1.Id, "temporal")
        {
            TemporalValue = "2020-01-01T00:00:00Z"
        };
        var temporal2 = new AnnotationValue(annotation2.Id, "temporal")
        {
            TemporalValue = "2021-06-15T12:00:00Z"
        };
        await _repository.SaveValueAsync(temporal1);
        await _repository.SaveValueAsync(temporal2);

        var options = new AnnotationsSearchOptions
        {
            TemporalStart = DateTime.Parse("2021-01-01T00:00:00Z"),
            TemporalEnd = DateTime.Parse("2022-01-01T00:00:00Z")
        };

        // Act
        var results = await _repository.SearchAsync(_testNodeId, options);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(annotation2.Id));
    }

    private Guid CreateTestTag(string name)
    {
        var tagId = Guid.NewGuid();
        using var command = _testDb.Connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO tag_taxonomy_nodes (Id, ParentId, Name, ""Order"", CreatedAt, UpdatedAt)
            VALUES (@Id, NULL, @Name, 0, @CreatedAt, @UpdatedAt)
        ";
        command.Parameters.AddWithValue("@Id", tagId.ToString());
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();
        return tagId;
    }
}


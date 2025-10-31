using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Storage;
using IdeaBranch.IntegrationTests.Storage;
using NUnit.Framework;

namespace IdeaBranch.IntegrationTests.Storage;

[TestFixture]
public class AnnotationsRepositoryRangeQueryTests
{
    private SqliteTestDatabase _testDb = null!;
    private SqliteAnnotationsRepository _repository = null!;
    private Guid _testNodeId;
    private Guid _tag1Id;
    private Guid _tag2Id;

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
        
        // Create test tag taxonomy nodes
        _tag1Id = Guid.NewGuid();
        _tag2Id = Guid.NewGuid();
        command.CommandText = @"
            INSERT INTO tag_taxonomy_nodes (Id, ParentId, Name, ""Order"", CreatedAt, UpdatedAt)
            VALUES (@Id1, NULL, @Name1, 0, @CreatedAt, @UpdatedAt),
                   (@Id2, NULL, @Name2, 0, @CreatedAt, @UpdatedAt)
        ";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@Id1", _tag1Id.ToString());
        command.Parameters.AddWithValue("@Id2", _tag2Id.ToString());
        command.Parameters.AddWithValue("@Name1", "Tag 1");
        command.Parameters.AddWithValue("@Name2", "Tag 2");
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
    public async Task QueryAsync_WithTagFilter_ShouldMatchGetByNodeIdAndTagsAsync()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10);
        var annotation2 = new Annotation(_testNodeId, 20, 30);
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);
        await _repository.AddTagAsync(annotation1.Id, _tag1Id);
        await _repository.AddTagAsync(annotation2.Id, _tag2Id);

        // Act
        var queryResults = await _repository.QueryAsync(_testNodeId, tagIds: new[] { _tag1Id });
        var legacyResults = await _repository.GetByNodeIdAndTagsAsync(_testNodeId, new[] { _tag1Id });

        // Assert
        Assert.That(queryResults.Count, Is.EqualTo(1));
        Assert.That(queryResults[0].Id, Is.EqualTo(annotation1.Id));
        Assert.That(queryResults.Count, Is.EqualTo(legacyResults.Count));
        Assert.That(queryResults[0].Id, Is.EqualTo(legacyResults[0].Id));
    }

    [Test]
    public async Task QueryAsync_WithNumericRange_ShouldFilterByNumericValue()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10);
        var annotation2 = new Annotation(_testNodeId, 20, 30);
        var annotation3 = new Annotation(_testNodeId, 40, 50);
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);
        await _repository.SaveAsync(annotation3);

        var value1 = new AnnotationValue(annotation1.Id, "numeric") { NumericValue = 25.0 };
        var value2 = new AnnotationValue(annotation2.Id, "numeric") { NumericValue = 50.0 };
        var value3 = new AnnotationValue(annotation3.Id, "numeric") { NumericValue = 75.0 };
        await _repository.SaveValueAsync(value1);
        await _repository.SaveValueAsync(value2);
        await _repository.SaveValueAsync(value3);

        // Act - Query for numeric values between 30 and 70
        var results = await _repository.QueryAsync(_testNodeId, numericMin: 30.0, numericMax: 70.0);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(annotation2.Id));
    }

    [Test]
    public async Task QueryAsync_WithNumericMinOnly_ShouldFilterByMinimumValue()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10);
        var annotation2 = new Annotation(_testNodeId, 20, 30);
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);

        var value1 = new AnnotationValue(annotation1.Id, "numeric") { NumericValue = 25.0 };
        var value2 = new AnnotationValue(annotation2.Id, "numeric") { NumericValue = 50.0 };
        await _repository.SaveValueAsync(value1);
        await _repository.SaveValueAsync(value2);

        // Act
        var results = await _repository.QueryAsync(_testNodeId, numericMin: 30.0);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(annotation2.Id));
    }

    [Test]
    public async Task QueryAsync_WithTemporalRange_ShouldFilterByTemporalValue()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10);
        var annotation2 = new Annotation(_testNodeId, 20, 30);
        var annotation3 = new Annotation(_testNodeId, 40, 50);
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);
        await _repository.SaveAsync(annotation3);

        var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var midDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var value1 = new AnnotationValue(annotation1.Id, "temporal") { TemporalValue = startDate.ToString("O") };
        var value2 = new AnnotationValue(annotation2.Id, "temporal") { TemporalValue = midDate.ToString("O") };
        var value3 = new AnnotationValue(annotation3.Id, "temporal") { TemporalValue = endDate.ToString("O") };
        await _repository.SaveValueAsync(value1);
        await _repository.SaveValueAsync(value2);
        await _repository.SaveValueAsync(value3);

        // Act - Query for temporal values between Feb 1 and Nov 1
        var filterStart = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var filterEnd = new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        var results = await _repository.QueryAsync(_testNodeId, temporalStart: filterStart, temporalEnd: filterEnd);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(annotation2.Id));
    }

    [Test]
    public async Task QueryAsync_WithGeospatialBbox_ShouldFilterByGeospatialValue()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10);
        var annotation2 = new Annotation(_testNodeId, 20, 30);
        var annotation3 = new Annotation(_testNodeId, 40, 50);
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);
        await _repository.SaveAsync(annotation3);

        // Create geospatial values as JSON
        var geo1 = JsonSerializer.Serialize(new { lat = 37.7749, lon = -122.4194 }); // San Francisco
        var geo2 = JsonSerializer.Serialize(new { lat = 40.7128, lon = -74.0060 }); // New York
        var geo3 = JsonSerializer.Serialize(new { lat = 51.5074, lon = -0.1278 }); // London

        var value1 = new AnnotationValue(annotation1.Id, "geospatial") { GeospatialValue = geo1 };
        var value2 = new AnnotationValue(annotation2.Id, "geospatial") { GeospatialValue = geo2 };
        var value3 = new AnnotationValue(annotation3.Id, "geospatial") { GeospatialValue = geo3 };
        await _repository.SaveValueAsync(value1);
        await _repository.SaveValueAsync(value2);
        await _repository.SaveValueAsync(value3);

        // Act - Query for US locations (rough bounding box)
        var results = await _repository.QueryAsync(_testNodeId, geoBbox: (35.0, -125.0, 45.0, -70.0));

        // Assert - Should return San Francisco and New York, but not London
        Assert.That(results.Count, Is.EqualTo(2));
        var resultIds = results.Select(r => r.Id).ToHashSet();
        Assert.That(resultIds, Contains.Item(annotation1.Id));
        Assert.That(resultIds, Contains.Item(annotation2.Id));
        Assert.That(resultIds, Does.Not.Contain(annotation3.Id));
    }

    [Test]
    public async Task QueryAsync_WithCombinedFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10);
        var annotation2 = new Annotation(_testNodeId, 20, 30);
        var annotation3 = new Annotation(_testNodeId, 40, 50);
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);
        await _repository.SaveAsync(annotation3);

        // Add tags
        await _repository.AddTagAsync(annotation1.Id, _tag1Id);
        await _repository.AddTagAsync(annotation2.Id, _tag1Id);
        await _repository.AddTagAsync(annotation3.Id, _tag2Id);

        // Add numeric values
        var value1 = new AnnotationValue(annotation1.Id, "numeric") { NumericValue = 25.0 };
        var value2 = new AnnotationValue(annotation2.Id, "numeric") { NumericValue = 50.0 };
        await _repository.SaveValueAsync(value1);
        await _repository.SaveValueAsync(value2);

        // Act - Query for tag1 AND numeric value >= 30
        var results = await _repository.QueryAsync(_testNodeId, tagIds: new[] { _tag1Id }, numericMin: 30.0);

        // Assert - Should only return annotation2 (has tag1 and numeric >= 30)
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(annotation2.Id));
    }

    [Test]
    public async Task QueryAsync_WithoutFilters_ShouldReturnAllAnnotationsForNode()
    {
        // Arrange
        var annotation1 = new Annotation(_testNodeId, 0, 10);
        var annotation2 = new Annotation(_testNodeId, 20, 30);
        await _repository.SaveAsync(annotation1);
        await _repository.SaveAsync(annotation2);

        // Act
        var results = await _repository.QueryAsync(_testNodeId);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2));
        var resultIds = results.Select(r => r.Id).ToHashSet();
        Assert.That(resultIds, Contains.Item(annotation1.Id));
        Assert.That(resultIds, Contains.Item(annotation2.Id));
    }
}


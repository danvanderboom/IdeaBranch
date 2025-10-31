using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Storage;
using NUnit.Framework;

namespace IdeaBranch.IntegrationTests.Storage;

/// <summary>
/// Integration tests for SqliteVersionHistoryRepository.
/// Tests the repository implementation against IVersionHistoryRepository interface.
/// </summary>
public class SqliteVersionHistoryRepositoryTests
{
    private string? _dbPath;
    private TopicDb? _db;
    private SqliteVersionHistoryRepository? _repository;

    [SetUp]
    public void SetUp()
    {
        // Create temporary database file for testing
        _dbPath = Path.Combine(Path.GetTempPath(), $"ideabranch_test_{Guid.NewGuid():N}.db");
        _db = new TopicDb($"Data Source={_dbPath}"); // TopicDb handles migration v2
        _repository = new SqliteVersionHistoryRepository(_db.Connection);
    }

    [TearDown]
    public void TearDown()
    {
        _repository = null;
        _db?.Dispose();

        // Clean up database file
        if (_dbPath != null && File.Exists(_dbPath))
        {
            try
            {
                File.Delete(_dbPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        _db = null;
        _dbPath = null;
    }

    [Test]
    public async Task SaveAsync_ShouldPersistVersionEntry()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        CreateTopicNode(nodeId, "Test Title", "Test Prompt", "Test Response");
        
        var version = new TopicNodeVersion(
            Guid.NewGuid(),
            nodeId,
            "Test Title",
            "Test Prompt",
            "Test Response",
            0,
            DateTime.UtcNow,
            "user1",
            "Test User");

        // Act
        await _repository!.SaveAsync(version);

        // Assert - Retrieve and verify
        var retrieved = await _repository.GetByNodeIdAsync(nodeId);
        retrieved.Should().NotBeEmpty();
        retrieved.Should().Contain(v => v.VersionId == version.VersionId);
        var savedVersion = retrieved.First(v => v.VersionId == version.VersionId);
        savedVersion.NodeId.Should().Be(nodeId);
        savedVersion.Title.Should().Be("Test Title");
        savedVersion.Prompt.Should().Be("Test Prompt");
        savedVersion.Response.Should().Be("Test Response");
        savedVersion.Order.Should().Be(0);
        savedVersion.AuthorId.Should().Be("user1");
        savedVersion.AuthorName.Should().Be("Test User");
    }

    [Test]
    public async Task GetByNodeIdAsync_ShouldReturnVersionsOrderedByTimestampDescending()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        CreateTopicNode(nodeId, "Title", "Prompt", "Response");
        
        var now = DateTime.UtcNow;
        var version1 = new TopicNodeVersion(Guid.NewGuid(), nodeId, "Title 1", "Prompt 1", "Response 1", 0, now.AddMinutes(-2));
        var version2 = new TopicNodeVersion(Guid.NewGuid(), nodeId, "Title 2", "Prompt 2", "Response 2", 0, now.AddMinutes(-1));
        var version3 = new TopicNodeVersion(Guid.NewGuid(), nodeId, "Title 3", "Prompt 3", "Response 3", 0, now);

        await _repository!.SaveAsync(version1);
        await _repository.SaveAsync(version2);
        await _repository.SaveAsync(version3);

        // Act
        var versions = await _repository.GetByNodeIdAsync(nodeId);

        // Assert
        versions.Should().HaveCount(3);
        versions.Should().BeInDescendingOrder(v => v.VersionTimestamp);
        
        // Verify relative ordering (timestamps may be parsed in different timezone)
        var timeDiff1 = (versions[0].VersionTimestamp - versions[1].VersionTimestamp).TotalMinutes;
        var timeDiff2 = (versions[1].VersionTimestamp - versions[2].VersionTimestamp).TotalMinutes;
        timeDiff1.Should().BeApproximately(1.0, 0.1);
        timeDiff2.Should().BeApproximately(1.0, 0.1);
    }

    [Test]
    public async Task GetByNodeIdAsync_WhenNoVersionsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var nodeId = Guid.NewGuid();

        // Act
        var versions = await _repository!.GetByNodeIdAsync(nodeId);

        // Assert
        versions.Should().BeEmpty();
    }

    [Test]
    public async Task GetLatestAsync_ShouldReturnMostRecentVersion()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        CreateTopicNode(nodeId, "Title", "Prompt", "Response");
        
        var now = DateTime.UtcNow;
        var version1 = new TopicNodeVersion(Guid.NewGuid(), nodeId, "Title 1", "Prompt 1", "Response 1", 0, now.AddMinutes(-2));
        var version2 = new TopicNodeVersion(Guid.NewGuid(), nodeId, "Title 2", "Prompt 2", "Response 2", 0, now.AddMinutes(-1));
        var version3 = new TopicNodeVersion(Guid.NewGuid(), nodeId, "Title 3", "Prompt 3", "Response 3", 0, now);

        await _repository!.SaveAsync(version1);
        await _repository.SaveAsync(version2);
        await _repository.SaveAsync(version3);

        // Act
        var latest = await _repository.GetLatestAsync(nodeId);

        // Assert
        latest.Should().NotBeNull();
        latest!.VersionId.Should().Be(version3.VersionId);
        // Verify it's the most recent by checking all versions
        var allVersions = await _repository.GetByNodeIdAsync(nodeId);
        var maxTimestamp = allVersions.Max(v => v.VersionTimestamp);
        latest.VersionTimestamp.Should().Be(maxTimestamp);
    }

    [Test]
    public async Task GetLatestAsync_WhenNoVersionsExist_ShouldReturnNull()
    {
        // Arrange
        var nodeId = Guid.NewGuid();

        // Act
        var latest = await _repository!.GetLatestAsync(nodeId);

        // Assert
        latest.Should().BeNull();
    }

    [Test]
    public async Task SaveAsync_ShouldPersistMultipleVersions()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        CreateTopicNode(nodeId, "Title", "Prompt", "Response");
        
        var version1 = new TopicNodeVersion(Guid.NewGuid(), nodeId, "Title 1", "Prompt 1", "Response 1", 0, DateTime.UtcNow);
        var version2 = new TopicNodeVersion(Guid.NewGuid(), nodeId, "Title 2", "Prompt 2", "Response 2", 0, DateTime.UtcNow);
        var version3 = new TopicNodeVersion(Guid.NewGuid(), nodeId, "Title 3", "Prompt 3", "Response 3", 0, DateTime.UtcNow);

        // Act
        await _repository!.SaveAsync(version1);
        await _repository.SaveAsync(version2);
        await _repository.SaveAsync(version3);

        // Assert
        var versions = await _repository.GetByNodeIdAsync(nodeId);
        versions.Should().HaveCount(3);
        versions.Should().Contain(v => v.VersionId == version1.VersionId);
        versions.Should().Contain(v => v.VersionId == version2.VersionId);
        versions.Should().Contain(v => v.VersionId == version3.VersionId);
    }

    [Test]
    public async Task SaveAsync_ShouldPersistAuthorInfo()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        CreateTopicNode(nodeId, "Test Title", "Test Prompt", "Test Response");
        
        var version = new TopicNodeVersion(
            Guid.NewGuid(),
            nodeId,
            "Test Title",
            "Test Prompt",
            "Test Response",
            0,
            DateTime.UtcNow,
            "author123",
            "John Doe");

        // Act
        await _repository!.SaveAsync(version);

        // Assert
        var retrieved = await _repository.GetByNodeIdAsync(nodeId);
        var savedVersion = retrieved.First(v => v.VersionId == version.VersionId);
        savedVersion.AuthorId.Should().Be("author123");
        savedVersion.AuthorName.Should().Be("John Doe");
    }

    [Test]
    public async Task SaveAsync_ShouldHandleNullAuthorInfo()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        CreateTopicNode(nodeId, "Test Title", "Test Prompt", "Test Response");
        
        var version = new TopicNodeVersion(
            Guid.NewGuid(),
            nodeId,
            "Test Title",
            "Test Prompt",
            "Test Response",
            0,
            DateTime.UtcNow,
            null,
            null);

        // Act
        await _repository!.SaveAsync(version);

        // Assert
        var retrieved = await _repository.GetByNodeIdAsync(nodeId);
        var savedVersion = retrieved.First(v => v.VersionId == version.VersionId);
        savedVersion.AuthorId.Should().BeNull();
        savedVersion.AuthorName.Should().BeNull();
    }

    [Test]
    public async Task GetByNodeIdAsync_ShouldFilterByNodeId()
    {
        // Arrange
        var nodeId1 = Guid.NewGuid();
        var nodeId2 = Guid.NewGuid();
        CreateTopicNode(nodeId1, "Title 1", "Prompt 1", "Response 1");
        CreateTopicNode(nodeId2, "Title 2", "Prompt 2", "Response 2");
        
        var version1 = new TopicNodeVersion(Guid.NewGuid(), nodeId1, "Title 1", "Prompt 1", "Response 1", 0, DateTime.UtcNow);
        var version2 = new TopicNodeVersion(Guid.NewGuid(), nodeId2, "Title 2", "Prompt 2", "Response 2", 0, DateTime.UtcNow);
        var version3 = new TopicNodeVersion(Guid.NewGuid(), nodeId1, "Title 3", "Prompt 3", "Response 3", 0, DateTime.UtcNow);

        await _repository!.SaveAsync(version1);
        await _repository.SaveAsync(version2);
        await _repository.SaveAsync(version3);

        // Act
        var versionsForNode1 = await _repository.GetByNodeIdAsync(nodeId1);
        var versionsForNode2 = await _repository.GetByNodeIdAsync(nodeId2);

        // Assert
        versionsForNode1.Should().HaveCount(2);
        versionsForNode1.Should().Contain(v => v.VersionId == version1.VersionId);
        versionsForNode1.Should().Contain(v => v.VersionId == version3.VersionId);
        versionsForNode1.Should().NotContain(v => v.VersionId == version2.VersionId);

        versionsForNode2.Should().HaveCount(1);
        versionsForNode2.Should().Contain(v => v.VersionId == version2.VersionId);
        versionsForNode2.Should().NotContain(v => v.VersionId == version1.VersionId);
        versionsForNode2.Should().NotContain(v => v.VersionId == version3.VersionId);
    }

    [Test]
    public async Task SaveAsync_ShouldPersistVersionWithNullTitle()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        CreateTopicNode(nodeId, null, "Test Prompt", "Test Response");
        
        var version = new TopicNodeVersion(
            Guid.NewGuid(),
            nodeId,
            null,
            "Test Prompt",
            "Test Response",
            0,
            DateTime.UtcNow);

        // Act
        await _repository!.SaveAsync(version);

        // Assert
        var retrieved = await _repository.GetByNodeIdAsync(nodeId);
        var savedVersion = retrieved.First(v => v.VersionId == version.VersionId);
        savedVersion.Title.Should().BeNull();
        savedVersion.Prompt.Should().Be("Test Prompt");
        savedVersion.Response.Should().Be("Test Response");
    }

    /// <summary>
    /// Helper method to create a topic node in the database for foreign key constraints.
    /// </summary>
    private void CreateTopicNode(Guid nodeId, string? title, string prompt, string response)
    {
        using var command = _db!.Connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO topic_nodes (NodeId, ParentId, Title, Prompt, Response, Ordinal, CreatedAt, UpdatedAt)
            VALUES (@NodeId, NULL, @Title, @Prompt, @Response, 0, @CreatedAt, @UpdatedAt)
        ";
        command.Parameters.AddWithValue("@NodeId", nodeId.ToString());
        command.Parameters.AddWithValue("@Title", (object?)title ?? DBNull.Value);
        command.Parameters.AddWithValue("@Prompt", prompt ?? string.Empty);
        command.Parameters.AddWithValue("@Response", response ?? string.Empty);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();
    }
}


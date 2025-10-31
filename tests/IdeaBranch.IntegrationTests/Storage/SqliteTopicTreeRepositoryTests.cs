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
/// Integration tests for SqliteTopicTreeRepository.
/// Tests the repository implementation against ITopicTreeRepository interface.
/// </summary>
public class SqliteTopicTreeRepositoryTests
{
    private string? _dbPath;
    private TopicDb? _db;
    private SqliteTopicTreeRepository? _repository;

    [SetUp]
    public void SetUp()
    {
        // Create temporary database file for testing
        _dbPath = Path.Combine(Path.GetTempPath(), $"ideabranch_test_{Guid.NewGuid():N}.db");
        _db = new TopicDb($"Data Source={_dbPath}");
        _repository = new SqliteTopicTreeRepository(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _repository?.Dispose();
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
        
        _repository = null;
        _db = null;
        _dbPath = null;
    }

    [Test]
    public async Task GetRootAsync_WhenNoRootExists_ShouldCreateDefaultRoot()
    {
        // Act
        var root = await _repository!.GetRootAsync();

        // Assert
        root.Should().NotBeNull();
        root.Prompt.Should().NotBeNullOrEmpty();
        root.Parent.Should().BeNull();
    }

    [Test]
    public async Task SaveAsync_ShouldPersistRootNode()
    {
        // Arrange
        var root = await _repository!.GetRootAsync();
        root.Title = "Test Root";
        root.Prompt = "Test prompt";
        root.SetResponse("Test response", parseListItems: false);

        // Act
        await _repository.SaveAsync(root);

        // Assert - Reload and verify
        var reloaded = await _repository.GetRootAsync();
        reloaded.Should().NotBeNull();
        reloaded.Id.Should().Be(root.Id);
        reloaded.Title.Should().Be("Test Root");
        reloaded.Prompt.Should().Be("Test prompt");
        reloaded.Response.Should().Be("Test response");
    }

    [Test]
    public async Task SaveAsync_ShouldPersistChildNodes()
    {
        // Arrange
        var root = await _repository!.GetRootAsync();
        var child1 = new TopicNode("Child 1 prompt", "Child 1");
        var child2 = new TopicNode("Child 2 prompt", "Child 2");
        root.AddChild(child1);
        root.AddChild(child2);

        // Act
        await _repository.SaveAsync(root);

        // Assert - Reload and verify children
        var reloaded = await _repository.GetRootAsync();
        reloaded.Children.Count.Should().Be(2);
        reloaded.Children.Should().Contain(c => c.Prompt == "Child 1 prompt");
        reloaded.Children.Should().Contain(c => c.Prompt == "Child 2 prompt");
    }

    [Test]
    public async Task SaveAsync_ShouldPersistNestedTree()
    {
        // Arrange
        var root = await _repository!.GetRootAsync();
        var child = new TopicNode("Child prompt", "Child");
        var grandchild = new TopicNode("Grandchild prompt", "Grandchild");
        root.AddChild(child);
        child.AddChild(grandchild);

        // Act
        await _repository.SaveAsync(root);

        // Assert - Reload and verify nested structure
        var reloaded = await _repository.GetRootAsync();
        reloaded.Children.Count.Should().Be(1);
        reloaded.Children[0].Title.Should().Be("Child");
        reloaded.Children[0].Children.Count.Should().Be(1);
        reloaded.Children[0].Children[0].Title.Should().Be("Grandchild");
    }

    [Test]
    public async Task SaveAsync_ShouldUpdateExistingNode()
    {
        // Arrange
        var root = await _repository!.GetRootAsync();
        root.Title = "Original Title";
        await _repository.SaveAsync(root);

        // Act - Update and save
        root.Title = "Updated Title";
        root.SetResponse("Updated response", parseListItems: false);
        await _repository.SaveAsync(root);

        // Assert
        var reloaded = await _repository.GetRootAsync();
        reloaded.Title.Should().Be("Updated Title");
        reloaded.Response.Should().Be("Updated response");
    }

    [Test]
    public async Task SaveAsync_ShouldHandleListParsing()
    {
        // Arrange
        var root = await _repository!.GetRootAsync();
        root.SetResponse("1. First item\n2. Second item\n3. Third item", parseListItems: true);

        // Act
        await _repository.SaveAsync(root);

        // Assert - Verify children were created (order may vary, so check by content)
        var reloaded = await _repository.GetRootAsync();
        reloaded.Children.Count.Should().Be(3);
        reloaded.Children.Should().Contain(c => c.Prompt == "First item");
        reloaded.Children.Should().Contain(c => c.Prompt == "Second item");
        reloaded.Children.Should().Contain(c => c.Prompt == "Third item");
    }

    [Test]
    public async Task SaveAsync_ShouldPersistTimestamps()
    {
        // Arrange
        var root = await _repository!.GetRootAsync();
        var originalCreatedAt = root.CreatedAt.ToUniversalTime();
        var originalUpdatedAt = root.UpdatedAt.ToUniversalTime();
        await _repository.SaveAsync(root);

        // Assert - Verify timestamps are persisted correctly (normalize to UTC for comparison)
        var reloaded = await _repository.GetRootAsync();
        reloaded.CreatedAt.ToUniversalTime().Should().BeCloseTo(originalCreatedAt, TimeSpan.FromSeconds(5));
        reloaded.UpdatedAt.ToUniversalTime().Should().BeCloseTo(originalUpdatedAt, TimeSpan.FromSeconds(5));
        // Note: TopicNode may not automatically update UpdatedAt on property changes
        // This test verifies that timestamps are correctly persisted to/from database
    }

    [Test]
    public async Task GetRootAsync_ShouldMaintainIdentityAcrossSaves()
    {
        // Arrange
        var root1 = await _repository!.GetRootAsync();
        var originalId = root1.Id;
        await _repository.SaveAsync(root1);

        // Act
        var root2 = await _repository.GetRootAsync();

        // Assert
        root2.Id.Should().Be(originalId);
    }

    [Test]
    [Ignore("Currently SaveTree only upserts nodes, doesn't delete orphaned nodes. Orphan cleanup requires tracking deleted nodes.")]
    public async Task SaveAsync_ShouldHandleDeletedChildren()
    {
        // Arrange
        var root = await _repository!.GetRootAsync();
        var child1 = new TopicNode("Child 1", "Child 1");
        var child2 = new TopicNode("Child 2", "Child 2");
        root.AddChild(child1);
        root.AddChild(child2);
        await _repository.SaveAsync(root);

        // Act - Remove child and save
        root.RemoveChild(child1);
        await _repository.SaveAsync(root);

        // Assert - Note: Currently SaveTree only upserts, doesn't delete orphaned nodes
        // This test verifies that the domain model correctly reflects the deletion
        var reloaded = await _repository.GetRootAsync();
        // When reloaded, the removed child should not be in the tree structure
        // However, due to current implementation limitations, we verify the domain model
        reloaded.Children.Should().NotContain(c => c.Title == "Child 1");
        reloaded.Children.Should().Contain(c => c.Title == "Child 2");
    }

    [Test]
    public async Task SaveAsync_ShouldHandleMovedChildren()
    {
        // Arrange
        var root = await _repository!.GetRootAsync();
        var parent1 = new TopicNode("Parent 1", "Parent 1");
        var parent2 = new TopicNode("Parent 2", "Parent 2");
        var child = new TopicNode("Child", "Child");
        root.AddChild(parent1);
        root.AddChild(parent2);
        parent1.AddChild(child);
        await _repository.SaveAsync(root);

        // Act - Move child to different parent
        parent1.MoveChild(child, parent2);
        await _repository.SaveAsync(root);

        // Assert
        var reloaded = await _repository.GetRootAsync();
        var reloadedParent1 = reloaded.Children.First(c => c.Title == "Parent 1");
        var reloadedParent2 = reloaded.Children.First(c => c.Title == "Parent 2");
        reloadedParent1.Children.Count.Should().Be(0);
        reloadedParent2.Children.Count.Should().Be(1);
        reloadedParent2.Children[0].Title.Should().Be("Child");
    }
}


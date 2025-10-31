using System;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Storage;
using IdeaBranch.IntegrationTests.Storage;
using NUnit.Framework;

namespace IdeaBranch.IntegrationTests.Storage;

[TestFixture]
public class PromptTemplateRepositoryTests
{
    private SqliteTestDatabase _testDb = null!;
    private SqlitePromptTemplateRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _testDb = new SqliteTestDatabase();
        // Use TopicDb to apply all migrations
        using var topicDb = new TopicDb($"Data Source={_testDb.DbPath}");
        _repository = new SqlitePromptTemplateRepository(topicDb.Connection);
        
        // Reopen connection after TopicDb is disposed
        _testDb.ReopenConnection();
        _repository = new SqlitePromptTemplateRepository(_testDb.Connection);
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
        Assert.That(root.IsCategory, Is.True);
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
    public async Task SaveAsync_ShouldPersistCategory()
    {
        // Arrange
        var category = new PromptTemplate("Test Category", null);

        // Act
        await _repository.SaveAsync(category);

        // Assert
        var retrieved = await _repository.GetByIdAsync(category.Id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Name, Is.EqualTo("Test Category"));
        Assert.That(retrieved.IsCategory, Is.True);
        Assert.That(retrieved.Body, Is.Null);
    }

    [Test]
    public async Task SaveAsync_ShouldPersistTemplateWithBody()
    {
        // Arrange
        var template = new PromptTemplate("Test Template", "Template body text with {placeholder}");

        // Act
        await _repository.SaveAsync(template);

        // Assert
        var retrieved = await _repository.GetByIdAsync(template.Id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Name, Is.EqualTo("Test Template"));
        Assert.That(retrieved.IsCategory, Is.False);
        Assert.That(retrieved.Body, Is.EqualTo("Template body text with {placeholder}"));
    }

    [Test]
    public async Task SaveAsync_ShouldSaveHierarchicalStructure()
    {
        // Arrange
        var root = new PromptTemplate("Root", null);
        var category = new PromptTemplate("Category", root.Id);
        var template = new PromptTemplate("Template", "Template body", category.Id);
        
        root.AddChild(category);
        category.AddChild(template);

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
        Assert.That(retrievedCategory.IsCategory, Is.True);

        var retrievedTemplate = await _repository.GetByIdAsync(template.Id);
        Assert.That(retrievedTemplate, Is.Not.Null);
        Assert.That(retrievedTemplate!.Name, Is.EqualTo("Template"));
        Assert.That(retrievedTemplate.ParentId, Is.EqualTo(category.Id));
        Assert.That(retrievedTemplate.IsCategory, Is.False);
        Assert.That(retrievedTemplate.Body, Is.EqualTo("Template body"));
    }

    [Test]
    public async Task GetChildrenAsync_ShouldReturnChildrenOrderedByOrder()
    {
        // Arrange
        var root = await _repository.GetRootAsync();
        var child1 = new PromptTemplate("Child 1", root.Id) { Order = 2 };
        var child2 = new PromptTemplate("Child 2", root.Id) { Order = 1 };
        root.AddChild(child1);
        root.AddChild(child2);
        await _repository.SaveAsync(root);

        // Act
        var children = await _repository.GetChildrenAsync(root.Id);

        // Assert
        Assert.That(children.Count, Is.GreaterThanOrEqualTo(2));
        var testChildren = children.Where(c => c.Name == "Child 1" || c.Name == "Child 2").OrderBy(c => c.Order).ToList();
        Assert.That(testChildren[0].Name, Is.EqualTo("Child 2")); // Order 1 comes first
        Assert.That(testChildren[1].Name, Is.EqualTo("Child 1")); // Order 2 comes second
    }

    [Test]
    public async Task GetChildrenAsync_WithRootId_ShouldReturnRootChildren()
    {
        // Arrange
        var root = await _repository.GetRootAsync();
        var child = new PromptTemplate("Child Category", root.Id);
        root.AddChild(child);
        await _repository.SaveAsync(root);

        // Act
        var children = await _repository.GetChildrenAsync(root.Id);

        // Assert
        var testChild = children.FirstOrDefault(c => c.Name == "Child Category");
        Assert.That(testChild, Is.Not.Null);
        Assert.That(testChild!.ParentId, Is.EqualTo(root.Id));
    }

    [Test]
    public async Task GetByPathAsync_ShouldReturnTemplateByPath()
    {
        // Arrange - Create root-level category (first path part must have ParentId IS NULL)
        var category = new PromptTemplate("Information Retrieval", null);
        var template = new PromptTemplate("Definitions", "Template body", category.Id);
        category.AddChild(template);
        await _repository.SaveAsync(category);

        // Act
        var retrieved = await _repository.GetByPathAsync("Information Retrieval/Definitions");

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Name, Is.EqualTo("Definitions"));
        Assert.That(retrieved.Body, Is.EqualTo("Template body"));
    }

    [Test]
    public async Task GetByPathAsync_ShouldReturnNullForNonExistentPath()
    {
        // Act
        var result = await _repository.GetByPathAsync("NonExistent/Category");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByPathAsync_ShouldReturnNullForEmptyPath()
    {
        // Act
        var result = await _repository.GetByPathAsync("");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetSubtreeAsync_ShouldReturnOnlyTemplatesNotCategories()
    {
        // Arrange
        var root = await _repository.GetRootAsync();
        var category = new PromptTemplate("Category", root.Id);
        var template1 = new PromptTemplate("Template 1", "Body 1", category.Id);
        var template2 = new PromptTemplate("Template 2", "Body 2", category.Id);
        var subCategory = new PromptTemplate("Sub Category", category.Id);
        var template3 = new PromptTemplate("Template 3", "Body 3", subCategory.Id);
        
        root.AddChild(category);
        category.AddChild(template1);
        category.AddChild(template2);
        category.AddChild(subCategory);
        subCategory.AddChild(template3);
        await _repository.SaveAsync(root);

        // Act
        var templates = await _repository.GetSubtreeAsync(category.Id);

        // Assert - Should return all templates (1, 2, 3) but not categories
        Assert.That(templates.Count, Is.EqualTo(3));
        var templateNames = templates.Select(t => t.Name).ToHashSet();
        Assert.That(templateNames, Contains.Item("Template 1"));
        Assert.That(templateNames, Contains.Item("Template 2"));
        Assert.That(templateNames, Contains.Item("Template 3"));
        
        // Verify none are categories
        Assert.That(templates.All(t => !t.IsCategory), Is.True);
    }

    [Test]
    public async Task GetSubtreeAsync_WithNullParentId_ShouldReturnAllTemplates()
    {
        // Arrange
        var root = await _repository.GetRootAsync();
        var category = new PromptTemplate("Category", root.Id);
        var template = new PromptTemplate("Template", "Body", category.Id);
        root.AddChild(category);
        category.AddChild(template);
        await _repository.SaveAsync(root);

        // Act
        var templates = await _repository.GetSubtreeAsync(null);

        // Assert
        var testTemplate = templates.FirstOrDefault(t => t.Name == "Template");
        Assert.That(testTemplate, Is.Not.Null);
        Assert.That(testTemplate!.Body, Is.EqualTo("Body"));
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveTemplate()
    {
        // Arrange
        var template = new PromptTemplate("Test Template", "Template body", null);
        await _repository.SaveAsync(template);

        // Act
        var deleted = await _repository.DeleteAsync(template.Id);

        // Assert
        Assert.That(deleted, Is.True);
        var retrieved = await _repository.GetByIdAsync(template.Id);
        Assert.That(retrieved, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldCascadeDeleteChildren()
    {
        // Arrange
        var root = new PromptTemplate("Root", null);
        var child1 = new PromptTemplate("Child 1", root.Id);
        var child2 = new PromptTemplate("Child 2", root.Id);
        root.AddChild(child1);
        root.AddChild(child2);
        await _repository.SaveAsync(root);

        // Verify children exist
        var childrenBefore = await _repository.GetChildrenAsync(root.Id);
        Assert.That(childrenBefore.Count, Is.GreaterThanOrEqualTo(2));

        // Act - Delete root
        var deleted = await _repository.DeleteAsync(root.Id);

        // Assert - Root deleted
        Assert.That(deleted, Is.True);
        var retrievedRoot = await _repository.GetByIdAsync(root.Id);
        Assert.That(retrievedRoot, Is.Null);

        // Assert - Children should be deleted by cascade (foreign key constraint)
        var childrenAfter = await _repository.GetChildrenAsync(root.Id);
        var testChildren = childrenAfter.Where(c => c.Name == "Child 1" || c.Name == "Child 2").ToList();
        Assert.That(testChildren.Count, Is.EqualTo(0));
        var retrievedChild1 = await _repository.GetByIdAsync(child1.Id);
        var retrievedChild2 = await _repository.GetByIdAsync(child2.Id);
        Assert.That(retrievedChild1, Is.Null);
        Assert.That(retrievedChild2, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNullForNonExistentTemplate()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }
}


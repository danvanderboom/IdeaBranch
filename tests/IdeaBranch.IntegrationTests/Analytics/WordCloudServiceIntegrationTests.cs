using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Analytics;
using IdeaBranch.Infrastructure.Export;
using IdeaBranch.Infrastructure.Storage;
using IdeaBranch.IntegrationTests.Storage;
using NUnit.Framework;

namespace IdeaBranch.IntegrationTests.Analytics;

/// <summary>
/// Integration tests for WordCloudService with real database repositories.
/// </summary>
[TestFixture]
public class WordCloudServiceIntegrationTests
{
    private SqliteTestDatabase _testDb = null!;
    private WordCloudService _service = null!;
    private IConversationsRepository _conversationsRepository = null!;
    private IAnnotationsRepository _annotationsRepository = null!;
    private ITopicTreeRepository _topicTreeRepository = null!;
    private ITagTaxonomyRepository _tagTaxonomyRepository = null!;
    private TopicDb _topicDb = null!;

    [SetUp]
    public void SetUp()
    {
        _testDb = new SqliteTestDatabase();
        _topicDb = new TopicDb($"Data Source={_testDb.DbPath}");
        
        _annotationsRepository = new SqliteAnnotationsRepository(_topicDb.Connection);
        _tagTaxonomyRepository = new SqliteTagTaxonomyRepository(_topicDb.Connection);
        _topicTreeRepository = new SqliteTopicTreeRepository(_topicDb, null);
        
        _conversationsRepository = new SqliteConversationsRepository(
            _topicDb.Connection,
            _annotationsRepository,
            _tagTaxonomyRepository);
        
        _service = new WordCloudService(
            _conversationsRepository,
            _annotationsRepository,
            _topicTreeRepository,
            _tagTaxonomyRepository);
    }

    [TearDown]
    public void TearDown()
    {
        (_topicTreeRepository as IDisposable)?.Dispose();
        _topicDb?.Dispose();
        _testDb?.Dispose();
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithPrompts_ReturnsWordFrequencies()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        root.SetResponse("The quick brown fox jumps over the lazy dog.");
        await _topicTreeRepository.SaveAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Prompts }
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.WordFrequencies.Should().NotBeEmpty();
        result.Metadata.TotalWordCount.Should().BeGreaterThan(0);
        result.Metadata.UniqueWordCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithResponses_ReturnsWordFrequencies()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        root.SetResponse("This is a test response about important concepts and meaningful topics.");
        await _topicTreeRepository.SaveAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses }
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.WordFrequencies.Should().NotBeEmpty();
        result.WordFrequencies.Should().Contain(w => w.Word.Equals("important", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithAnnotations_ReturnsWordFrequencies()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        var nodeId = root.Id;

        var annotation1 = new Annotation(nodeId, 0, 10, "This is a comment about analytics and data visualization.");
        var annotation2 = new Annotation(nodeId, 20, 30, "Another comment discussing important topics and concepts.");
        await _annotationsRepository.SaveAsync(annotation1);
        await _annotationsRepository.SaveAsync(annotation2);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Annotations }
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.WordFrequencies.Should().NotBeEmpty();
        result.WordFrequencies.Should().Contain(w => w.Word.Equals("comment", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithMultipleSources_AggregatesWords()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        root.Prompt = "What is artificial intelligence?";
        root.SetResponse("Artificial intelligence is a branch of computer science.");
        await _topicTreeRepository.SaveAsync(root);

        var annotation = new Annotation(root.Id, 0, 10, "AI comment");
        await _annotationsRepository.SaveAsync(annotation);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> 
            { 
                TextSourceType.Prompts, 
                TextSourceType.Responses, 
                TextSourceType.Annotations 
            }
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.WordFrequencies.Should().NotBeEmpty();
        result.WordFrequencies.Should().Contain(w => w.Word.Equals("artificial", StringComparison.OrdinalIgnoreCase));
        result.Metadata.TotalWordCount.Should().BeGreaterThan(5);
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithMinFrequency_FiltersLowFrequencyWords()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        root.SetResponse("test test test unique");
        await _topicTreeRepository.SaveAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses },
            MinFrequency = 2
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.WordFrequencies.Should().OnlyContain(w => w.Frequency >= 2);
        result.WordFrequencies.Should().Contain(w => w.Word.Equals("test", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithMaxWords_LimitsWordCount()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        root.SetResponse("one two three four five six seven eight nine ten eleven twelve");
        await _topicTreeRepository.SaveAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses },
            MaxWords = 5
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.WordFrequencies.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Test]
    public async Task GenerateWordCloudAsync_CalculatesWeightsCorrectly()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        root.SetResponse("common common common rare");
        await _topicTreeRepository.SaveAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses }
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.WordFrequencies.Should().NotBeEmpty();
        
        var commonWord = result.WordFrequencies.FirstOrDefault(w => w.Word.Equals("common", StringComparison.OrdinalIgnoreCase));
        var rareWord = result.WordFrequencies.FirstOrDefault(w => w.Word.Equals("rare", StringComparison.OrdinalIgnoreCase));
        
        commonWord.Should().NotBeNull();
        rareWord.Should().NotBeNull();
        
        commonWord!.Weight.Should().BeGreaterThan(rareWord!.Weight);
        commonWord.Weight.Should().BeInRange(0.0, 1.0);
        rareWord.Weight.Should().BeInRange(0.0, 1.0);
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithDateFilter_FiltersByDate()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        root.SetResponse("Old content");
        await _topicTreeRepository.SaveAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses },
            StartDate = DateTime.UtcNow.AddDays(1), // Future date
            EndDate = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        // Since all data is older than the filter, should have no results
        result.WordFrequencies.Should().BeEmpty();
    }
}


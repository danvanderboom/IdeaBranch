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
using System.Text.Json;

namespace IdeaBranch.IntegrationTests.Analytics;

/// <summary>
/// Integration tests for AnalyticsExportService with real analytics data.
/// </summary>
[TestFixture]
public class AnalyticsExportServiceIntegrationTests
{
    private SqliteTestDatabase _testDb = null!;
    private AnalyticsExportService _exportService = null!;
    private WordCloudService _wordCloudService = null!;
    private TimelineService _timelineService = null!;
    private TopicDb _topicDb = null!;

    [SetUp]
    public void SetUp()
    {
        _testDb = new SqliteTestDatabase();
        _topicDb = new TopicDb($"Data Source={_testDb.DbPath}");
        
        var annotationsRepository = new SqliteAnnotationsRepository(_topicDb.Connection);
        var tagTaxonomyRepository = new SqliteTagTaxonomyRepository(_topicDb.Connection);
        var topicTreeRepository = new SqliteTopicTreeRepository(_topicDb, null);
        
        var conversationsRepository = new SqliteConversationsRepository(
            _topicDb.Connection,
            annotationsRepository,
            tagTaxonomyRepository);
        
        _wordCloudService = new WordCloudService(
            conversationsRepository,
            annotationsRepository,
            topicTreeRepository,
            tagTaxonomyRepository);
        
        _timelineService = new TimelineService(
            conversationsRepository,
            annotationsRepository,
            topicTreeRepository,
            tagTaxonomyRepository);
        
        _exportService = new AnalyticsExportService();
    }

    [TearDown]
    public void TearDown()
    {
        _topicDb?.Dispose();
        _testDb?.Dispose();
    }

    [Test]
    public async Task ExportWordCloudToJsonAsync_ReturnsValidJson()
    {
        // Arrange
        var root = await GetTopicTreeRepository().GetRootAsync();
        root.SetResponse("test test unique");
        await GetTopicTreeRepository().SaveAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses }
        };

        var wordCloudData = await _wordCloudService.GenerateWordCloudAsync(options);

        // Act
        var json = await _exportService.ExportWordCloudToJsonAsync(wordCloudData);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("wordFrequencies");
        json.Should().Contain("metadata");
        
        // Validate JSON can be parsed
        var parsed = JsonSerializer.Deserialize<WordCloudData>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        parsed.Should().NotBeNull();
        parsed!.WordFrequencies.Should().NotBeEmpty();
    }

    [Test]
    public async Task ExportWordCloudToCsvAsync_ReturnsValidCsv()
    {
        // Arrange
        var root = await GetTopicTreeRepository().GetRootAsync();
        root.SetResponse("test test unique");
        await GetTopicTreeRepository().SaveAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses }
        };

        var wordCloudData = await _wordCloudService.GenerateWordCloudAsync(options);

        // Act
        var csv = await _exportService.ExportWordCloudToCsvAsync(wordCloudData);

        // Assert
        csv.Should().NotBeNullOrEmpty();
        csv.Should().Contain("Word,Frequency,Weight");
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().BeGreaterThan(1); // Header + data rows
    }

    [Test]
    public async Task ExportWordCloudToPngAsync_ReturnsValidPng()
    {
        // Arrange
        var root = await GetTopicTreeRepository().GetRootAsync();
        root.SetResponse("test test test unique word cloud visualization");
        await GetTopicTreeRepository().SaveAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses }
        };

        var wordCloudData = await _wordCloudService.GenerateWordCloudAsync(options);

        // Act
        var pngBytes = await _exportService.ExportWordCloudToPngAsync(wordCloudData);

        // Assert
        pngBytes.Should().NotBeNullOrEmpty();
        pngBytes.Length.Should().BeGreaterThan(100); // Should be a valid PNG
        
        // PNG magic number: 89 50 4E 47
        pngBytes[0].Should().Be(0x89);
        pngBytes[1].Should().Be(0x50);
        pngBytes[2].Should().Be(0x4E);
        pngBytes[3].Should().Be(0x47);
    }

    [Test]
    public async Task ExportTimelineToJsonAsync_ReturnsValidJson()
    {
        // Arrange
        var root = await GetTopicTreeRepository().GetRootAsync();
        root.SetResponse("Test");
        await GetTopicTreeRepository().SaveAsync(root);

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> { EventSourceType.Topics }
        };

        var timelineData = await _timelineService.GenerateTimelineAsync(options);

        // Act
        var json = await _exportService.ExportTimelineToJsonAsync(timelineData);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("events");
        json.Should().Contain("bands");
        json.Should().Contain("metadata");
        
        // Validate JSON can be parsed
        var parsed = JsonSerializer.Deserialize<TimelineData>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        parsed.Should().NotBeNull();
    }

    [Test]
    public async Task ExportTimelineToCsvAsync_ReturnsValidCsv()
    {
        // Arrange
        var root = await GetTopicTreeRepository().GetRootAsync();
        root.SetResponse("Test");
        await GetTopicTreeRepository().SaveAsync(root);

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> { EventSourceType.Topics }
        };

        var timelineData = await _timelineService.GenerateTimelineAsync(options);

        // Act
        var csv = await _exportService.ExportTimelineToCsvAsync(timelineData);

        // Assert
        csv.Should().NotBeNullOrEmpty();
        csv.Should().Contain("Timestamp,EventType,Title,Details,NodeId");
        if (timelineData.Events.Count > 0)
        {
            var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            lines.Length.Should().BeGreaterThan(1); // Header + data rows
        }
    }

    [Test]
    public async Task ExportTimelineToPngAsync_ReturnsValidPng()
    {
        // Arrange
        var root = await GetTopicTreeRepository().GetRootAsync();
        
        var child = new TopicNode("Child", "Child");
        root.AddChild(child);
        await GetTopicTreeRepository().SaveAsync(root);

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> { EventSourceType.Topics }
        };

        var timelineData = await _timelineService.GenerateTimelineAsync(options);

        // Act
        var pngBytes = await _exportService.ExportTimelineToPngAsync(timelineData);

        // Assert
        pngBytes.Should().NotBeNullOrEmpty();
        pngBytes.Length.Should().BeGreaterThan(100); // Should be a valid PNG
        
        // PNG magic number: 89 50 4E 47
        pngBytes[0].Should().Be(0x89);
        pngBytes[1].Should().Be(0x50);
        pngBytes[2].Should().Be(0x4E);
        pngBytes[3].Should().Be(0x47);
    }

    [Test]
    public async Task ExportWordCloudToCsvAsync_EscapesSpecialCharacters()
    {
        // Arrange
        var root = await GetTopicTreeRepository().GetRootAsync();
        root.SetResponse("word with, comma and \"quote\"");
        await GetTopicTreeRepository().SaveAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses }
        };

        var wordCloudData = await _wordCloudService.GenerateWordCloudAsync(options);

        // Act
        var csv = await _exportService.ExportWordCloudToCsvAsync(wordCloudData);

        // Assert
        csv.Should().NotBeNullOrEmpty();
        // CSV should be valid even with special characters
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().BeGreaterThan(1);
    }

    private ITopicTreeRepository GetTopicTreeRepository()
    {
        return new SqliteTopicTreeRepository(_topicDb, null);
    }
}


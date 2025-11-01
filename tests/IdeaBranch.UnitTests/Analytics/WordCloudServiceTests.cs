using FluentAssertions;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Analytics;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.UnitTests.Analytics;

/// <summary>
/// Tests for WordCloudService.
/// </summary>
public class WordCloudServiceTests
{
    private Mock<IConversationsRepository> _conversationsRepositoryMock = null!;
    private Mock<IAnnotationsRepository> _annotationsRepositoryMock = null!;
    private Mock<ITopicTreeRepository> _topicTreeRepositoryMock = null!;
    private Mock<ITagTaxonomyRepository> _tagTaxonomyRepositoryMock = null!;
    private WordCloudService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _conversationsRepositoryMock = new Mock<IConversationsRepository>();
        _annotationsRepositoryMock = new Mock<IAnnotationsRepository>();
        _topicTreeRepositoryMock = new Mock<ITopicTreeRepository>();
        _tagTaxonomyRepositoryMock = new Mock<ITagTaxonomyRepository>();

        _service = new WordCloudService(
            _conversationsRepositoryMock.Object,
            _annotationsRepositoryMock.Object,
            _topicTreeRepositoryMock.Object,
            _tagTaxonomyRepositoryMock.Object);
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithPrompts_ReturnsWordFrequencies()
    {
        // Arrange
        var root = new TopicNode("What is the meaning of life?", "Root");
        root.SetResponse("The meaning of life is to find purpose and happiness.");
        var child = new TopicNode("How to find purpose?", "Child");
        child.SetResponse("Purpose comes from meaningful work and relationships.");
        root.AddChild(child);

        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Prompts }
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.WordFrequencies.Should().NotBeEmpty();
        result.WordFrequencies.Should().Contain(w => w.Word.Equals("purpose", StringComparison.OrdinalIgnoreCase));
        result.Metadata.TotalWordCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithResponses_ReturnsWordFrequencies()
    {
        // Arrange
        var root = new TopicNode("Question", "Root");
        root.SetResponse("This is a response about happiness and purpose.");
        var child = new TopicNode("Another question", "Child");
        child.SetResponse("Another response about life and meaning.");
        root.AddChild(child);

        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses }
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.WordFrequencies.Should().NotBeEmpty();
        result.WordFrequencies.Should().Contain(w => w.Word.Equals("response", StringComparison.OrdinalIgnoreCase));
        result.Metadata.TotalWordCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithMinFrequency_FiltersLowFrequencyWords()
    {
        // Arrange
        var root = new TopicNode("Test question", "Root");
        root.SetResponse("This is a test. Test test test. This is also a test.");
        
        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Responses },
            MinFrequency = 3
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.WordFrequencies.Should().OnlyContain(w => w.Frequency >= 3);
        result.WordFrequencies.Should().Contain(w => w.Word.Equals("test", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public async Task GenerateWordCloudAsync_WithMaxWords_LimitsWordCount()
    {
        // Arrange
        var root = new TopicNode("Question", "Root");
        root.SetResponse("This is a test with many different words to test the max words limit functionality.");
        
        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

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
    public async Task GenerateWordCloudAsync_WithAnnotations_IncludesAnnotationText()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        var annotation = new Annotation(nodeId, 0, 10, "This is an annotation comment about important topics.");
        
        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TopicNode("Test", "Root"));

        _annotationsRepositoryMock
            .Setup(r => r.GetByNodeIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Annotation> { annotation }.AsReadOnly());

        var options = new WordCloudOptions
        {
            SourceTypes = new HashSet<TextSourceType> { TextSourceType.Annotations }
        };

        // Act
        var result = await _service.GenerateWordCloudAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.TotalWordCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GenerateWordCloudAsync_CalculatesWeights()
    {
        // Arrange
        var root = new TopicNode("Test", "Root");
        root.SetResponse("common common common rare");
        
        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

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
}


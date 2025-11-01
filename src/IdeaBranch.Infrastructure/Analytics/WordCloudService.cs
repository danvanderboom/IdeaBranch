using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;

namespace IdeaBranch.Infrastructure.Analytics;

/// <summary>
/// Service for generating word cloud data from text sources.
/// </summary>
public class WordCloudService : IAnalyticsService
{
    private readonly IConversationsRepository _conversationsRepository;
    private readonly IAnnotationsRepository _annotationsRepository;
    private readonly ITopicTreeRepository _topicTreeRepository;
    private readonly ITagTaxonomyRepository _tagTaxonomyRepository;
    private readonly HashSet<string> _stopWords;

    /// <summary>
    /// Initializes a new instance with required repositories.
    /// </summary>
    public WordCloudService(
        IConversationsRepository conversationsRepository,
        IAnnotationsRepository annotationsRepository,
        ITopicTreeRepository topicTreeRepository,
        ITagTaxonomyRepository tagTaxonomyRepository)
    {
        _conversationsRepository = conversationsRepository ?? throw new ArgumentNullException(nameof(conversationsRepository));
        _annotationsRepository = annotationsRepository ?? throw new ArgumentNullException(nameof(annotationsRepository));
        _topicTreeRepository = topicTreeRepository ?? throw new ArgumentNullException(nameof(topicTreeRepository));
        _tagTaxonomyRepository = tagTaxonomyRepository ?? throw new ArgumentNullException(nameof(tagTaxonomyRepository));
        _stopWords = LoadStopWords();
    }

    /// <inheritdoc/>
    public async Task<WordCloudData> GenerateWordCloudAsync(
        WordCloudOptions options,
        CancellationToken cancellationToken = default)
    {
        var textSources = new List<string>();

        // Collect text from different sources
        if (options.SourceTypes.Contains(TextSourceType.Prompts))
        {
            var promptTexts = await CollectPromptTextsAsync(options, cancellationToken);
            textSources.AddRange(promptTexts);
        }

        if (options.SourceTypes.Contains(TextSourceType.Responses))
        {
            var responseTexts = await CollectResponseTextsAsync(options, cancellationToken);
            textSources.AddRange(responseTexts);
        }

        if (options.SourceTypes.Contains(TextSourceType.Annotations))
        {
            var annotationTexts = await CollectAnnotationTextsAsync(options, cancellationToken);
            textSources.AddRange(annotationTexts);
        }

        if (options.SourceTypes.Contains(TextSourceType.Topics))
        {
            var topicTexts = await CollectTopicTextsAsync(options, cancellationToken);
            textSources.AddRange(topicTexts);
        }

        // Tokenize and count words
        var wordFrequencies = TokenizeAndCount(textSources, options);

        // Calculate weights (normalize to 0.0-1.0)
        var maxFrequency = wordFrequencies.Count > 0 ? wordFrequencies[0].Frequency : 1;
        foreach (var wordFreq in wordFrequencies)
        {
            wordFreq.Weight = maxFrequency > 0 ? (double)wordFreq.Frequency / maxFrequency : 0.0;
        }

        return new WordCloudData
        {
            WordFrequencies = wordFrequencies.AsReadOnly(),
            Metadata = new WordCloudMetadata
            {
                AppliedFilters = options,
                TotalWordCount = textSources.Sum(t => CountWords(t)),
                UniqueWordCount = wordFrequencies.Count,
                GeneratedAt = DateTime.UtcNow
            }
        };
    }

    /// <inheritdoc/>
    public Task<TimelineData> GenerateTimelineAsync(
        TimelineOptions options,
        CancellationToken cancellationToken = default)
    {
        // This is implemented by TimelineService
        throw new NotImplementedException("Timeline generation is handled by TimelineService.");
    }

    /// <summary>
    /// Collects prompt texts based on options.
    /// </summary>
    private async Task<List<string>> CollectPromptTextsAsync(
        WordCloudOptions options,
        CancellationToken cancellationToken)
    {
        var texts = new List<string>();

        if (options.TagIds != null && options.TagIds.Count > 0)
        {
            var messages = await _conversationsRepository.GetMessagesByTagsAsync(
                options.TagIds,
                options.IncludeTagDescendants,
                options.StartDate,
                options.EndDate,
                cancellationToken);

            texts.AddRange(messages
                .Where(m => m.MessageType == ConversationMessageType.Prompt)
                .Select(m => m.Text));
        }
        else if (options.NodeIds != null && options.NodeIds.Count > 0)
        {
            var messages = await _conversationsRepository.GetMessagesByNodeIdsAsync(
                options.NodeIds,
                cancellationToken);

            texts.AddRange(messages
                .Where(m => m.MessageType == ConversationMessageType.Prompt)
                .Select(m => m.Text));
        }
        else
        {
            // Get all prompts - iterate through topic tree
            var root = await _topicTreeRepository.GetRootAsync(cancellationToken);
            texts.AddRange(CollectPromptTextsFromNode(root, options));
        }

        return texts;
    }

    /// <summary>
    /// Collects response texts based on options.
    /// </summary>
    private async Task<List<string>> CollectResponseTextsAsync(
        WordCloudOptions options,
        CancellationToken cancellationToken)
    {
        var texts = new List<string>();

        if (options.TagIds != null && options.TagIds.Count > 0)
        {
            var messages = await _conversationsRepository.GetMessagesByTagsAsync(
                options.TagIds,
                options.IncludeTagDescendants,
                options.StartDate,
                options.EndDate,
                cancellationToken);

            texts.AddRange(messages
                .Where(m => m.MessageType == ConversationMessageType.Response)
                .Select(m => m.Text));
        }
        else if (options.NodeIds != null && options.NodeIds.Count > 0)
        {
            var messages = await _conversationsRepository.GetMessagesByNodeIdsAsync(
                options.NodeIds,
                cancellationToken);

            texts.AddRange(messages
                .Where(m => m.MessageType == ConversationMessageType.Response)
                .Select(m => m.Text));
        }
        else
        {
            // Get all responses - iterate through topic tree
            var root = await _topicTreeRepository.GetRootAsync(cancellationToken);
            texts.AddRange(CollectResponseTextsFromNode(root, options));
        }

        return texts;
    }

    /// <summary>
    /// Collects annotation texts based on options.
    /// </summary>
    private async Task<List<string>> CollectAnnotationTextsAsync(
        WordCloudOptions options,
        CancellationToken cancellationToken)
    {
        var texts = new List<string>();

        // Get annotations - we need to iterate nodes or use tag filters
        var nodeIds = new List<Guid>();

        if (options.NodeIds != null && options.NodeIds.Count > 0)
        {
            nodeIds.AddRange(options.NodeIds);
        }
        else
        {
            // Get all node IDs from topic tree
            var root = await _topicTreeRepository.GetRootAsync(cancellationToken);
            nodeIds.AddRange(CollectNodeIds(root));
        }

        foreach (var nodeId in nodeIds)
        {
            IReadOnlyList<Annotation> annotations;
            
            if (options.TagIds != null && options.TagIds.Count > 0)
            {
                annotations = await _annotationsRepository.GetByNodeIdAndTagsAsync(
                    nodeId,
                    options.TagIds,
                    cancellationToken);
            }
            else
            {
                annotations = await _annotationsRepository.GetByNodeIdAsync(
                    nodeId,
                    cancellationToken);
            }

            texts.AddRange(annotations
                .Where(a => options.StartDate == null || a.CreatedAt >= options.StartDate)
                .Where(a => options.EndDate == null || a.CreatedAt <= options.EndDate)
                .Where(a => !string.IsNullOrWhiteSpace(a.Comment))
                .Select(a => a.Comment!));
        }

        return texts;
    }

    /// <summary>
    /// Collects topic texts (prompts + responses) based on options.
    /// </summary>
    private async Task<List<string>> CollectTopicTextsAsync(
        WordCloudOptions options,
        CancellationToken cancellationToken)
    {
        var texts = new List<string>();

        var root = await _topicTreeRepository.GetRootAsync(cancellationToken);
        texts.AddRange(CollectPromptTextsFromNode(root, options));
        texts.AddRange(CollectResponseTextsFromNode(root, options));

        return texts;
    }

    /// <summary>
    /// Recursively collects prompt texts from a topic node tree.
    /// </summary>
    private List<string> CollectPromptTextsFromNode(TopicNode node, WordCloudOptions options)
    {
        var texts = new List<string>();

        // Check date filter
        bool includeNode = true;
        if (options.StartDate.HasValue && node.CreatedAt < options.StartDate.Value)
            includeNode = false;
        if (options.EndDate.HasValue && node.CreatedAt > options.EndDate.Value)
            includeNode = false;

        if (includeNode && !string.IsNullOrWhiteSpace(node.Prompt))
        {
            texts.Add(node.Prompt);
        }

        foreach (var child in node.Children)
        {
            texts.AddRange(CollectPromptTextsFromNode(child, options));
        }

        return texts;
    }

    /// <summary>
    /// Recursively collects response texts from a topic node tree.
    /// </summary>
    private List<string> CollectResponseTextsFromNode(TopicNode node, WordCloudOptions options)
    {
        var texts = new List<string>();

        // Check date filter
        bool includeNode = true;
        if (options.StartDate.HasValue && node.UpdatedAt < options.StartDate.Value)
            includeNode = false;
        if (options.EndDate.HasValue && node.UpdatedAt > options.EndDate.Value)
            includeNode = false;

        if (includeNode && !string.IsNullOrWhiteSpace(node.Response))
        {
            texts.Add(node.Response);
        }

        foreach (var child in node.Children)
        {
            texts.AddRange(CollectResponseTextsFromNode(child, options));
        }

        return texts;
    }

    /// <summary>
    /// Recursively collects all node IDs from a topic node tree.
    /// </summary>
    private List<Guid> CollectNodeIds(TopicNode node)
    {
        var nodeIds = new List<Guid> { node.Id };
        
        foreach (var child in node.Children)
        {
            nodeIds.AddRange(CollectNodeIds(child));
        }

        return nodeIds;
    }

    /// <summary>
    /// Tokenizes text and counts word frequencies.
    /// </summary>
    private List<WordFrequency> TokenizeAndCount(List<string> texts, WordCloudOptions options)
    {
        var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var text in texts)
        {
            var words = Tokenize(text);
            foreach (var word in words)
            {
                if (wordCounts.ContainsKey(word))
                {
                    wordCounts[word]++;
                }
                else
                {
                    wordCounts[word] = 1;
                }
            }
        }

        // Filter by minimum frequency and max words, then sort by frequency
        var frequencies = wordCounts
            .Where(kvp => kvp.Value >= options.MinFrequency)
            .Select(kvp => new WordFrequency { Word = kvp.Key, Frequency = kvp.Value })
            .OrderByDescending(wf => wf.Frequency)
            .ToList();

        if (options.MaxWords.HasValue)
        {
            frequencies = frequencies.Take(options.MaxWords.Value).ToList();
        }

        return frequencies;
    }

    /// <summary>
    /// Tokenizes text into words, normalizing and filtering stop words.
    /// </summary>
    private List<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        // Normalize: lowercase, remove punctuation
        var normalized = text.ToLowerInvariant();
        
        // Remove punctuation except apostrophes within words
        var sb = new StringBuilder();
        foreach (char c in normalized)
        {
            if (char.IsLetter(c) || c == '\'')
            {
                sb.Append(c);
            }
            else
            {
                sb.Append(' ');
            }
        }

        // Split into words
        var words = sb.ToString()
            .Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1) // Filter single characters
            .Where(w => !_stopWords.Contains(w)) // Filter stop words
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .ToList();

        return words;
    }

    /// <summary>
    /// Counts words in text.
    /// </summary>
    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return Tokenize(text).Count;
    }

    /// <summary>
    /// Loads English stop words list.
    /// </summary>
    private static HashSet<string> LoadStopWords()
    {
        // Common English stop words
        var stopWords = new[]
        {
            "a", "an", "and", "are", "as", "at", "be", "by", "for", "from",
            "has", "he", "in", "is", "it", "its", "of", "on", "that", "the",
            "to", "was", "were", "will", "with", "the", "this", "but", "they",
            "have", "had", "what", "said", "each", "which", "their", "time",
            "if", "up", "out", "many", "then", "them", "these", "so", "some",
            "her", "would", "make", "like", "him", "into", "has", "two", "more",
            "very", "after", "words", "long", "than", "first", "been", "call",
            "who", "oil", "its", "now", "find", "down", "day", "did", "get",
            "come", "made", "may", "part"
        };

        return new HashSet<string>(stopWords, StringComparer.OrdinalIgnoreCase);
    }
}


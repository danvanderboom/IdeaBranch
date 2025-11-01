using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;

namespace IdeaBranch.Infrastructure.Analytics;

/// <summary>
/// Coordinating service for analytics operations that delegates to specialized services.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly WordCloudService _wordCloudService;
    private readonly TimelineService _timelineService;

    /// <summary>
    /// Initializes a new instance with specialized services.
    /// </summary>
    public AnalyticsService(WordCloudService wordCloudService, TimelineService timelineService)
    {
        _wordCloudService = wordCloudService ?? throw new ArgumentNullException(nameof(wordCloudService));
        _timelineService = timelineService ?? throw new ArgumentNullException(nameof(timelineService));
    }

    /// <inheritdoc/>
    public Task<WordCloudData> GenerateWordCloudAsync(
        WordCloudOptions options,
        CancellationToken cancellationToken = default)
    {
        return _wordCloudService.GenerateWordCloudAsync(options, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<TimelineData> GenerateTimelineAsync(
        TimelineOptions options,
        CancellationToken cancellationToken = default)
    {
        return _timelineService.GenerateTimelineAsync(options, cancellationToken);
    }
}


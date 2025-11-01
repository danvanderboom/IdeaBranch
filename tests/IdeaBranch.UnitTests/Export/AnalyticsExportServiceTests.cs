using System;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Export;
using NUnit.Framework;

namespace IdeaBranch.UnitTests.Export;

public class AnalyticsExportServiceTests
{
    [Test]
    public async System.Threading.Tasks.Task ExportWordCloud_ToPng_ReturnsBytes()
    {
        var svc = new AnalyticsExportService();
        var data = new WordCloudData
        {
            WordFrequencies = new[]
            {
                new WordFrequency { Word = "alpha", Frequency = 5, Weight = 0.9 },
                new WordFrequency { Word = "beta", Frequency = 3, Weight = 0.5 },
                new WordFrequency { Word = "gamma", Frequency = 2, Weight = 0.2 }
            }
        };
        var theme = new VisualizationTheme { WordGradientStart = SkiaSharp.SKColors.Blue, WordGradientEnd = SkiaSharp.SKColors.Red };
        var bytes = await svc.ExportWordCloudToPngAsync(data, 400, 300, new ExportOptions { DpiScale = 1 }, theme, WordCloudLayout.Random);
        Assert.That(bytes, Is.Not.Null);
        Assert.That(bytes.Length, Is.GreaterThan(0));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportTimeline_ToSvg_ReturnsSvgContent()
    {
        var svc = new AnalyticsExportService();
        var now = DateTime.UtcNow.Date;
        var band = new TimelineBand
        {
            StartTime = now,
            EndTime = now.AddDays(1),
            Events = new[]
            {
                new TimelineEvent { Id = Guid.NewGuid(), Timestamp = now.AddHours(1), EventType = TimelineEventType.TopicCreated, Title = "A" },
                new TimelineEvent { Id = Guid.NewGuid(), Timestamp = now.AddHours(2), EventType = TimelineEventType.TopicUpdated, Title = "B" }
            }
        };
        var data = new TimelineData { Bands = new[] { band } };
        var svg = await svc.ExportTimelineToSvgAsync(data, 600, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme());
        Assert.That(string.IsNullOrWhiteSpace(svg), Is.False);
        Assert.That(svg, Does.Contain("<svg"));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportMap_ToPngAndSvg_Succeeds()
    {
        var svc = new AnalyticsExportService();
        var overlays = new (double, double, string?)[] { (0.25, 0.25, "P1"), (0.75, 0.6, "P2") };
        var png = await svc.ExportMapToPngAsync(overlays, 400, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme(), includeTiles: true, includeLegend: true);
        Assert.That(png, Is.Not.Null);
        Assert.That(png.Length, Is.GreaterThan(0));

        var svg = await svc.ExportMapToSvgAsync(overlays, 400, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme(), includeTiles: true, includeLegend: true);
        Assert.That(string.IsNullOrWhiteSpace(svg), Is.False);
        Assert.That(svg, Does.Contain("<svg"));
    }
}



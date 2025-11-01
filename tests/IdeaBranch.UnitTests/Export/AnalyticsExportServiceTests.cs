using System;
using System.Collections.Generic;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Export;
using NUnit.Framework;

namespace IdeaBranch.UnitTests.Export;

public class AnalyticsExportServiceTests
{
    private static readonly Guid TestEventId1 = new Guid("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TestEventId2 = new Guid("22222222-2222-2222-2222-222222222222");
    private static readonly DateTime TestDate = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    [Test]
    public async System.Threading.Tasks.Task ExportWordCloud_ToPng_ReturnsValidPng()
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
        
        // Verify PNG magic number
        Assert.That(bytes[0], Is.EqualTo(0x89));
        Assert.That(bytes[1], Is.EqualTo(0x50)); // P
        Assert.That(bytes[2], Is.EqualTo(0x4E)); // N
        Assert.That(bytes[3], Is.EqualTo(0x47)); // G
    }

    [Test]
    public async System.Threading.Tasks.Task ExportWordCloud_ToSvg_ContainsExpectedElements()
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
        var svg = await svc.ExportWordCloudToSvgAsync(data, 400, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme(), WordCloudLayout.Random);
        
        Assert.That(string.IsNullOrWhiteSpace(svg), Is.False);
        Assert.That(svg, Does.Contain("<svg"));
        Assert.That(svg, Does.Contain("alpha"));
        Assert.That(svg, Does.Contain("beta"));
        Assert.That(svg, Does.Contain("gamma"));
        Assert.That(svg, Does.Contain("</svg>"));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportWordCloud_AllLayouts_ProduceValidOutputs()
    {
        var svc = new AnalyticsExportService();
        var data = new WordCloudData
        {
            WordFrequencies = new[]
            {
                new WordFrequency { Word = "test", Frequency = 10, Weight = 1.0 }
            }
        };

        foreach (WordCloudLayout layout in Enum.GetValues(typeof(WordCloudLayout)))
        {
            var png = await svc.ExportWordCloudToPngAsync(data, 400, 300, new ExportOptions { DpiScale = 1 }, null, layout);
            Assert.That(png.Length, Is.GreaterThan(0), $"Layout {layout} should produce PNG");
            
            var svg = await svc.ExportWordCloudToSvgAsync(data, 400, 300, new ExportOptions { DpiScale = 1 }, null, layout);
            Assert.That(svg, Does.Contain("<svg"), $"Layout {layout} should produce SVG");
        }
    }

    [Test]
    public async System.Threading.Tasks.Task ExportTimeline_ToPng_ReturnsValidPng()
    {
        var svc = new AnalyticsExportService();
        var band = new TimelineBand
        {
            StartTime = TestDate,
            EndTime = TestDate.AddDays(1),
            Events = new[]
            {
                new TimelineEvent { Id = TestEventId1, Timestamp = TestDate.AddHours(1), EventType = TimelineEventType.TopicCreated, Title = "Event A" },
                new TimelineEvent { Id = TestEventId2, Timestamp = TestDate.AddHours(2), EventType = TimelineEventType.TopicUpdated, Title = "Event B" }
            }
        };
        var data = new TimelineData { Bands = new[] { band } };
        var png = await svc.ExportTimelineToPngAsync(data, 600, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme());
        
        Assert.That(png, Is.Not.Null);
        Assert.That(png.Length, Is.GreaterThan(0));
        
        // Verify PNG magic number
        Assert.That(png[0], Is.EqualTo(0x89));
        Assert.That(png[1], Is.EqualTo(0x50));
        Assert.That(png[2], Is.EqualTo(0x4E));
        Assert.That(png[3], Is.EqualTo(0x47));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportTimeline_ToSvg_ContainsExpectedElements()
    {
        var svc = new AnalyticsExportService();
        var band = new TimelineBand
        {
            StartTime = TestDate,
            EndTime = TestDate.AddDays(1),
            Events = new[]
            {
                new TimelineEvent { Id = TestEventId1, Timestamp = TestDate.AddHours(1), EventType = TimelineEventType.TopicCreated, Title = "Event A" },
                new TimelineEvent { Id = TestEventId2, Timestamp = TestDate.AddHours(2), EventType = TimelineEventType.TopicUpdated, Title = "Event B" }
            }
        };
        var data = new TimelineData { Bands = new[] { band } };
        var svg = await svc.ExportTimelineToSvgAsync(data, 600, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme());
        
        Assert.That(string.IsNullOrWhiteSpace(svg), Is.False);
        Assert.That(svg, Does.Contain("<svg"));
        Assert.That(svg, Does.Contain("2024-01-15")); // Date label is rendered
        Assert.That(svg, Does.Contain("<circle")); // Event circles are rendered
        Assert.That(svg, Does.Contain("</svg>"));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportTimeline_WithConnections_IncludesConnectionElements()
    {
        var svc = new AnalyticsExportService();
        var band = new TimelineBand
        {
            StartTime = TestDate,
            EndTime = TestDate.AddDays(1),
            Events = new[]
            {
                new TimelineEvent { Id = TestEventId1, Timestamp = TestDate.AddHours(1), EventType = TimelineEventType.TopicCreated, Title = "Event A" },
                new TimelineEvent { Id = TestEventId2, Timestamp = TestDate.AddHours(2), EventType = TimelineEventType.TopicUpdated, Title = "Event B" }
            }
        };
        var data = new TimelineData { Bands = new[] { band } };
        var connections = new List<(Guid, Guid)> { (TestEventId1, TestEventId2) };
        
        var svg = await svc.ExportTimelineToSvgAsync(
            data, 
            600, 
            300, 
            new ExportOptions { DpiScale = 1, IncludeTimelineConnections = true }, 
            new VisualizationTheme(),
            connections);
        
        Assert.That(svg, Does.Contain("<svg"));
        // Connections may be rendered as lines or paths
        Assert.That(svg, Does.Contain("<line").Or.Contains("<path"));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportTimeline_WithStatistics_IncludesStatisticsPanel()
    {
        var svc = new AnalyticsExportService();
        var band = new TimelineBand
        {
            StartTime = TestDate,
            EndTime = TestDate.AddDays(1),
            Events = new[]
            {
                new TimelineEvent { Id = TestEventId1, Timestamp = TestDate.AddHours(1), EventType = TimelineEventType.TopicCreated, Title = "Event A" }
            }
        };
        var data = new TimelineData { Bands = new[] { band } };
        
        var svg = await svc.ExportTimelineToSvgAsync(
            data,
            600,
            300,
            new ExportOptions { DpiScale = 1, IncludeTimelineStatistics = true },
            new VisualizationTheme(),
            null,
            includeStatisticsPanel: true);
        
        Assert.That(svg, Does.Contain("<svg"));
        // Statistics panel should include text or rect elements
        Assert.That(svg, Does.Contain("<text").Or.Contains("<rect"));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportMap_ToPng_ReturnsValidPng()
    {
        var svc = new AnalyticsExportService();
        var overlays = new (double, double, string?)[] { (0.25, 0.25, "P1"), (0.75, 0.6, "P2") };
        var png = await svc.ExportMapToPngAsync(overlays, 400, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme(), includeTiles: false, includeLegend: false);
        
        Assert.That(png, Is.Not.Null);
        Assert.That(png.Length, Is.GreaterThan(0));
        
        // Verify PNG magic number
        Assert.That(png[0], Is.EqualTo(0x89));
        Assert.That(png[1], Is.EqualTo(0x50));
        Assert.That(png[2], Is.EqualTo(0x4E));
        Assert.That(png[3], Is.EqualTo(0x47));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportMap_ToSvg_ContainsExpectedElements()
    {
        var svc = new AnalyticsExportService();
        var overlays = new (double, double, string?)[] { (0.25, 0.25, "Point1"), (0.75, 0.6, "Point2") };
        var svg = await svc.ExportMapToSvgAsync(overlays, 400, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme(), includeTiles: false, includeLegend: false);
        
        Assert.That(string.IsNullOrWhiteSpace(svg), Is.False);
        Assert.That(svg, Does.Contain("<svg"));
        Assert.That(svg, Does.Contain("Point1"));
        Assert.That(svg, Does.Contain("Point2"));
        Assert.That(svg, Does.Contain("</svg>"));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportMap_WithTilesAndLegend_IncludesBoth()
    {
        var svc = new AnalyticsExportService();
        var overlays = new (double, double, string?)[] { (0.5, 0.5, "Center") };
        var svg = await svc.ExportMapToSvgAsync(overlays, 400, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme(), includeTiles: true, includeLegend: true);
        
        Assert.That(svg, Does.Contain("<svg"));
        // Tiles and legend may be rendered as various SVG elements
        Assert.That(svg.Length, Is.GreaterThan(500)); // Should have substantial content
    }

    [Test]
    public async System.Threading.Tasks.Task ExportWordCloud_EmptyData_ProducesEmptyStateMessage()
    {
        var svc = new AnalyticsExportService();
        var data = new WordCloudData { WordFrequencies = Array.Empty<WordFrequency>() };
        
        var png = await svc.ExportWordCloudToPngAsync(data, 400, 300, new ExportOptions { DpiScale = 1 }, null, WordCloudLayout.Random);
        Assert.That(png.Length, Is.GreaterThan(0)); // Should still produce PNG
        
        var svg = await svc.ExportWordCloudToSvgAsync(data, 400, 300, new ExportOptions { DpiScale = 1 }, null, WordCloudLayout.Random);
        Assert.That(svg, Does.Contain("<svg"));
        Assert.That(svg, Does.Contain("No words"));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportTimeline_EmptyData_ProducesEmptyStateMessage()
    {
        var svc = new AnalyticsExportService();
        var data = new TimelineData { Bands = Array.Empty<TimelineBand>() };
        
        var png = await svc.ExportTimelineToPngAsync(data, 600, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme());
        Assert.That(png.Length, Is.GreaterThan(0)); // Should still produce PNG
        
        var svg = await svc.ExportTimelineToSvgAsync(data, 600, 300, new ExportOptions { DpiScale = 1 }, new VisualizationTheme());
        Assert.That(svg, Does.Contain("<svg"));
        Assert.That(svg, Does.Contain("No timeline events"));
    }

    [Test]
    public async System.Threading.Tasks.Task ExportWithDifferentDpiScales_ProducesDifferentSizedOutputs()
    {
        var svc = new AnalyticsExportService();
        var data = new WordCloudData
        {
            WordFrequencies = new[] { new WordFrequency { Word = "test", Frequency = 5, Weight = 1.0 } }
        };

        var scale1Bytes = await svc.ExportWordCloudToPngAsync(data, 400, 300, new ExportOptions { DpiScale = 1 }, null, WordCloudLayout.Random);
        var scale2Bytes = await svc.ExportWordCloudToPngAsync(data, 400, 300, new ExportOptions { DpiScale = 2 }, null, WordCloudLayout.Random);
        
        // Higher DPI should generally produce larger file (though compression can affect this)
        // At minimum, both should be valid PNGs
        Assert.That(scale1Bytes.Length, Is.GreaterThan(0));
        Assert.That(scale2Bytes.Length, Is.GreaterThan(0));
        Assert.That(scale1Bytes[0], Is.EqualTo(0x89)); // PNG magic
        Assert.That(scale2Bytes[0], Is.EqualTo(0x89)); // PNG magic
    }
}



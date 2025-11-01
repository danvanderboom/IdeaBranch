using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using SkiaSharp;

namespace IdeaBranch.Infrastructure.Export;

/// <summary>
/// Service for exporting analytics data to various formats.
/// </summary>
public class AnalyticsExportService
{
    /// <summary>
    /// Exports word cloud data to JSON format.
    /// </summary>
    public async Task<string> ExportWordCloudToJsonAsync(
        WordCloudData data,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return json;
        }, cancellationToken);
    }

    /// <summary>
    /// Exports word cloud data to CSV format.
    /// </summary>
    public async Task<string> ExportWordCloudToCsvAsync(
        WordCloudData data,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("Word,Frequency,Weight");

            // Data rows
            foreach (var wordFreq in data.WordFrequencies)
            {
                sb.AppendLine($"{EscapeCsvField(wordFreq.Word)},{wordFreq.Frequency},{wordFreq.Weight:F4}");
            }

            return sb.ToString();
        }, cancellationToken);
    }

    /// <summary>
    /// Exports timeline data to JSON format.
    /// </summary>
    public async Task<string> ExportTimelineToJsonAsync(
        TimelineData data,
        bool includeAllFields = false,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (includeAllFields)
            {
                // Create enhanced export object with all required fields
                var exportData = data.Events.Select(evt => new
                {
                    eventId = evt.Id.ToString(),
                    type = evt.EventType.ToString(),
                    title = evt.Title ?? string.Empty,
                    body = evt.Details ?? string.Empty,
                    start = evt.Timestamp,
                    end = (DateTime?)null, // Timeline events are point events
                    precision = "day", // Assume day precision for now
                    nodeId = evt.NodeId?.ToString(),
                    nodePath = (string?)null, // Would need to fetch from topic tree
                    tags = evt.TagIds.Select(id => id.ToString()).ToArray(),
                    annotationIds = Array.Empty<string>(), // Would need to fetch from annotations
                    source = GetSourceFromEventType(evt.EventType),
                    actor = "System", // Would need to fetch from event metadata
                    createdAt = evt.Timestamp,
                    updatedAt = evt.Timestamp
                }).ToArray();

                var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return json;
            }
            else
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return json;
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Exports timeline data to CSV format.
    /// </summary>
    public async Task<string> ExportTimelineToCsvAsync(
        TimelineData data,
        bool includeAllFields = false,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();

            if (includeAllFields)
            {
                // Header with all required fields
                sb.AppendLine("eventId,type,title,body,start,end,precision,nodeId,nodePath,tags,annotationIds,source,actor,createdAt,updatedAt");

                // Data rows
                foreach (var evt in data.Events)
                {
                    var tagString = string.Join(";", evt.TagIds.Select(id => id.ToString()));
                    var annotationIdsString = string.Empty; // Would need to fetch from annotations
                    var nodePath = string.Empty; // Would need to fetch from topic tree
                    var source = GetSourceFromEventType(evt.EventType);
                    var actor = "System"; // Would need to fetch from event metadata
                    
                    sb.AppendLine($"{EscapeCsvField(evt.Id.ToString())}," +
                                 $"{EscapeCsvField(evt.EventType.ToString())}," +
                                 $"{EscapeCsvField(evt.Title ?? "")}," +
                                 $"{EscapeCsvField(evt.Details ?? "")}," +
                                 $"{evt.Timestamp:O}," +
                                 $"," + // end
                                 $"day," + // precision
                                 $"{(evt.NodeId.HasValue ? EscapeCsvField(evt.NodeId.Value.ToString()) : "")}," +
                                 $"{EscapeCsvField(nodePath)}," +
                                 $"{EscapeCsvField(tagString)}," +
                                 $"{EscapeCsvField(annotationIdsString)}," +
                                 $"{EscapeCsvField(source)}," +
                                 $"{EscapeCsvField(actor)}," +
                                 $"{evt.Timestamp:O}," + // createdAt
                                 $"{evt.Timestamp:O}"); // updatedAt
                }
            }
            else
            {
                // Header (original format)
                sb.AppendLine("Timestamp,EventType,Title,Details,NodeId");

                // Data rows
                foreach (var evt in data.Events)
                {
                    sb.AppendLine($"{evt.Timestamp:O},{evt.EventType},{EscapeCsvField(evt.Title)},{EscapeCsvField(evt.Details ?? "")},{evt.NodeId}");
                }
            }

            return sb.ToString();
        }, cancellationToken);
    }

    /// <summary>
    /// Exports word cloud visualization to PNG image.
    /// </summary>
    public async Task<byte[]> ExportWordCloudToPngAsync(
        WordCloudData data,
        int width = 800,
        int height = 600,
        ExportOptions? options = null,
        VisualizationTheme? theme = null,
        WordCloudLayout layout = WordCloudLayout.Random,
        CancellationToken cancellationToken = default)
    {
        var exportOpts = options ?? new ExportOptions { Width = width, Height = height };
        var scaledWidth = exportOpts.ScaledWidth;
        var scaledHeight = exportOpts.ScaledHeight;

        return await Task.Run(() =>
        {
            using var surface = SKSurface.Create(new SKImageInfo(scaledWidth, scaledHeight));
            var canvas = surface.Canvas;

            // Clear background honoring theme/background options
            if (((theme == null) || theme.BackgroundType != BackgroundType.Transparent) &&
                (exportOpts.BackgroundColor.HasValue || (theme?.BackgroundColor.HasValue ?? false)))
            {
                var bgColor = theme?.BackgroundColor ?? exportOpts.BackgroundColor ?? SKColors.White;
                canvas.Clear(bgColor);
            }
            else
            {
                canvas.Clear(SKColors.Transparent);
            }

            // Calculate layout for words
            var words = data.WordFrequencies.Take(50).ToList(); // Limit to top 50 for readability
            if (words.Count == 0)
            {
                // Empty word cloud - show message
                using var paint = new SKPaint
                {
                    Color = SKColors.Gray,
                    TextSize = 24 * exportOpts.DpiScale,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText("No words to display", scaledWidth / 2, scaledHeight / 2, paint);
                return EncodeSurfaceToPng(surface);
            }

            var minSize = 12.0f * exportOpts.DpiScale;
            var maxSize = 72.0f * exportOpts.DpiScale;
            var minWeight = words.Min(w => w.Weight);
            var maxWeight = words.Max(w => w.Weight);
            var weightRange = maxWeight - minWeight;

            var positions = new List<(float x, float y)>();

            if (layout == WordCloudLayout.Spiral)
            {
                PlaceWordsSpiral(words, positions, scaledWidth, scaledHeight, minSize, maxSize, minWeight, maxWeight);
            }
            else if (layout == WordCloudLayout.ForceDirected)
            {
                PlaceWordsForceDirected(words, positions, scaledWidth, scaledHeight);
            }
            else
            {
                PlaceWordsRandom(words, positions, scaledWidth, scaledHeight, minSize, maxSize, minWeight, maxWeight);
            }

            for (int i = 0; i < words.Count; i++)
            {
                var wordFreq = words[i];
                var fontSize = weightRange > 0
                    ? (float)(minSize + (wordFreq.Weight - minWeight) / weightRange * (maxSize - minSize))
                    : minSize;

                var (x, y) = positions[i];

                using var paint = new SKPaint
                {
                    Color = GetWordColor(wordFreq.Weight, minWeight, maxWeight, theme),
                    TextSize = fontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };

                if (theme?.FontFamily != null)
                {
                    paint.Typeface = SKTypeface.FromFamilyName(theme.FontFamily);
                }

                canvas.DrawText(wordFreq.Word, x, y, paint);
            }

            return EncodeSurfaceToPng(surface);
        }, cancellationToken);
    }

    /// <summary>
    /// Exports word cloud visualization to SVG format.
    /// </summary>
    public async Task<string> ExportWordCloudToSvgAsync(
        WordCloudData data,
        int width = 800,
        int height = 600,
        ExportOptions? options = null,
        VisualizationTheme? theme = null,
        WordCloudLayout layout = WordCloudLayout.Random,
        CancellationToken cancellationToken = default)
    {
        var exportOpts = options ?? new ExportOptions { Width = width, Height = height };
        var writer = new SvgWriter(exportOpts.ScaledWidth, exportOpts.ScaledHeight);

        return await Task.Run(() =>
        {
            writer.StartSvg();
            
            // Draw background
            if (((theme == null) || theme.BackgroundType != BackgroundType.Transparent) && (exportOpts.BackgroundColor.HasValue || (theme?.BackgroundColor.HasValue ?? false)))
            {
                var bgColor = theme?.BackgroundColor ?? exportOpts.BackgroundColor ?? SKColors.White;
                using var bgPaint = new SKPaint { Color = bgColor, Style = SKPaintStyle.Fill };
                writer.DrawRect(0, 0, exportOpts.ScaledWidth, exportOpts.ScaledHeight, bgPaint);
            }

            // Calculate layout for words
            var words = data.WordFrequencies.Take(50).ToList();
            if (words.Count == 0)
            {
                using var paint = new SKPaint
                {
                    Color = SKColors.Gray,
                    TextSize = 24 * exportOpts.DpiScale,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };
                writer.DrawText("No words to display", exportOpts.ScaledWidth / 2, exportOpts.ScaledHeight / 2, paint);
                writer.EndSvg();
                return writer.GetContent();
            }

            var minSize = 12.0f * exportOpts.DpiScale;
            var maxSize = 72.0f * exportOpts.DpiScale;
            var minWeight = words.Min(w => w.Weight);
            var maxWeight = words.Max(w => w.Weight);
            var weightRange = maxWeight - minWeight;

            var positions = new List<(float x, float y)>();

            if (layout == WordCloudLayout.Spiral)
            {
                PlaceWordsSpiral(words, positions, exportOpts.ScaledWidth, exportOpts.ScaledHeight, minSize, maxSize, minWeight, maxWeight);
            }
            else if (layout == WordCloudLayout.ForceDirected)
            {
                PlaceWordsForceDirected(words, positions, exportOpts.ScaledWidth, exportOpts.ScaledHeight);
            }
            else
            {
                PlaceWordsRandom(words, positions, exportOpts.ScaledWidth, exportOpts.ScaledHeight, minSize, maxSize, minWeight, maxWeight);
            }

            for (int i = 0; i < words.Count; i++)
            {
                var wordFreq = words[i];
                var fontSize = weightRange > 0
                    ? (float)(minSize + (wordFreq.Weight - minWeight) / weightRange * (maxSize - minSize))
                    : minSize;

                var (x, y) = positions[i];

                var color = GetWordColor(wordFreq.Weight, minWeight, maxWeight, theme);
                using var paint = new SKPaint
                {
                    Color = color,
                    TextSize = fontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };

                if (theme?.FontFamily != null)
                {
                    paint.Typeface = SKTypeface.FromFamilyName(theme.FontFamily);
                }

                writer.DrawText(wordFreq.Word, x, y, paint);
            }

            writer.EndSvg();
            return writer.GetContent();
        }, cancellationToken);
    }

    /// <summary>
    /// Exports timeline visualization to PNG image.
    /// </summary>
    public async Task<byte[]> ExportTimelineToPngAsync(
        TimelineData data,
        int width = 1200,
        int height = 600,
        ExportOptions? options = null,
        VisualizationTheme? theme = null,
        IReadOnlyList<(Guid fromEventId, Guid toEventId)>? connections = null,
        bool includeStatisticsPanel = false,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            // Clear background with theming
            if (((theme == null) || theme.BackgroundType != BackgroundType.Transparent))
            {
                var bg = theme?.BackgroundColor ?? SKColors.White;
                canvas.Clear(bg);
            }
            else
            {
                canvas.Clear(SKColors.Transparent);
            }

            if (data.Bands.Count == 0)
            {
                // Empty timeline - show message
                using var paint = new SKPaint
                {
                    Color = SKColors.Gray,
                    TextSize = 24,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText("No timeline events to display", width / 2, height / 2, paint);
                return EncodeSurfaceToPng(surface);
            }

            // Draw timeline bands
            var padding = 50f;
            var bandHeight = (height - padding * 2) / Math.Max(data.Bands.Count, 1);
            var timelineWidth = width - padding * 2;

            var earliestTime = data.Bands.Min(b => b.StartTime);
            var latestTime = data.Bands.Max(b => b.EndTime);
            var timeRange = (latestTime - earliestTime).TotalDays;
            if (timeRange < 1)
                timeRange = 1;

            var yPos = padding;
            var eventPositions = new Dictionary<Guid, (float x, float y)>();
            foreach (var band in data.Bands)
            {
                // Draw band background
                using (var paint = new SKPaint
                {
                    Color = SKColors.LightGray.WithAlpha(128),
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(padding, yPos, timelineWidth, bandHeight - 2, paint);
                }

                // Calculate x position for events
                foreach (var evt in band.Events)
                {
                    var daysFromStart = (evt.Timestamp - earliestTime).TotalDays;
                    var xPos = padding + (float)(daysFromStart / timeRange * timelineWidth);

                    // Draw event marker
                    using var paint = new SKPaint
                    {
                        Color = GetColorForEventType(evt.EventType),
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill
                    };
                    var cy = yPos + bandHeight / 2;
                    canvas.DrawCircle(xPos, cy, 4, paint);
                    eventPositions[evt.Id] = (xPos, cy);
                }

                // Draw band label (date range)
                using (var textPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 10,
                    IsAntialias = true
                })
                {
                    var label = $"{band.StartTime:yyyy-MM-dd}";
                    canvas.DrawText(label, padding + 5, yPos + bandHeight / 2 + 3, textPaint);
                }

                yPos += bandHeight;
            }

            // Draw connections if provided
            if (connections != null && connections.Count > 0)
            {
                using var connPaint = new SKPaint
                {
                    Color = SKColors.DarkGray,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.5f
                };

                foreach (var (fromEventId, toEventId) in connections)
                {
                    if (eventPositions.TryGetValue(fromEventId, out var p0) && eventPositions.TryGetValue(toEventId, out var p1))
                    {
                        canvas.DrawLine(p0.x, p0.y, p1.x, p1.y, connPaint);
                    }
                }
            }

            // Draw statistics panel if requested
            if (includeStatisticsPanel)
            {
                DrawTimelineStatsPanelSkia(canvas, data, width, height);
            }

            return EncodeSurfaceToPng(surface);
        }, cancellationToken);
    }

    /// <summary>
    /// Exports timeline visualization to SVG format with optional connections.
    /// </summary>
    public async Task<string> ExportTimelineToSvgAsync(
        TimelineData data,
        int width = 1200,
        int height = 600,
        ExportOptions? options = null,
        VisualizationTheme? theme = null,
        IReadOnlyList<(Guid fromEventId, Guid toEventId)>? connections = null,
        bool includeStatisticsPanel = false,
        CancellationToken cancellationToken = default)
    {
        var exportOpts = options ?? new ExportOptions { Width = width, Height = height };
        var writer = new SvgWriter(exportOpts.ScaledWidth, exportOpts.ScaledHeight);

        return await Task.Run(() =>
        {
            writer.StartSvg();

            // Background
            if (((theme == null) || theme.BackgroundType != BackgroundType.Transparent))
            {
                var bg = theme?.BackgroundColor ?? SKColors.White;
                using var bgPaint = new SKPaint { Color = bg, Style = SKPaintStyle.Fill };
                writer.DrawRect(0, 0, exportOpts.ScaledWidth, exportOpts.ScaledHeight, bgPaint);
            }

            var padding = 50f;
            var bandHeight = (exportOpts.ScaledHeight - padding * 2) / Math.Max(data.Bands.Count, 1);
            var timelineWidth = exportOpts.ScaledWidth - padding * 2;

            if (data.Bands.Count == 0)
            {
                using var paint = new SKPaint { Color = SKColors.Gray, TextSize = 24 * exportOpts.DpiScale, IsAntialias = true, TextAlign = SKTextAlign.Center };
                writer.DrawText("No timeline events to display", exportOpts.ScaledWidth / 2, exportOpts.ScaledHeight / 2, paint);
                writer.EndSvg();
                return writer.GetContent();
            }

            var earliestTime = data.Bands.Min(b => b.StartTime);
            var latestTime = data.Bands.Max(b => b.EndTime);
            var timeRange = (latestTime - earliestTime).TotalDays;
            if (timeRange < 1) timeRange = 1;

            var yPos = padding;
            var eventPositions = new Dictionary<Guid, (float x, float y)>();

            foreach (var band in data.Bands)
            {
                using (var bandPaint = new SKPaint { Color = SKColors.LightGray.WithAlpha(128), IsAntialias = true })
                {
                    writer.DrawRect(padding, yPos, timelineWidth, bandHeight - 2, bandPaint);
                }

                foreach (var evt in band.Events)
                {
                    var daysFromStart = (evt.Timestamp - earliestTime).TotalDays;
                    var xPos = padding + (float)(daysFromStart / timeRange * timelineWidth);
                    var cy = yPos + bandHeight / 2;

                    using var circlePaint = new SKPaint { Color = GetColorForEventType(evt.EventType), IsAntialias = true, Style = SKPaintStyle.Fill };
                    writer.DrawCircle(xPos, cy, 4, circlePaint);
                    eventPositions[evt.Id] = (xPos, cy);
                }

                using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 10 * exportOpts.DpiScale, IsAntialias = true })
                {
                    var label = $"{band.StartTime:yyyy-MM-dd}";
                    writer.DrawText(label, padding + 5, yPos + bandHeight / 2 + 3, textPaint);
                }

                yPos += bandHeight;
            }

            if (connections != null && connections.Count > 0)
            {
                using var connPaint = new SKPaint { Color = SKColors.DarkGray, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
                foreach (var (fromEventId, toEventId) in connections)
                {
                    if (eventPositions.TryGetValue(fromEventId, out var p0) && eventPositions.TryGetValue(toEventId, out var p1))
                    {
                        writer.DrawLine(p0.x, p0.y, p1.x, p1.y, connPaint);
                    }
                }
            }

            if (includeStatisticsPanel)
            {
                DrawTimelineStatsPanelSvg(writer, data, exportOpts.ScaledWidth, exportOpts.ScaledHeight, exportOpts.DpiScale);
            }

            writer.EndSvg();
            return writer.GetContent();
        }, cancellationToken);
    }

    private void DrawTimelineStatsPanelSkia(SKCanvas canvas, TimelineData data, int width, int height)
    {
        var padding = 10f;
        var panelWidth = Math.Min(220f, width * 0.25f);
        var panelHeight = 12f + 5 * 18f; // header + ~5 lines
        var x = width - panelWidth - padding;
        var y = padding;

        using (var bg = new SKPaint { Color = new SKColor(255, 255, 255, 230), IsAntialias = true, Style = SKPaintStyle.Fill })
        {
            canvas.DrawRect(x, y, panelWidth, panelHeight, bg);
        }

        using var text = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 12f };
        canvas.DrawText("Statistics", x + 8, y + 18, text);

        var events = data.Bands.SelectMany(b => b.Events);
        var counts = events.GroupBy(e => e.EventType)
            .Select(g => (Type: g.Key, Count: g.Count()))
            .OrderByDescending(t => t.Count)
            .ToList();

        float lineY = y + 36;
        foreach (var (type, count) in counts)
        {
            canvas.DrawText($"{type}: {count}", x + 8, lineY, text);
            lineY += 18f;
        }
    }

    private void DrawTimelineStatsPanelSvg(SvgWriter writer, TimelineData data, float width, float height, int dpiScale)
    {
        var padding = 10f * dpiScale;
        var panelWidth = Math.Min(220f * dpiScale, width * 0.25f);
        var panelHeight = 12f * dpiScale + 5 * 18f * dpiScale;
        var x = width - panelWidth - padding;
        var y = padding;

        using (var bg = new SKPaint { Color = new SKColor(255, 255, 255, 230), IsAntialias = true, Style = SKPaintStyle.Fill })
        {
            writer.DrawRect(x, y, panelWidth, panelHeight, bg);
        }

        using var text = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 12f * dpiScale };
        writer.DrawText("Statistics", x + 8 * dpiScale, y + 18 * dpiScale, text);

        var events = data.Bands.SelectMany(b => b.Events);
        var counts = events.GroupBy(e => e.EventType)
            .Select(g => (Type: g.Key, Count: g.Count()))
            .OrderByDescending(t => t.Count)
            .ToList();

        float lineY = y + 36 * dpiScale;
        foreach (var (type, count) in counts)
        {
            writer.DrawText($"{type}: {count}", x + 8 * dpiScale, lineY, text);
            lineY += 18f * dpiScale;
        }
    }

    /// <summary>
    /// Gets a color for a word weight.
    /// </summary>
    private SKColor GetColorForWeight(double weight, double minWeight, double maxWeight)
    {
        // Interpolate from blue (low) to red (high)
        var normalized = maxWeight > minWeight
            ? (weight - minWeight) / (maxWeight - minWeight)
            : 0.0;

        var r = (byte)(normalized * 255);
        var b = (byte)((1 - normalized) * 255);
        var g = (byte)(128);

        return new SKColor(r, g, b);
    }

    private SKColor GetWordColor(double weight, double minWeight, double maxWeight, VisualizationTheme? theme)
    {
        if (theme?.WordGradientStart.HasValue == true && theme.WordGradientEnd.HasValue)
        {
            var t = maxWeight > minWeight ? (float)((weight - minWeight) / (maxWeight - minWeight)) : 0f;
            var start = theme.WordGradientStart.Value;
            var end = theme.WordGradientEnd.Value;
            byte r = (byte)(start.Red + (end.Red - start.Red) * t);
            byte g = (byte)(start.Green + (end.Green - start.Green) * t);
            byte b = (byte)(start.Blue + (end.Blue - start.Blue) * t);
            byte a = (byte)(start.Alpha + (end.Alpha - start.Alpha) * t);
            return new SKColor(r, g, b, a);
        }
        return GetColorForWeight(weight, minWeight, maxWeight);
    }

    private void PlaceWordsRandom(List<WordFrequency> words, List<(float x, float y)> positions, int width, int height, float minSize, float maxSize, double minWeight, double maxWeight)
    {
        var random = new Random();
        var weightRange = maxWeight - minWeight;
        foreach (var word in words)
        {
            var fontSize = weightRange > 0
                ? (float)(minSize + (word.Weight - minWeight) / weightRange * (maxSize - minSize))
                : minSize;
            using var tempPaint = new SKPaint { TextSize = fontSize, IsAntialias = true };
            var textWidth = tempPaint.MeasureText(word.Word);
            var textHeight = fontSize;

            int attempts = 0;
            float x = 0, y = 0;
            bool placed = false;
            while (attempts < 80 && !placed)
            {
                x = random.Next((int)(textWidth / 2), (int)(width - textWidth / 2));
                y = random.Next((int)(textHeight), (int)(height - textHeight));
                bool collision = false;
                foreach (var (px, py) in positions)
                {
                    var distance = Math.Sqrt(Math.Pow(x - px, 2) + Math.Pow(y - py, 2));
                    if (distance < textWidth + 20)
                    {
                        collision = true;
                        break;
                    }
                }
                if (!collision)
                {
                    positions.Add((x, y));
                    placed = true;
                }
                attempts++;
            }
            if (!placed)
            {
                positions.Add((Math.Max(textWidth, 0) / 2f, Math.Max(textHeight, 0)));
            }
        }
    }

    private void PlaceWordsSpiral(List<WordFrequency> words, List<(float x, float y)> positions, int width, int height, float minSize, float maxSize, double minWeight, double maxWeight)
    {
        var centerX = width / 2f;
        var centerY = height / 2f;
        var weightRange = maxWeight - minWeight;
        foreach (var word in words)
        {
            var fontSize = weightRange > 0
                ? (float)(minSize + (word.Weight - minWeight) / weightRange * (maxSize - minSize))
                : minSize;
            using var tempPaint = new SKPaint { TextSize = fontSize, IsAntialias = true };
            var textWidth = tempPaint.MeasureText(word.Word);
            var textHeight = fontSize;

            float theta = 0f;
            bool placed = false;
            int attempts = 0;
            while (!placed && attempts < 1000)
            {
                var a = 4f; // spiral tightness
                var radius = a + 2f * theta;
                var x = centerX + radius * (float)Math.Cos(theta);
                var y = centerY + radius * (float)Math.Sin(theta);
                if (x - textWidth / 2f >= 0 && x + textWidth / 2f <= width && y - textHeight >= 0 && y <= height)
                {
                    bool collision = false;
                    foreach (var (px, py) in positions)
                    {
                        var distance = Math.Sqrt(Math.Pow(x - px, 2) + Math.Pow(y - py, 2));
                        if (distance < textWidth + 20)
                        {
                            collision = true;
                            break;
                        }
                    }
                    if (!collision)
                    {
                        positions.Add((x, y));
                        placed = true;
                        break;
                    }
                }
                theta += 0.2f;
                attempts++;
            }
            if (!placed)
            {
                positions.Add((centerX, centerY));
            }
        }
    }

    private void PlaceWordsForceDirected(List<WordFrequency> words, List<(float x, float y)> positions, int width, int height)
    {
        var centerX = width / 2f;
        var centerY = height / 2f;
        var random = new Random();

        for (int i = 0; i < words.Count; i++)
        {
            var angle = (float)(random.NextDouble() * Math.PI * 2);
            var radius = 10f + (float)(random.NextDouble() * Math.Min(width, height) / 8f);
            positions.Add((centerX + radius * (float)Math.Cos(angle), centerY + radius * (float)Math.Sin(angle)));
        }

        for (int iter = 0; iter < 200; iter++)
        {
            for (int i = 0; i < words.Count; i++)
            {
                var posI = positions[i];
                var forceX = 0f;
                var forceY = 0f;
                for (int j = 0; j < words.Count; j++)
                {
                    if (i == j) continue;
                    var posJ = positions[j];
                    var dx = posI.x - posJ.x;
                    var dy = posI.y - posJ.y;
                    var distSq = dx * dx + dy * dy + 0.01f;
                    var repulse = 10000f / distSq;
                    forceX += dx * repulse;
                    forceY += dy * repulse;
                }
                forceX += (centerX - posI.x) * 0.01f;
                forceY += (centerY - posI.y) * 0.01f;

                positions[i] = (Clamp(posI.x + forceX * 0.0005f, 0, width), Clamp(posI.y + forceY * 0.0005f, 0, height));
            }
        }
    }

    private float Clamp(float v, float min, float max) => v < min ? min : (v > max ? max : v);

    /// <summary>
    /// Exports a simple map visualization to PNG with optional tile grid and overlays.
    /// Overlays use normalized coordinates in [0,1] relative to the viewport.
    /// </summary>
    public async Task<byte[]> ExportMapToPngAsync(
        IReadOnlyList<(double xNorm, double yNorm, string? label)> overlays,
        int width = 1200,
        int height = 800,
        ExportOptions? options = null,
        VisualizationTheme? theme = null,
        bool includeTiles = false,
        bool includeLegend = false,
        CancellationToken cancellationToken = default)
    {
        var exportOpts = options ?? new ExportOptions { Width = width, Height = height };
        var scaledWidth = exportOpts.ScaledWidth;
        var scaledHeight = exportOpts.ScaledHeight;

        return await Task.Run(() =>
        {
            using var surface = SKSurface.Create(new SKImageInfo(scaledWidth, scaledHeight));
            var canvas = surface.Canvas;

            if (((theme == null) || theme.BackgroundType != BackgroundType.Transparent) &&
                (exportOpts.BackgroundColor.HasValue || (theme?.BackgroundColor.HasValue ?? false)))
            {
                var bg = theme?.BackgroundColor ?? exportOpts.BackgroundColor ?? SKColors.White;
                canvas.Clear(bg);
            }
            else
            {
                canvas.Clear(SKColors.Transparent);
            }

            // Optional tile grid (placeholder for map tiles)
            if (includeTiles)
            {
                using var gridPaint = new SKPaint { Color = new SKColor(220, 220, 220), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
                var tileSize = 128 * exportOpts.DpiScale;
                for (int x = 0; x <= scaledWidth; x += tileSize)
                {
                    canvas.DrawLine(x, 0, x, scaledHeight, gridPaint);
                }
                for (int y = 0; y <= scaledHeight; y += tileSize)
                {
                    canvas.DrawLine(0, y, scaledWidth, y, gridPaint);
                }
            }

            // Draw overlays
            using var pointPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true, Style = SKPaintStyle.Fill };
            using var labelPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 12f * exportOpts.DpiScale };
            foreach (var (xNorm, yNorm, label) in overlays)
            {
                var x = (float)(xNorm * scaledWidth);
                var y = (float)(yNorm * scaledHeight);
                canvas.DrawCircle(x, y, 4 * exportOpts.DpiScale, pointPaint);
                if (!string.IsNullOrWhiteSpace(label))
                {
                    canvas.DrawText(label!, x + 6 * exportOpts.DpiScale, y - 6 * exportOpts.DpiScale, labelPaint);
                }
            }

            // Optional legend
            if (includeLegend)
            {
                using var bg = new SKPaint { Color = new SKColor(255, 255, 255, 230), Style = SKPaintStyle.Fill };
                var legendWidth = 160f * exportOpts.DpiScale;
                var legendHeight = 40f * exportOpts.DpiScale;
                var lx = scaledWidth - legendWidth - 10 * exportOpts.DpiScale;
                var ly = 10 * exportOpts.DpiScale;
                canvas.DrawRect(lx, ly, legendWidth, legendHeight, bg);
                canvas.DrawCircle(lx + 14 * exportOpts.DpiScale, ly + 20 * exportOpts.DpiScale, 4 * exportOpts.DpiScale, pointPaint);
                using var text = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 12f * exportOpts.DpiScale };
                canvas.DrawText("Overlay point", lx + 26 * exportOpts.DpiScale, ly + 24 * exportOpts.DpiScale, text);
            }

            return EncodeSurfaceToPng(surface);
        }, cancellationToken);
    }

    /// <summary>
    /// Exports a simple map visualization to SVG with optional tile grid and overlays.
    /// Overlays use normalized coordinates in [0,1] relative to the viewport.
    /// </summary>
    public async Task<string> ExportMapToSvgAsync(
        IReadOnlyList<(double xNorm, double yNorm, string? label)> overlays,
        int width = 1200,
        int height = 800,
        ExportOptions? options = null,
        VisualizationTheme? theme = null,
        bool includeTiles = false,
        bool includeLegend = false,
        CancellationToken cancellationToken = default)
    {
        var exportOpts = options ?? new ExportOptions { Width = width, Height = height };
        var writer = new SvgWriter(exportOpts.ScaledWidth, exportOpts.ScaledHeight);

        return await Task.Run(() =>
        {
            writer.StartSvg();

            if (((theme == null) || theme.BackgroundType != BackgroundType.Transparent) &&
                (exportOpts.BackgroundColor.HasValue || (theme?.BackgroundColor.HasValue ?? false)))
            {
                var bg = theme?.BackgroundColor ?? exportOpts.BackgroundColor ?? SKColors.White;
                using var bgPaint = new SKPaint { Color = bg, Style = SKPaintStyle.Fill };
                writer.DrawRect(0, 0, exportOpts.ScaledWidth, exportOpts.ScaledHeight, bgPaint);
            }

            if (includeTiles)
            {
                using var gridPaint = new SKPaint { Color = new SKColor(220, 220, 220), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
                var tileSize = 128 * exportOpts.DpiScale;
                for (int x = 0; x <= exportOpts.ScaledWidth; x += tileSize)
                {
                    writer.DrawLine(x, 0, x, exportOpts.ScaledHeight, gridPaint);
                }
                for (int y = 0; y <= exportOpts.ScaledHeight; y += tileSize)
                {
                    writer.DrawLine(0, y, exportOpts.ScaledWidth, y, gridPaint);
                }
            }

            using var pointPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true, Style = SKPaintStyle.Fill };
            using var labelPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 12f * exportOpts.DpiScale };
            foreach (var (xNorm, yNorm, label) in overlays)
            {
                var x = (float)(xNorm * exportOpts.ScaledWidth);
                var y = (float)(yNorm * exportOpts.ScaledHeight);
                writer.DrawCircle(x, y, 4 * exportOpts.DpiScale, pointPaint);
                if (!string.IsNullOrWhiteSpace(label))
                {
                    writer.DrawText(label!, x + 6 * exportOpts.DpiScale, y - 6 * exportOpts.DpiScale, labelPaint);
                }
            }

            if (includeLegend)
            {
                using var bg = new SKPaint { Color = new SKColor(255, 255, 255, 230), Style = SKPaintStyle.Fill };
                var legendWidth = 160f * exportOpts.DpiScale;
                var legendHeight = 40f * exportOpts.DpiScale;
                var lx = exportOpts.ScaledWidth - legendWidth - 10 * exportOpts.DpiScale;
                var ly = 10 * exportOpts.DpiScale;
                writer.DrawRect(lx, ly, legendWidth, legendHeight, bg);
                writer.DrawCircle(lx + 14 * exportOpts.DpiScale, ly + 20 * exportOpts.DpiScale, 4 * exportOpts.DpiScale, pointPaint);
                using var text = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 12f * exportOpts.DpiScale };
                writer.DrawText("Overlay point", lx + 26 * exportOpts.DpiScale, ly + 24 * exportOpts.DpiScale, text);
            }

            writer.EndSvg();
            return writer.GetContent();
        }, cancellationToken);
    }

    /// <summary>
    /// Gets a color for an event type.
    /// </summary>
    private SKColor GetColorForEventType(TimelineEventType eventType)
    {
        return eventType switch
        {
            TimelineEventType.TopicCreated => SKColors.Blue,
            TimelineEventType.TopicUpdated => SKColors.DarkBlue,
            TimelineEventType.AnnotationCreated => SKColors.Green,
            TimelineEventType.AnnotationUpdated => SKColors.DarkGreen,
            TimelineEventType.ConversationMessage => SKColors.Orange,
            _ => SKColors.Gray
        };
    }

    /// <summary>
    /// Encodes a SkiaSharp surface to PNG bytes.
    /// </summary>
    private byte[] EncodeSurfaceToPng(SKSurface surface)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream();
        data.SaveTo(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Escapes a field for CSV format.
    /// </summary>
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // If field contains comma, quote, or newline, wrap in quotes and escape quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    /// <summary>
    /// Gets the source name from an event type.
    /// </summary>
    private string GetSourceFromEventType(TimelineEventType eventType)
    {
        return eventType switch
        {
            TimelineEventType.TopicCreated => "Topics",
            TimelineEventType.TopicUpdated => "Topics",
            TimelineEventType.AnnotationCreated => "Annotations",
            TimelineEventType.AnnotationUpdated => "Annotations",
            TimelineEventType.ConversationMessage => "Conversations",
            _ => "Unknown"
        };
    }
}


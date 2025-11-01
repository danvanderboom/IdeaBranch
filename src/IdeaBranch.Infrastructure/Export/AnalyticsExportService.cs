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
        CancellationToken cancellationToken = default)
    {
        var exportOpts = options ?? new ExportOptions { Width = width, Height = height };
        var scaledWidth = exportOpts.ScaledWidth;
        var scaledHeight = exportOpts.ScaledHeight;

        return await Task.Run(() =>
        {
            using var surface = SKSurface.Create(new SKImageInfo(scaledWidth, scaledHeight));
            var canvas = surface.Canvas;

            // Clear background
            canvas.Clear(exportOpts.BackgroundColor ?? SKColors.White);

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
            var random = new Random();

            // Simple layout algorithm: place words randomly, avoiding overlaps
            foreach (var wordFreq in words)
            {
                var fontSize = weightRange > 0
                    ? (float)(minSize + (wordFreq.Weight - minWeight) / weightRange * (maxSize - minSize))
                    : minSize;

                using var tempPaint = new SKPaint
                {
                    TextSize = fontSize,
                    IsAntialias = true
                };

                var textWidth = tempPaint.MeasureText(wordFreq.Word);
                var textHeight = fontSize;

                // Try to place word (simple collision avoidance)
                int attempts = 0;
                float x = 0, y = 0;
                bool placed = false;

                while (attempts < 50 && !placed)
                {
                    x = random.Next((int)(textWidth / 2), (int)(scaledWidth - textWidth / 2));
                    y = random.Next((int)(textHeight), (int)(scaledHeight - textHeight));

                    // Check collision with existing positions
                    bool collision = false;
                    foreach (var (px, py) in positions)
                    {
                        var distance = Math.Sqrt(Math.Pow(x - px, 2) + Math.Pow(y - py, 2));
                        if (distance < textWidth + 20) // Add some spacing
                        {
                            collision = true;
                            break;
                        }
                    }

                    if (!collision)
                    {
                        placed = true;
                        positions.Add((x, y));
                    }

                    attempts++;
                }

                // Draw word
                using var paint = new SKPaint
                {
                    Color = GetColorForWeight(wordFreq.Weight, minWeight, maxWeight),
                    TextSize = fontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };

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
            var random = new Random();

            // Simple layout algorithm: place words randomly, avoiding overlaps
            foreach (var wordFreq in words)
            {
                var fontSize = weightRange > 0
                    ? (float)(minSize + (wordFreq.Weight - minWeight) / weightRange * (maxSize - minSize))
                    : minSize;

                using var tempPaint = new SKPaint
                {
                    TextSize = fontSize,
                    IsAntialias = true
                };

                var textWidth = tempPaint.MeasureText(wordFreq.Word);
                var textHeight = fontSize;

                int attempts = 0;
                float x = 0, y = 0;
                bool placed = false;

                while (attempts < 50 && !placed)
                {
                    x = random.Next((int)(textWidth / 2), (int)(exportOpts.ScaledWidth - textWidth / 2));
                    y = random.Next((int)(textHeight), (int)(exportOpts.ScaledHeight - textHeight));

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
                        placed = true;
                        positions.Add((x, y));
                    }

                    attempts++;
                }

                // Draw word to SVG
                var color = GetColorForWeight(wordFreq.Weight, minWeight, maxWeight);
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
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            // Clear background
            canvas.Clear(SKColors.White);

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
                    canvas.DrawCircle(xPos, yPos + bandHeight / 2, 4, paint);
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

            return EncodeSurfaceToPng(surface);
        }, cancellationToken);
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


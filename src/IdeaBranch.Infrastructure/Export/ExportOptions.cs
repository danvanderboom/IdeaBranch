using SkiaSharp;

namespace IdeaBranch.Infrastructure.Export;

/// <summary>
/// Options for visualization export with DPI scaling and format support.
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Gets or sets the DPI scale factor (1x-4x).
    /// </summary>
    public int DpiScale { get; set; } = 1;

    /// <summary>
    /// Gets or sets the base width in pixels (before DPI scaling).
    /// </summary>
    public int Width { get; set; } = 800;

    /// <summary>
    /// Gets or sets the base height in pixels (before DPI scaling).
    /// </summary>
    public int Height { get; set; } = 600;

    /// <summary>
    /// Gets or sets the background color (null for transparent).
    /// </summary>
    public SKColor? BackgroundColor { get; set; } = SKColors.White;

    /// <summary>
    /// Gets or sets whether to include legends in the export.
    /// </summary>
    public bool IncludeLegend { get; set; } = true;

    /// <summary>
    /// Gets or sets the word cloud layout algorithm.
    /// </summary>
    public WordCloudLayout WordCloudLayout { get; set; } = WordCloudLayout.Random;

    /// <summary>
    /// Gets or sets whether to include map tiles in map exports.
    /// </summary>
    public bool IncludeMapTiles { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include map legend in map exports.
    /// </summary>
    public bool IncludeMapLegend { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include timeline statistics panel in timeline exports.
    /// </summary>
    public bool IncludeTimelineStatistics { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include timeline event connections in timeline exports.
    /// </summary>
    public bool IncludeTimelineConnections { get; set; } = false;

    /// <summary>
    /// Gets the scaled width based on DPI scale.
    /// </summary>
    public int ScaledWidth => Width * DpiScale;

    /// <summary>
    /// Gets the scaled height based on DPI scale.
    /// </summary>
    public int ScaledHeight => Height * DpiScale;

    /// <summary>
    /// Gets the DPI value based on scale factor.
    /// </summary>
    public int Dpi => 72 * DpiScale;
}

/// <summary>
/// Theme options for visualizations.
/// </summary>
public class VisualizationTheme
{
    /// <summary>
    /// Gets or sets the color palette name.
    /// </summary>
    public string? ColorPalette { get; set; }

    /// <summary>
    /// Gets or sets the font family name.
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// Gets or sets the font size (base).
    /// </summary>
    public float? FontSize { get; set; }

    /// <summary>
    /// Gets or sets the background type.
    /// </summary>
    public BackgroundType BackgroundType { get; set; } = BackgroundType.Solid;

    /// <summary>
    /// Gets or sets the background color (for solid/gradient backgrounds).
    /// </summary>
    public SKColor? BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the background gradient start color.
    /// </summary>
    public SKColor? BackgroundGradientStart { get; set; }

    /// <summary>
    /// Gets or sets the background gradient end color.
    /// </summary>
    public SKColor? BackgroundGradientEnd { get; set; }

    /// <summary>
    /// Gets or sets the background image path (if using image background).
    /// </summary>
    public string? BackgroundImagePath { get; set; }

    /// <summary>
    /// Gets or sets the background opacity (0.0-1.0).
    /// </summary>
    public float BackgroundOpacity { get; set; } = 1.0f;

    /// <summary>
    /// Optional gradient start color for word coloring (frequency-based).
    /// When set with <see cref="WordGradientEnd"/>, word colors will be
    /// interpolated between start and end based on weight/frequency.
    /// </summary>
    public SKColor? WordGradientStart { get; set; }

    /// <summary>
    /// Optional gradient end color for word coloring (frequency-based).
    /// </summary>
    public SKColor? WordGradientEnd { get; set; }
}

/// <summary>
/// Background types for visualizations.
/// </summary>
public enum BackgroundType
{
    Solid,
    Gradient,
    Image,
    Transparent
}

/// <summary>
/// Word cloud layout options.
/// </summary>
public enum WordCloudLayout
{
    Random,
    Spiral,
    ForceDirected
}


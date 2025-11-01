using System.Globalization;
using System.Text;
using SkiaSharp;

namespace IdeaBranch.Infrastructure.Export;

/// <summary>
/// Helper class for writing SVG content from SkiaSharp drawing operations.
/// </summary>
public class SvgWriter
{
    private readonly StringBuilder _sb;
    private readonly float _width;
    private readonly float _height;
    private readonly Dictionary<string, string> _definitions;
    private int _gradientIdCounter;

    public SvgWriter(float width, float height)
    {
        _width = width;
        _height = height;
        _sb = new StringBuilder();
        _definitions = new Dictionary<string, string>();
        _gradientIdCounter = 1;
    }

    /// <summary>
    /// Starts the SVG document.
    /// </summary>
    public void StartSvg(string? viewBox = null)
    {
        var vb = viewBox ?? $"0 0 {_width:F2} {_height:F2}";
        _sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{_width:F2}\" height=\"{_height:F2}\" viewBox=\"{vb}\">");
        
        if (_definitions.Count > 0)
        {
            _sb.AppendLine("  <defs>");
            foreach (var def in _definitions.Values)
            {
                _sb.AppendLine($"    {def}");
            }
            _sb.AppendLine("  </defs>");
        }
    }

    /// <summary>
    /// Ends the SVG document.
    /// </summary>
    public void EndSvg()
    {
        _sb.AppendLine("</svg>");
    }

    /// <summary>
    /// Gets the SVG content.
    /// </summary>
    public string GetContent() => _sb.ToString();

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    public void DrawRect(float x, float y, float width, float height, SKPaint paint)
    {
        var style = paint.Style == SKPaintStyle.Fill ? "fill" : "stroke";
        var attrs = new StringBuilder();
        
        if (paint.Style == SKPaintStyle.Fill || paint.Color.Alpha == 255)
        {
            attrs.Append($" fill=\"{ColorToSvg(paint.Color)}\"");
            attrs.Append($" fill-opacity=\"{paint.Color.Alpha / 255.0f:F2}\"");
        }
        else
        {
            attrs.Append(" fill=\"none\"");
        }

        if (paint.Style == SKPaintStyle.Stroke || paint.Style == SKPaintStyle.StrokeAndFill)
        {
            attrs.Append($" stroke=\"{ColorToSvg(paint.Color)}\"");
            attrs.Append($" stroke-opacity=\"{paint.Color.Alpha / 255.0f:F2}\"");
            attrs.Append($" stroke-width=\"{paint.StrokeWidth:F2}\"");
        }

        _sb.AppendLine($"  <rect x=\"{x:F2}\" y=\"{y:F2}\" width=\"{width:F2}\" height=\"{height:F2}\"{attrs} />");
    }

    /// <summary>
    /// Draws a circle.
    /// </summary>
    public void DrawCircle(float cx, float cy, float radius, SKPaint paint)
    {
        var style = paint.Style == SKPaintStyle.Fill ? "fill" : "stroke";
        var attrs = new StringBuilder();
        
        if (paint.Style == SKPaintStyle.Fill || paint.Color.Alpha == 255)
        {
            attrs.Append($" fill=\"{ColorToSvg(paint.Color)}\"");
            attrs.Append($" fill-opacity=\"{paint.Color.Alpha / 255.0f:F2}\"");
        }
        else
        {
            attrs.Append(" fill=\"none\"");
        }

        if (paint.Style == SKPaintStyle.Stroke || paint.Style == SKPaintStyle.StrokeAndFill)
        {
            attrs.Append($" stroke=\"{ColorToSvg(paint.Color)}\"");
            attrs.Append($" stroke-opacity=\"{paint.Color.Alpha / 255.0f:F2}\"");
            attrs.Append($" stroke-width=\"{paint.StrokeWidth:F2}\"");
        }

        _sb.AppendLine($"  <circle cx=\"{cx:F2}\" cy=\"{cy:F2}\" r=\"{radius:F2}\"{attrs} />");
    }

    /// <summary>
    /// Draws a line.
    /// </summary>
    public void DrawLine(float x0, float y0, float x1, float y1, SKPaint paint)
    {
        var attrs = new StringBuilder();
        attrs.Append($" stroke=\"{ColorToSvg(paint.Color)}\"");
        attrs.Append($" stroke-opacity=\"{paint.Color.Alpha / 255.0f:F2}\"");
        attrs.Append($" stroke-width=\"{paint.StrokeWidth:F2}\"");

        _sb.AppendLine($"  <line x1=\"{x0:F2}\" y1=\"{y0:F2}\" x2=\"{x1:F2}\" y2=\"{y1:F2}\"{attrs} />");
    }

    /// <summary>
    /// Draws text.
    /// </summary>
    public void DrawText(string text, float x, float y, SKPaint paint)
    {
        var attrs = new StringBuilder();
        attrs.Append($" fill=\"{ColorToSvg(paint.Color)}\"");
        attrs.Append($" fill-opacity=\"{paint.Color.Alpha / 255.0f:F2}\"");
        attrs.Append($" font-size=\"{paint.TextSize:F2}\"");

        if (!string.IsNullOrEmpty(paint.Typeface?.FamilyName))
        {
            attrs.Append($" font-family=\"{EscapeXml(paint.Typeface.FamilyName)}\"");
        }

        if (paint.TextAlign == SKTextAlign.Center)
        {
            attrs.Append($" text-anchor=\"middle\"");
        }
        else if (paint.TextAlign == SKTextAlign.Right)
        {
            attrs.Append($" text-anchor=\"end\"");
        }

        var escapedText = EscapeXml(text);
        _sb.AppendLine($"  <text x=\"{x:F2}\" y=\"{y:F2}\"{attrs}>{escapedText}</text>");
    }

    /// <summary>
    /// Draws a path (polyline or path string).
    /// </summary>
    public void DrawPath(string pathData, SKPaint paint)
    {
        var attrs = new StringBuilder();
        
        if (paint.Style == SKPaintStyle.Fill || paint.Style == SKPaintStyle.StrokeAndFill)
        {
            attrs.Append($" fill=\"{ColorToSvg(paint.Color)}\"");
            attrs.Append($" fill-opacity=\"{paint.Color.Alpha / 255.0f:F2}\"");
        }
        else
        {
            attrs.Append(" fill=\"none\"");
        }

        if (paint.Style == SKPaintStyle.Stroke || paint.Style == SKPaintStyle.StrokeAndFill)
        {
            attrs.Append($" stroke=\"{ColorToSvg(paint.Color)}\"");
            attrs.Append($" stroke-opacity=\"{paint.Color.Alpha / 255.0f:F2}\"");
            attrs.Append($" stroke-width=\"{paint.StrokeWidth:F2}\"");
        }

        _sb.AppendLine($"  <path d=\"{pathData}\"{attrs} />");
    }

    /// <summary>
    /// Converts an SKColor to SVG hex format.
    /// </summary>
    private static string ColorToSvg(SKColor color)
    {
        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
    }

    /// <summary>
    /// Escapes XML special characters.
    /// </summary>
    private static string EscapeXml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}


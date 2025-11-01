using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdeaBranch.App.Services;
using IdeaBranch.Infrastructure.Export;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SkiaSharp;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for MapPage.
/// </summary>
public class MapViewModel : INotifyPropertyChanged
{
    private readonly AnalyticsExportService _exportService;
    private readonly SettingsService _settingsService;
    private bool _isExporting;

    /// <summary>
    /// Initializes a new instance with required services.
    /// </summary>
    public MapViewModel(
        AnalyticsExportService exportService,
        SettingsService settingsService)
    {
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        ExportPngCommand = new Command(async () => await ExportPngAsync(), () => !IsExporting);
        ExportSvgCommand = new Command(async () => await ExportSvgAsync(), () => !IsExporting);
    }

    /// <summary>
    /// Initializes a new instance from dependency injection (parameterless constructor for XAML).
    /// </summary>
    public MapViewModel() : this(
        GetService<AnalyticsExportService>(),
        GetService<SettingsService>())
    {
    }

    private static T GetService<T>() where T : notnull
    {
        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Services not available");
        return services.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets or sets whether an export is in progress.
    /// </summary>
    public bool IsExporting
    {
        get => _isExporting;
        private set
        {
            if (_isExporting != value)
            {
                _isExporting = value;
                OnPropertyChanged(nameof(IsExporting));
                ExportPngCommand.ChangeCanExecute();
                ExportSvgCommand.ChangeCanExecute();
            }
        }
    }

    /// <summary>
    /// Gets the command to export as PNG.
    /// </summary>
    public Command ExportPngCommand { get; }

    /// <summary>
    /// Gets the command to export as SVG.
    /// </summary>
    public Command ExportSvgCommand { get; }

    /// <summary>
    /// Exports map visualization to PNG.
    /// </summary>
    public async Task ExportPngAsync()
    {
        try
        {
            IsExporting = true;

            // Create sample overlays (in a real implementation, this would come from map data)
            var overlays = new List<(double xNorm, double yNorm, string? label)>();
            
            var (exportOptions, theme, includeTiles, includeLegend) = await BuildExportOptionsAsync();
            var pngBytes = await _exportService.ExportMapToPngAsync(
                overlays,
                exportOptions.Width,
                exportOptions.Height,
                exportOptions,
                theme,
                includeTiles,
                includeLegend);
            
            var fileName = $"map_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, pngBytes);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Map",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
            // Could show error message if error property is added
        }
        finally
        {
            IsExporting = false;
        }
    }

    /// <summary>
    /// Exports map visualization to SVG.
    /// </summary>
    public async Task ExportSvgAsync()
    {
        try
        {
            IsExporting = true;

            // Create sample overlays (in a real implementation, this would come from map data)
            var overlays = new List<(double xNorm, double yNorm, string? label)>();
            
            var (exportOptions, theme, includeTiles, includeLegend) = await BuildExportOptionsAsync();
            var svg = await _exportService.ExportMapToSvgAsync(
                overlays,
                exportOptions.Width,
                exportOptions.Height,
                exportOptions,
                theme,
                includeTiles,
                includeLegend);
            
            var fileName = $"map_{DateTime.UtcNow:yyyyMMddHHmmss}.svg";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllTextAsync(filePath, svg);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Map",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
            // Could show error message if error property is added
        }
        finally
        {
            IsExporting = false;
        }
    }

    /// <summary>
    /// Builds export options and theme from settings.
    /// </summary>
    private async Task<(ExportOptions options, VisualizationTheme theme, bool includeTiles, bool includeLegend)> BuildExportOptionsAsync()
    {
        var dpiScale = await _settingsService.GetExportDpiScaleAsync();
        var backgroundColorHex = await _settingsService.GetExportBackgroundColorAsync();
        var transparentBackground = await _settingsService.GetExportTransparentBackgroundAsync();
        var fontFamily = await _settingsService.GetExportFontFamilyAsync();
        var palette = await _settingsService.GetExportPaletteAsync();
        var includeLegend = await _settingsService.GetExportIncludeLegendAsync();

        // Get map-specific settings
        var includeTilesValue = await SecureStorage.GetAsync("map_include_tiles");
        var includeTiles = bool.TryParse(includeTilesValue, out var tiles) && tiles;

        var exportOptions = new ExportOptions
        {
            DpiScale = dpiScale,
            Width = 1200,
            Height = 800,
            IncludeLegend = includeLegend,
            IncludeMapTiles = includeTiles,
            IncludeMapLegend = includeLegend
        };

        if (!transparentBackground && !string.IsNullOrEmpty(backgroundColorHex))
        {
            if (SKColor.TryParse(backgroundColorHex, out var bgColor))
            {
                exportOptions.BackgroundColor = bgColor;
            }
        }

        var theme = new VisualizationTheme
        {
            ColorPalette = palette,
            FontFamily = fontFamily,
            BackgroundType = transparentBackground ? BackgroundType.Transparent : BackgroundType.Solid
        };

        if (!transparentBackground && !string.IsNullOrEmpty(backgroundColorHex))
        {
            if (SKColor.TryParse(backgroundColorHex, out var bgColor))
            {
                theme.BackgroundColor = bgColor;
            }
        }

        return (exportOptions, theme, includeTiles, includeLegend);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

using IdeaBranch.App.Services;
using Microsoft.Maui.Controls;

namespace IdeaBranch.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
	}

	protected override void OnNavigated(ShellNavigatedEventArgs args)
	{
		base.OnNavigated(args);
		
		// Emit navigation telemetry
		var telemetry = Handler?.MauiContext?.Services?.GetService<TelemetryService>();
		if (telemetry != null && !string.IsNullOrEmpty(args.Current?.Location?.OriginalString))
		{
			var pageName = args.Current.Location.OriginalString.Split('/').LastOrDefault() ?? "Unknown";
			
			// Map page names to telemetry event names
			var telemetryName = pageName switch
			{
				"TopicTreePage" => "topic_tree",
				"MapPage" => "map",
				"TimelinePage" => "timeline",
				_ => pageName.ToLowerInvariant()
			};
			
			telemetry.EmitNavigationEvent(telemetryName);
		}
	}
}

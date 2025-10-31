using System.Diagnostics;
using IdeaBranch.App.Services;
using IdeaBranch.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;

namespace IdeaBranch.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register telemetry service (consent-aware)
		builder.Services.AddSingleton<TelemetryService>();

		// Register resilience policies for HttpClientFactory and outbound I/O
		builder.Services.AddResiliencePolicies();

		// Register example API service with named HttpClient
		builder.Services.AddHttpClient("ExampleApi")
			.AddStandardResiliencePolicy("ExampleApi")
			.ConfigureHttpClient(client =>
			{
				client.BaseAddress = new Uri("https://httpbin.org");
				client.Timeout = TimeSpan.FromSeconds(30);
			});

		// Register example service for demonstration
		builder.Services.AddSingleton<IdeaBranch.App.Services.ExampleApiService>();

		// Enable OpenTelemetry ActivitySource listeners
		ActivitySource.AddActivityListener(new ActivityListener
		{
			ShouldListenTo = source => source.Name.StartsWith("IdeaBranch."),
			ActivityStarted = activity => { },
			ActivityStopped = activity => { }
		});

		return builder.Build();
	}
}

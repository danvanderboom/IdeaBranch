using System.Diagnostics;
using System.IO;
using IdeaBranch.App.Services;
using IdeaBranch.App.ViewModels;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Resilience;
using IdeaBranch.Infrastructure.Storage;
using IdeaBranch.Infrastructure.Sync;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

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

		// Register settings service
		builder.Services.AddSingleton<Services.SettingsService>();

		// Register LLM factory
		builder.Services.AddSingleton<Services.LLM.LLMClientFactory>(sp =>
		{
			var settings = sp.GetRequiredService<Services.SettingsService>();
			var loggerFactory = sp.GetService<ILoggerFactory>();
			return new Services.LLM.LLMClientFactory(settings, loggerFactory);
		});

		// Register database and repository
		builder.Services.AddSingleton<TopicDb>(sp =>
		{
			var dbPath = Path.Combine(FileSystem.AppDataDirectory, "ideabranch.db");
			return new TopicDb(dbPath);
		});
		builder.Services.AddSingleton<IVersionHistoryRepository>(sp =>
		{
			var db = sp.GetRequiredService<TopicDb>();
			return new SqliteVersionHistoryRepository(db.Connection);
		});
		builder.Services.AddSingleton<ITopicTreeRepository>(sp =>
		{
			var db = sp.GetRequiredService<TopicDb>();
			var versionHistoryRepository = sp.GetRequiredService<IVersionHistoryRepository>();
			return new SqliteTopicTreeRepository(db, versionHistoryRepository);
		});

		// Register ViewModels
		builder.Services.AddTransient<TopicTreeViewModel>(sp =>
		{
			var repository = sp.GetRequiredService<ITopicTreeRepository>();
			var llmFactory = sp.GetService<Services.LLM.LLMClientFactory>();
			var telemetry = sp.GetService<TelemetryService>();
			return new TopicTreeViewModel(repository, llmFactory, telemetry);
		});

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

		// Register sync services
		builder.Services.AddSingleton<IConnectivityService, MauiConnectivityService>();
		builder.Services.AddSingleton<IRemoteSyncClient, InMemoryRemoteSyncClient>();
		builder.Services.AddSingleton<ISyncService>(sp =>
		{
			var connectivityService = sp.GetRequiredService<IConnectivityService>();
			var remoteClient = sp.GetRequiredService<IRemoteSyncClient>();
			var repository = sp.GetRequiredService<ITopicTreeRepository>();
			var versionHistoryRepository = sp.GetRequiredService<IVersionHistoryRepository>();
			var db = sp.GetRequiredService<TopicDb>();
			var logger = sp.GetService<ILogger<SyncService>>();
			return new SyncService(connectivityService, remoteClient, repository, versionHistoryRepository, db.Connection, logger);
		});

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

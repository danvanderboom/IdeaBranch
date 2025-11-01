using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using IdeaBranch.App.Services;
using IdeaBranch.App.Services.Notifications;
using IdeaBranch.App.ViewModels;
using IdeaBranch.Presentation.ViewModels;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Analytics;
using IdeaBranch.Infrastructure.Export;
using IdeaBranch.Infrastructure.Resilience;
using IdeaBranch.Infrastructure.Storage;
using IdeaBranch.Infrastructure.Sync;
using IdeaBranch.App.ViewModels.Analytics;
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
			.UseMauiCommunityToolkit()
			.UseSkiaSharp()
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
		builder.Services.AddSingleton<INotificationsRepository>(sp =>
		{
			var db = sp.GetRequiredService<TopicDb>();
			return new SqliteNotificationsRepository(db.Connection);
		});
		builder.Services.AddSingleton<IAnnotationsRepository>(sp =>
		{
			var db = sp.GetRequiredService<TopicDb>();
			return new SqliteAnnotationsRepository(db.Connection);
		});
		builder.Services.AddSingleton<ITagTaxonomyRepository>(sp =>
		{
			var db = sp.GetRequiredService<TopicDb>();
			return new SqliteTagTaxonomyRepository(db.Connection);
		});
		builder.Services.AddSingleton<IPromptTemplateRepository>(sp =>
		{
			var db = sp.GetRequiredService<TopicDb>();
			return new SqlitePromptTemplateRepository(db.Connection);
		});

		// Register conversations repository
		builder.Services.AddSingleton<IConversationsRepository>(sp =>
		{
			var db = sp.GetRequiredService<TopicDb>();
			var annotationsRepository = sp.GetRequiredService<IAnnotationsRepository>();
			var tagTaxonomyRepository = sp.GetRequiredService<ITagTaxonomyRepository>();
			return new SqliteConversationsRepository(db.Connection, annotationsRepository, tagTaxonomyRepository);
		});

		// Register analytics services
		builder.Services.AddSingleton<WordCloudService>(sp =>
		{
			var conversationsRepository = sp.GetRequiredService<IConversationsRepository>();
			var annotationsRepository = sp.GetRequiredService<IAnnotationsRepository>();
			var topicTreeRepository = sp.GetRequiredService<ITopicTreeRepository>();
			var tagTaxonomyRepository = sp.GetRequiredService<ITagTaxonomyRepository>();
			return new WordCloudService(conversationsRepository, annotationsRepository, topicTreeRepository, tagTaxonomyRepository);
		});

		builder.Services.AddSingleton<TimelineService>(sp =>
		{
			var conversationsRepository = sp.GetRequiredService<IConversationsRepository>();
			var annotationsRepository = sp.GetRequiredService<IAnnotationsRepository>();
			var topicTreeRepository = sp.GetRequiredService<ITopicTreeRepository>();
			var tagTaxonomyRepository = sp.GetRequiredService<ITagTaxonomyRepository>();
			return new TimelineService(conversationsRepository, annotationsRepository, topicTreeRepository, tagTaxonomyRepository);
		});

		builder.Services.AddSingleton<IAnalyticsService>(sp =>
		{
			var wordCloudService = sp.GetRequiredService<WordCloudService>();
			var timelineService = sp.GetRequiredService<TimelineService>();
			return new AnalyticsService(wordCloudService, timelineService);
		});

		// Register export service
		builder.Services.AddSingleton<AnalyticsExportService>();

		// Register ViewModels
		builder.Services.AddTransient<TopicTreeViewModel>(sp =>
		{
			var repository = sp.GetRequiredService<ITopicTreeRepository>();
			var llmFactory = sp.GetService<Services.LLM.LLMClientFactory>();
			var telemetry = sp.GetService<TelemetryService>();
			return new TopicTreeViewModel(repository, llmFactory, telemetry);
		});
		
		builder.Services.AddTransient<SettingsViewModel>(sp =>
		{
			var settingsService = sp.GetRequiredService<Services.SettingsService>();
			return new SettingsViewModel(settingsService);
		});
		
		builder.Services.AddTransient<NotificationsViewModel>(sp =>
		{
			var repository = sp.GetRequiredService<INotificationsRepository>();
			var notificationService = sp.GetService<INotificationService>();
			return new NotificationsViewModel(repository, notificationService);
		});
		
		builder.Services.AddTransient<TagTaxonomyViewModel>(sp =>
		{
			var repository = sp.GetRequiredService<ITagTaxonomyRepository>();
			var annotationsRepository = sp.GetService<IAnnotationsRepository>();
			return new TagTaxonomyViewModel(repository, annotationsRepository);
		});

		builder.Services.AddTransient<AdvancedSearchViewModel>(sp =>
		{
			var searchCoordinator = sp.GetRequiredService<IdeaBranch.App.Services.Search.SearchCoordinator>();
			return new AdvancedSearchViewModel(searchCoordinator);
		});

		builder.Services.AddTransient<SimpleSearchViewModel>(sp =>
		{
			var searchCoordinator = sp.GetRequiredService<IdeaBranch.App.Services.Search.SearchCoordinator>();
			return new SimpleSearchViewModel(searchCoordinator);
		});

		// Register analytics ViewModels
		builder.Services.AddTransient<WordCloudViewModel>(sp =>
		{
			var analyticsService = sp.GetRequiredService<IAnalyticsService>();
			var exportService = sp.GetRequiredService<AnalyticsExportService>();
			var tagTaxonomyRepository = sp.GetRequiredService<ITagTaxonomyRepository>();
			return new WordCloudViewModel(analyticsService, exportService, tagTaxonomyRepository);
		});

		builder.Services.AddTransient<ViewModels.Analytics.TimelineViewModel>(sp =>
		{
			var analyticsService = sp.GetRequiredService<IAnalyticsService>();
			var exportService = sp.GetRequiredService<AnalyticsExportService>();
			var tagTaxonomyRepository = sp.GetRequiredService<ITagTaxonomyRepository>();
			return new ViewModels.Analytics.TimelineViewModel(analyticsService, exportService, tagTaxonomyRepository);
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

		// Register search coordinator
		builder.Services.AddSingleton<IdeaBranch.App.Services.Search.SearchCoordinator>(sp =>
		{
			var annotationsRepository = sp.GetRequiredService<IAnnotationsRepository>();
			var tagTaxonomyRepository = sp.GetRequiredService<ITagTaxonomyRepository>();
			var promptTemplateRepository = sp.GetRequiredService<IPromptTemplateRepository>();
			var topicTreeRepository = sp.GetRequiredService<ITopicTreeRepository>();
			return new IdeaBranch.App.Services.Search.SearchCoordinator(
				annotationsRepository,
				tagTaxonomyRepository,
				promptTemplateRepository,
				topicTreeRepository);
		});

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

		// Register notification services
		// Note: INotificationsRepository will be registered after we create SqliteNotificationsRepository
		builder.Services.AddSingleton<INotificationService>(sp =>
		{
			var repository = sp.GetRequiredService<INotificationsRepository>();
			var settingsService = sp.GetRequiredService<Services.SettingsService>();
			return new NotificationService(repository, settingsService);
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

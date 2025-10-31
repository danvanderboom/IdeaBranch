using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace IdeaBranch.App;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
		// Initialize culture synchronously at app startup
		InitializeCulture();
	}

	private static void InitializeCulture()
	{
		try
		{
			// Read language preference directly from SecureStorage
			var languagePreference = Task.Run(async () => await SecureStorage.GetAsync("app_language")).GetAwaiter().GetResult();
			
			CultureInfo culture;
			if (languagePreference == "system" || string.IsNullOrEmpty(languagePreference))
			{
				// Use device locale
				culture = CultureInfo.CurrentCulture;
			}
			else
			{
				// Map language code to culture
				culture = languagePreference.ToLowerInvariant() switch
				{
					"en" => new CultureInfo("en-US"),
					"es" => new CultureInfo("es-ES"),
					"fr" => new CultureInfo("fr-FR"),
					_ => new CultureInfo("en-US") // Default fallback
				};
			}
			
			// Set culture for current thread
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
			
			// Set default culture for all threads
			CultureInfo.DefaultThreadCurrentCulture = culture;
			CultureInfo.DefaultThreadCurrentUICulture = culture;
		}
		catch
		{
			// If initialization fails, fall back to device culture
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
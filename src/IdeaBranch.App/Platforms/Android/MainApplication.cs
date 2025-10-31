using Android.App;
using Android.Runtime;

namespace IdeaBranch.App;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp()
	{
		var app = MauiProgram.CreateMauiApp();
		
		// Create notification channel for Android 8.0+
		Platforms.Android.AndroidNotificationService.CreateNotificationChannel();
		
		return app;
	}
}

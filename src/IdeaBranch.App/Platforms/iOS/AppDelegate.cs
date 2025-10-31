using Foundation;
using UIKit;
using UserNotifications;
using IdeaBranch.App.Services.Notifications;

namespace IdeaBranch.App;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate
{
	protected override MauiApp CreateMauiApp()
	{
		var app = MauiProgram.CreateMauiApp();
		
		// Set notification delegate
		UNUserNotificationCenter.Current.Delegate = this;
		
		return app;
	}

	public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
	{
		// Convert device token to string
		var tokenString = ConvertDeviceTokenToString(deviceToken);
		
		// Store token locally for later backend registration
		// Note: This will be handled by NotificationService.RegisterForPushNotificationsAsync
		System.Diagnostics.Debug.WriteLine($"Device token: {tokenString}");
		
		// Store in SecureStorage for later use
		_ = Task.Run(async () =>
		{
			await Microsoft.Maui.Storage.SecureStorage.SetAsync("notification_device_token", tokenString);
		});
	}

	public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
	{
		System.Diagnostics.Debug.WriteLine($"Failed to register for remote notifications: {error?.LocalizedDescription}");
	}

	public void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
	{
		// Handle received push notification
		System.Diagnostics.Debug.WriteLine($"Received remote notification: {userInfo}");
		completionHandler(UIBackgroundFetchResult.NewData);
	}

	private static string ConvertDeviceTokenToString(NSData deviceToken)
	{
		var bytes = new byte[deviceToken.Length];
		System.Runtime.InteropServices.Marshal.Copy(deviceToken.Bytes, bytes, 0, Convert.ToInt32(deviceToken.Length));
		return BitConverter.ToString(bytes).Replace("-", "");
	}

	// UNUserNotificationCenterDelegate implementation
	public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
	{
		// Show notification even when app is in foreground
		completionHandler(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound);
	}

	public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
	{
		// Handle notification tap
		System.Diagnostics.Debug.WriteLine($"Notification tapped: {response.Notification.Request.Content.Title}");
		completionHandler();
	}
}

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using IdeaBranch.App.Services.Notifications;
using Microsoft.Maui.ApplicationModel;

namespace IdeaBranch.App.Platforms.Android;

/// <summary>
/// Android-specific notification service implementation.
/// </summary>
public class AndroidNotificationService
{
    private const string NotificationChannelId = "ideabranch_notifications";
    private const string NotificationChannelName = "IdeaBranch Notifications";
    private const string NotificationChannelDescription = "Notifications for content updates and deadlines";

    /// <summary>
    /// Creates the notification channel for Android 8.0+.
    /// </summary>
    public static void CreateNotificationChannel()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var channel = new NotificationChannel(
                NotificationChannelId,
                NotificationChannelName,
                NotificationImportance.Default)
            {
                Description = NotificationChannelDescription
            };

            var notificationManager = Platform.CurrentActivity?.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.CreateNotificationChannel(channel);
        }
    }

    /// <summary>
    /// Requests notification permission for Android 13+.
    /// </summary>
    public static async Task<bool> RequestNotificationPermissionAsync()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var activity = Platform.CurrentActivity;
            if (activity == null)
                return false;

            var permission = Manifest.Permission.PostNotifications;
            var status = ContextCompat.CheckSelfPermission(activity, permission);
            
            if (status == Permission.Granted)
                return true;

            // Request permission
            await Task.Run(async () =>
            {
                ActivityCompat.RequestPermissions(activity, new[] { permission }, 0);
                // Wait a bit for the permission dialog
                await Task.Delay(500);
            });

            // Check again after request
            status = ContextCompat.CheckSelfPermission(activity, permission);
            return status == Permission.Granted;
        }
        
        // Android 12 and below don't require runtime permission
        return true;
    }

    /// <summary>
    /// Gets the notification permission status for Android 13+.
    /// </summary>
    public static async Task<PushPermissionStatus> GetNotificationPermissionStatusAsync()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var activity = Platform.CurrentActivity;
            if (activity == null)
                return PushPermissionStatus.Unknown;

            var permission = Manifest.Permission.PostNotifications;
            var status = ContextCompat.CheckSelfPermission(activity, permission);
            
            return status switch
            {
                Permission.Granted => PushPermissionStatus.Granted,
                Permission.Denied => PushPermissionStatus.Denied,
                _ => PushPermissionStatus.Unknown
            };
        }
        
        // Android 12 and below have permission by default
        await Task.CompletedTask;
        return PushPermissionStatus.Granted;
    }
}


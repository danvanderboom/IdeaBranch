using Windows.ApplicationModel;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;
using IdeaBranch.App.Services.Notifications;

namespace IdeaBranch.App.Platforms.Windows;

/// <summary>
/// Windows-specific notification service implementation.
/// </summary>
public class WindowsNotificationService
{
    /// <summary>
    /// Requests notification permission and registers for push notifications.
    /// </summary>
    public static async Task<bool> RequestNotificationPermissionAsync()
    {
        try
        {
            // Windows 10+ requires user consent for notifications
            // Check if notifications are enabled in system settings
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            
            if (channel == null)
                return false;

            // Store channel URI for later backend registration
            await Microsoft.Maui.Storage.SecureStorage.SetAsync("notification_device_token", channel.Uri);
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Windows notification permission error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the current notification permission status.
    /// </summary>
    public static async Task<PushPermissionStatus> GetNotificationPermissionStatusAsync()
    {
        try
        {
            // Check if we can create a notification channel
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            
            if (channel != null)
                return PushPermissionStatus.Granted;
            
            return PushPermissionStatus.Unknown;
        }
        catch
        {
            return PushPermissionStatus.Denied;
        }
    }

    /// <summary>
    /// Registers for Windows push notifications.
    /// </summary>
    public static async Task<string?> RegisterForPushNotificationsAsync()
    {
        try
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            
            if (channel == null)
                return null;

            // Store channel URI for later backend registration
            await Microsoft.Maui.Storage.SecureStorage.SetAsync("notification_device_token", channel.Uri);
            
            return channel.Uri;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Windows push notification registration error: {ex.Message}");
            return null;
        }
    }
}


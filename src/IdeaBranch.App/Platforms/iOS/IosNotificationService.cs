using UserNotifications;
using IdeaBranch.App.Services.Notifications;

namespace IdeaBranch.App.Platforms.iOS;

/// <summary>
/// iOS-specific notification service implementation.
/// </summary>
public class IosNotificationService
{
    /// <summary>
    /// Requests notification authorization from the user.
    /// </summary>
    public static async Task<bool> RequestNotificationAuthorizationAsync()
    {
        var center = UNUserNotificationCenter.Current;
        var (granted, error) = await center.RequestAuthorizationAsync(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound);

        if (error != null)
        {
            System.Diagnostics.Debug.WriteLine($"iOS notification authorization error: {error}");
            return false;
        }

        return granted;
    }

    /// <summary>
    /// Gets the current notification authorization status.
    /// </summary>
    public static async Task<PushPermissionStatus> GetNotificationAuthorizationStatusAsync()
    {
        var center = UNUserNotificationCenter.Current;
        var settings = await center.GetNotificationSettingsAsync();

        return settings.AuthorizationStatus switch
        {
            UNAuthorizationStatus.Authorized => PushPermissionStatus.Granted,
            UNAuthorizationStatus.Denied => PushPermissionStatus.Denied,
            UNAuthorizationStatus.NotDetermined => PushPermissionStatus.Unknown,
            _ => PushPermissionStatus.Unknown
        };
    }

    /// <summary>
    /// Registers for remote notifications and returns the device token.
    /// Note: The actual token will be received in AppDelegate's RegisteredForRemoteNotifications method.
    /// </summary>
    public static void RegisterForRemoteNotifications()
    {
        UIApplication.SharedApplication.RegisterForRemoteNotifications();
    }
}


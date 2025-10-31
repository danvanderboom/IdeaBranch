using CommunityToolkit.Maui.Alerts;
using IdeaBranch.App.Services;
using IdeaBranch.Domain;
using Microsoft.Maui.ApplicationModel;

namespace IdeaBranch.App.Services.Notifications;

/// <summary>
/// Cross-platform notification service implementation.
/// Handles both in-app notifications and push notification registration.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationsRepository _repository;
    private readonly SettingsService _settingsService;
    private const string DeviceTokenKey = "notification_device_token";

    /// <summary>
    /// Initializes a new instance of the NotificationService class.
    /// </summary>
    public NotificationService(INotificationsRepository repository, SettingsService settingsService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    /// <summary>
    /// Event fired when a new notification is added.
    /// </summary>
    public event EventHandler<NotificationItem>? NotificationAdded;

    /// <summary>
    /// Shows an in-app notification (snackbar/toast).
    /// </summary>
    public async Task ShowInAppNotificationAsync(string title, string message, string type = "general", CancellationToken cancellationToken = default)
    {
        // Check if in-app notifications are enabled
        var inAppEnabled = await _settingsService.GetInAppNotificationsEnabledAsync();
        if (!inAppEnabled)
            return;

        // Create and save notification
        var notification = new NotificationItem(title, message, type);
        await _repository.SaveAsync(notification, cancellationToken);

        // Show snackbar
        var snackbar = Snackbar.Make(message, duration: TimeSpan.FromSeconds(3));
        await snackbar.Show(cancellationToken);

        // Fire event
        NotificationAdded?.Invoke(this, notification);
    }

    /// <summary>
    /// Requests push notification permission from the OS.
    /// </summary>
    public async Task<bool> RequestPushPermissionAsync()
    {
#if ANDROID
        return await Platforms.Android.AndroidNotificationService.RequestNotificationPermissionAsync();
#elif IOS
        return await Platforms.iOS.IosNotificationService.RequestNotificationAuthorizationAsync();
#elif WINDOWS
        return await Platforms.Windows.WindowsNotificationService.RequestNotificationPermissionAsync();
#else
        await Task.CompletedTask;
        return false;
#endif
    }

    /// <summary>
    /// Gets the current push notification permission status.
    /// </summary>
    public async Task<PushPermissionStatus> GetPushPermissionStatusAsync()
    {
#if ANDROID
        return await Platforms.Android.AndroidNotificationService.GetNotificationPermissionStatusAsync();
#elif IOS
        return await Platforms.iOS.IosNotificationService.GetNotificationAuthorizationStatusAsync();
#elif WINDOWS
        return await Platforms.Windows.WindowsNotificationService.GetNotificationPermissionStatusAsync();
#else
        await Task.CompletedTask;
        return PushPermissionStatus.Unknown;
#endif
    }

    /// <summary>
    /// Registers the device for push notifications (stores token locally for later backend registration).
    /// </summary>
    public async Task<string?> RegisterForPushNotificationsAsync(CancellationToken cancellationToken = default)
    {
        // Check if push notifications are enabled in settings
        var pushEnabled = await _settingsService.GetPushNotificationsEnabledAsync();
        if (!pushEnabled)
            return null;

        // Request OS permission
        var permissionGranted = await RequestPushPermissionAsync();
        if (!permissionGranted)
            return null;

#if IOS
        // Register for remote notifications
        Platforms.iOS.IosNotificationService.RegisterForRemoteNotifications();
        
        // Wait a bit for token registration
        await Task.Delay(1000, cancellationToken);
        
        // Retrieve stored token
        return await GetDeviceTokenAsync();
#elif ANDROID
        // Android FCM token registration would go here when backend is ready
        // For now, return null as a stub
        await Task.CompletedTask;
        return null;
#elif WINDOWS
        return await Platforms.Windows.WindowsNotificationService.RegisterForPushNotificationsAsync();
#else
        await Task.CompletedTask;
        return null;
#endif
    }

    /// <summary>
    /// Gets the stored device token for push notifications.
    /// </summary>
    public async Task<string?> GetDeviceTokenAsync()
    {
        return await SecureStorage.GetAsync(DeviceTokenKey);
    }

    /// <summary>
    /// Clears the stored device token.
    /// </summary>
    public async Task ClearDeviceTokenAsync()
    {
        SecureStorage.Remove(DeviceTokenKey);
        await Task.CompletedTask;
    }
}


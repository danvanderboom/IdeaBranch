using IdeaBranch.Domain;

namespace IdeaBranch.App.Services.Notifications;

/// <summary>
/// Cross-platform notification service interface.
/// Handles both in-app notifications and push notification registration.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Event fired when a new notification is added.
    /// </summary>
    event EventHandler<NotificationItem>? NotificationAdded;

    /// <summary>
    /// Shows an in-app notification (snackbar/toast).
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="type">The notification type (e.g., "update", "deadline", "task").</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the operation.</returns>
    Task ShowInAppNotificationAsync(string title, string message, string type = "general", CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests push notification permission from the OS.
    /// </summary>
    /// <returns>True if permission was granted, false otherwise.</returns>
    Task<bool> RequestPushPermissionAsync();

    /// <summary>
    /// Gets the current push notification permission status.
    /// </summary>
    /// <returns>The permission status.</returns>
    Task<PushPermissionStatus> GetPushPermissionStatusAsync();

    /// <summary>
    /// Registers the device for push notifications (stores token locally for later backend registration).
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The device token, or null if registration failed.</returns>
    Task<string?> RegisterForPushNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the stored device token for push notifications.
    /// </summary>
    /// <returns>The device token, or null if not registered.</returns>
    Task<string?> GetDeviceTokenAsync();

    /// <summary>
    /// Clears the stored device token.
    /// </summary>
    Task ClearDeviceTokenAsync();
}

/// <summary>
/// Represents the push notification permission status.
/// </summary>
public enum PushPermissionStatus
{
    /// <summary>
    /// Permission status is unknown or not yet requested.
    /// </summary>
    Unknown,

    /// <summary>
    /// Permission has been granted.
    /// </summary>
    Granted,

    /// <summary>
    /// Permission has been denied.
    /// </summary>
    Denied
}


namespace IdeaBranch.Domain;

/// <summary>
/// Repository interface for persisting and loading notifications.
/// </summary>
public interface INotificationsRepository
{
    /// <summary>
    /// Gets all notifications, optionally filtered by read status.
    /// </summary>
    /// <param name="includeRead">Whether to include read notifications.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of notifications, ordered by creation date (newest first).</returns>
    Task<IReadOnlyList<NotificationItem>> GetAllAsync(bool includeRead = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a notification by its ID.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The notification, or null if not found.</returns>
    Task<NotificationItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a notification item.
    /// </summary>
    /// <param name="notification">The notification to save.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the save operation.</returns>
    Task SaveAsync(NotificationItem notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read or unread.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <param name="isRead">True to mark as read, false to mark as unread.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the notification was found and updated; false otherwise.</returns>
    Task<bool> MarkReadAsync(Guid id, bool isRead, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the notification was found and deleted; false otherwise.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all notifications.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of notifications deleted.</returns>
    Task<int> DeleteAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unread notifications.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of unread notifications.</returns>
    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);
}


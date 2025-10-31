namespace IdeaBranch.Domain;

/// <summary>
/// Represents a notification item in the application.
/// </summary>
public class NotificationItem
{
    /// <summary>
    /// Gets the unique identifier for this notification.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets or sets the title of the notification.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message content of the notification.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type/category of the notification (e.g., "update", "deadline", "task").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp when this notification was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets or sets whether this notification has been read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Initializes a new instance of the NotificationItem class.
    /// </summary>
    public NotificationItem(string title, string message, string type = "general")
    {
        Id = Guid.NewGuid();
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
    }

    /// <summary>
    /// Initializes a new instance with an existing ID (for loading from storage).
    /// </summary>
    public NotificationItem(Guid id, string title, string message, DateTime createdAt, string type = "general", bool isRead = false)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        CreatedAt = createdAt;
        IsRead = isRead;
    }
}


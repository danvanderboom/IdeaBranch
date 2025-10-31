using System.Collections.ObjectModel;
using System.ComponentModel;
using IdeaBranch.App.Services.Notifications;
using IdeaBranch.Domain;

namespace IdeaBranch.App.ViewModels;

/// <summary>
/// ViewModel for NotificationsPage that manages the notifications feed.
/// </summary>
public class NotificationsViewModel : INotifyPropertyChanged
{
    private readonly INotificationsRepository _repository;
    private readonly INotificationService? _notificationService;
    private ObservableCollection<NotificationItem> _notifications = new();
    private bool _isLoading;
    private bool _hasUnreadNotifications;

    /// <summary>
    /// Initializes a new instance with the notifications repository.
    /// </summary>
    public NotificationsViewModel(INotificationsRepository repository, INotificationService? notificationService = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _notificationService = notificationService;
        LoadNotificationsAsync();
    }

    /// <summary>
    /// Gets the list of notifications.
    /// </summary>
    public ObservableCollection<NotificationItem> Notifications
    {
        get => _notifications;
        private set
        {
            if (_notifications != value)
            {
                _notifications = value;
                OnPropertyChanged(nameof(Notifications));
            }
        }
    }

    /// <summary>
    /// Gets whether notifications are currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    /// <summary>
    /// Gets whether there are unread notifications.
    /// </summary>
    public bool HasUnreadNotifications
    {
        get => _hasUnreadNotifications;
        private set
        {
            if (_hasUnreadNotifications != value)
            {
                _hasUnreadNotifications = value;
                OnPropertyChanged(nameof(HasUnreadNotifications));
            }
        }
    }

    /// <summary>
    /// Loads notifications from the repository.
    /// </summary>
    public async void LoadNotificationsAsync()
    {
        IsLoading = true;
        try
        {
            var notifications = await _repository.GetAllAsync(includeRead: true);
            Notifications = new ObservableCollection<NotificationItem>(notifications);
            
            var unreadCount = await _repository.GetUnreadCountAsync();
            HasUnreadNotifications = unreadCount > 0;
        }
        catch
        {
            // Error handling - keep existing notifications
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Marks a notification as read or unread.
    /// </summary>
    public async void ToggleReadStatusAsync(NotificationItem notification)
    {
        if (notification == null)
            return;

        try
        {
            var newStatus = !notification.IsRead;
            var success = await _repository.MarkReadAsync(notification.Id, newStatus);
            if (success)
            {
                notification.IsRead = newStatus;
                OnPropertyChanged(nameof(Notifications));
                
                var unreadCount = await _repository.GetUnreadCountAsync();
                HasUnreadNotifications = unreadCount > 0;
            }
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    public async void DeleteNotificationAsync(NotificationItem notification)
    {
        if (notification == null)
            return;

        try
        {
            var success = await _repository.DeleteAsync(notification.Id);
            if (success)
            {
                Notifications.Remove(notification);
                
                var unreadCount = await _repository.GetUnreadCountAsync();
                HasUnreadNotifications = unreadCount > 0;
            }
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Deletes all notifications.
    /// </summary>
    public async void ClearAllNotificationsAsync()
    {
        try
        {
            await _repository.DeleteAllAsync();
            Notifications.Clear();
            HasUnreadNotifications = false;
        }
        catch
        {
            // Error handling
        }
    }

    /// <summary>
    /// Refreshes the notifications list.
    /// </summary>
    public void Refresh()
    {
        LoadNotificationsAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


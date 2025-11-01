using IdeaBranch.App.ViewModels;
using IdeaBranch.Domain;

namespace IdeaBranch.App.Views;

public partial class NotificationsPage : ContentPage
{
    private NotificationsViewModel? ViewModel => BindingContext as NotificationsViewModel;

    public NotificationsPage(NotificationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public NotificationsPage() : this(
        new NotificationsViewModel(
            (Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services)!
                .GetRequiredService<IdeaBranch.Domain.INotificationsRepository>(),
            Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services!
                .GetService<IdeaBranch.App.Services.Notifications.INotificationService>()))
    {
    }

    private void OnToggleReadClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is NotificationItem notification)
        {
            ViewModel?.ToggleReadStatusAsync(notification);
        }
    }

    private void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is NotificationItem notification)
        {
            ViewModel?.DeleteNotificationAsync(notification);
        }
    }

    private void OnClearAllClicked(object? sender, EventArgs e)
    {
        ViewModel?.ClearAllNotificationsAsync();
    }

    private void OnRefresh(object? sender, EventArgs e)
    {
        ViewModel?.Refresh();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel?.Refresh();
    }
}


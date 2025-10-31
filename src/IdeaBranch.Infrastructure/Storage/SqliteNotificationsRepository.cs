using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// SQLite implementation of INotificationsRepository.
/// </summary>
public class SqliteNotificationsRepository : INotificationsRepository
{
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Initializes a new instance with a SQLite connection.
    /// </summary>
    public SqliteNotificationsRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NotificationItem>> GetAllAsync(bool includeRead = true, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = includeRead
                ? @"
                    SELECT Id, Title, Message, Type, CreatedAt, IsRead
                    FROM notifications
                    ORDER BY CreatedAt DESC
                "
                : @"
                    SELECT Id, Title, Message, Type, CreatedAt, IsRead
                    FROM notifications
                    WHERE IsRead = 0
                    ORDER BY CreatedAt DESC
                ";

            var notifications = new List<NotificationItem>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var notification = ReadNotification(reader);
                notifications.Add(notification);
            }

            return notifications.AsReadOnly();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<NotificationItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Message, Type, CreatedAt, IsRead
                FROM notifications
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@Id", id.ToString());

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return ReadNotification(reader);
            }

            return null;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(NotificationItem notification, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO notifications (Id, Title, Message, Type, CreatedAt, IsRead)
                VALUES (@Id, @Title, @Message, @Type, @CreatedAt, @IsRead)
                ON CONFLICT(Id) DO UPDATE SET
                    Title = @Title,
                    Message = @Message,
                    Type = @Type,
                    IsRead = @IsRead
            ";

            command.Parameters.AddWithValue("@Id", notification.Id.ToString());
            command.Parameters.AddWithValue("@Title", notification.Title);
            command.Parameters.AddWithValue("@Message", notification.Message);
            command.Parameters.AddWithValue("@Type", notification.Type);
            command.Parameters.AddWithValue("@CreatedAt", notification.CreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@IsRead", notification.IsRead ? 1 : 0);

            command.ExecuteNonQuery();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> MarkReadAsync(Guid id, bool isRead, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                UPDATE notifications
                SET IsRead = @IsRead
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@Id", id.ToString());
            command.Parameters.AddWithValue("@IsRead", isRead ? 1 : 0);

            return command.ExecuteNonQuery() > 0;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM notifications
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@Id", id.ToString());

            return command.ExecuteNonQuery() > 0;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM notifications
            ";

            return command.ExecuteNonQuery();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*)
                FROM notifications
                WHERE IsRead = 0
            ";

            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }, cancellationToken);
    }

    /// <summary>
    /// Reads a NotificationItem from a data reader.
    /// </summary>
    private static NotificationItem ReadNotification(SqliteDataReader reader)
    {
        var id = Guid.Parse(reader.GetString(0));
        var title = reader.GetString(1);
        var message = reader.GetString(2);
        var type = reader.GetString(3);
        var createdAt = DateTime.Parse(reader.GetString(4));
        var isRead = reader.GetInt32(5) != 0;

        return new NotificationItem(id, title, message, createdAt, type, isRead);
    }
}


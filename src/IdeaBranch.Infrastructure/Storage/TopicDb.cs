using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.Infrastructure.Storage;

/// <summary>
/// Manages SQLite database connection lifecycle and schema migrations.
/// </summary>
public class TopicDb : IDisposable
{
    private readonly SqliteConnection _connection;
    private bool _disposed;
    private const int CurrentSchemaVersion = 3;
    private const string SchemaInfoKey = "schema_version";

    /// <summary>
    /// Initializes a new instance with a database connection string.
    /// </summary>
    /// <param name="connectionString">The connection string for the SQLite database (e.g., "Data Source=path/to/file.db").</param>
    public TopicDb(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        _connection = new SqliteConnection(connectionString);
        _connection.Open();
        EnsureMigrations();
    }

    /// <summary>
    /// Gets the SQLite connection.
    /// </summary>
    public SqliteConnection Connection
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TopicDb));
            return _connection;
        }
    }

    /// <summary>
    /// Ensures all migrations are applied up to the current schema version.
    /// </summary>
    private void EnsureMigrations()
    {
        EnsureSchemaInfoTable();
        var currentVersion = GetSchemaVersion();
        
        if (currentVersion < CurrentSchemaVersion)
        {
            ApplyMigrations(currentVersion);
            SetSchemaVersion(CurrentSchemaVersion);
        }
    }

    /// <summary>
    /// Creates the SchemaInfo table if it doesn't exist.
    /// </summary>
    private void EnsureSchemaInfoTable()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS SchemaInfo (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Gets the current schema version from the database.
    /// </summary>
    private int GetSchemaVersion()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT Value FROM SchemaInfo WHERE Key = @Key";
        command.Parameters.AddWithValue("@Key", SchemaInfoKey);
        
        var result = command.ExecuteScalar();
        if (result == null)
            return 0;
        
        return int.TryParse(result.ToString(), out var version) ? version : 0;
    }

    /// <summary>
    /// Sets the schema version in the database.
    /// </summary>
    private void SetSchemaVersion(int version)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO SchemaInfo (Key, Value)
            VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value;
        ";
        command.Parameters.AddWithValue("@Key", SchemaInfoKey);
        command.Parameters.AddWithValue("@Value", version.ToString());
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Applies migrations from the current version to the target version.
    /// </summary>
    private void ApplyMigrations(int fromVersion)
    {
        // Migration v1: Create topic_nodes table
        if (fromVersion < 1)
        {
            ApplyMigrationV1();
        }

        // Migration v2: Create version_history table
        if (fromVersion < 2)
        {
            ApplyMigrationV2();
        }

        // Migration v3: Create notifications table
        if (fromVersion < 3)
        {
            ApplyMigrationV3();
        }
    }

    /// <summary>
    /// Applies migration v1: Creates the topic_nodes table.
    /// </summary>
    private void ApplyMigrationV1()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS topic_nodes (
                NodeId TEXT PRIMARY KEY,
                ParentId TEXT,
                Title TEXT,
                Prompt TEXT NOT NULL DEFAULT '',
                Response TEXT NOT NULL DEFAULT '',
                Ordinal INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (ParentId) REFERENCES topic_nodes(NodeId) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS idx_topic_nodes_parent_ordinal 
                ON topic_nodes(ParentId, Ordinal);

            CREATE INDEX IF NOT EXISTS idx_topic_nodes_parent 
                ON topic_nodes(ParentId);
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Applies migration v2: Creates the version_history table.
    /// </summary>
    private void ApplyMigrationV2()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS version_history (
                VersionId TEXT PRIMARY KEY,
                NodeId TEXT NOT NULL,
                Title TEXT,
                Prompt TEXT NOT NULL DEFAULT '',
                Response TEXT NOT NULL DEFAULT '',
                Ordinal INTEGER NOT NULL DEFAULT 0,
                VersionTimestamp TEXT NOT NULL,
                AuthorId TEXT,
                AuthorName TEXT,
                FOREIGN KEY (NodeId) REFERENCES topic_nodes(NodeId) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS idx_version_history_node_id 
                ON version_history(NodeId);

            CREATE INDEX IF NOT EXISTS idx_version_history_node_timestamp 
                ON version_history(NodeId, VersionTimestamp DESC);
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Applies migration v3: Creates the notifications table.
    /// </summary>
    private void ApplyMigrationV3()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS notifications (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Message TEXT NOT NULL,
                Type TEXT NOT NULL DEFAULT 'general',
                CreatedAt TEXT NOT NULL,
                IsRead INTEGER NOT NULL DEFAULT 0
            );

            CREATE INDEX IF NOT EXISTS idx_notifications_created_at 
                ON notifications(CreatedAt DESC);

            CREATE INDEX IF NOT EXISTS idx_notifications_is_read 
                ON notifications(IsRead);
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Disposes the database connection.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connection?.Close();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}


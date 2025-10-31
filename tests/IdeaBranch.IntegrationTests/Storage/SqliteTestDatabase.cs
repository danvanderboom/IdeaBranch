using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.IntegrationTests.Storage;

/// <summary>
/// Helper for creating file-based SQLite test databases with schema setup.
/// Provides temporary file management and connection lifecycle.
/// </summary>
public class SqliteTestDatabase : IDisposable
{
    private readonly string _dbPath;
    private SqliteConnection? _connection;

    /// <summary>
    /// Creates a new file-based SQLite database in the system temp directory.
    /// </summary>
    public SqliteTestDatabase()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"ideabranch_test_{Guid.NewGuid():N}.db");
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        _connection.Open();
    }

    /// <summary>
    /// Gets the connection to the test database.
    /// </summary>
    public SqliteConnection Connection
    {
        get
        {
            if (_connection == null)
                throw new ObjectDisposedException(nameof(SqliteTestDatabase));
            return _connection;
        }
    }

    /// <summary>
    /// Gets the database file path.
    /// </summary>
    public string DbPath => _dbPath;

    /// <summary>
    /// Creates the topic_nodes table schema with indexes and foreign keys.
    /// </summary>
    public void CreateSchema()
    {
        using var command = Connection.CreateCommand();
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
    /// Closes and reopens the connection (simulates app restart).
    /// </summary>
    public void ReopenConnection()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        _connection.Open();
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;

        // Clean up temp file
        if (File.Exists(_dbPath))
        {
            try
            {
                File.Delete(_dbPath);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}


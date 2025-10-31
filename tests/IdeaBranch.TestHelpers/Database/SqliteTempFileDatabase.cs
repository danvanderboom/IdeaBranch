using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.TestHelpers.Database;

/// <summary>
/// Helper for creating per-test temp-file SQLite databases with automatic cleanup.
/// Use this for parallel test execution where each test needs its own database file.
/// </summary>
public class SqliteTempFileDatabase : IDisposable
{
    private readonly string _dbPath;
    private SqliteConnection? _connection;
    private bool _disposed;

    /// <summary>
    /// Creates a new temp-file SQLite database with a unique filename.
    /// </summary>
    public SqliteTempFileDatabase()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.db");
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        _connection.Open();
    }

    /// <summary>
    /// Creates a new temp-file SQLite database with a custom filename prefix.
    /// </summary>
    /// <param name="prefix">The prefix for the temp filename.</param>
    public SqliteTempFileDatabase(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new ArgumentException("Prefix cannot be null or whitespace.", nameof(prefix));

        _dbPath = Path.Combine(Path.GetTempPath(), $"{prefix}_{Guid.NewGuid():N}.db");
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
                throw new ObjectDisposedException(nameof(SqliteTempFileDatabase));
            return _connection;
        }
    }

    /// <summary>
    /// Gets the database file path.
    /// </summary>
    public string DbPath => _dbPath;

    /// <summary>
    /// Reopens the connection. Useful for simulating app restart scenarios.
    /// </summary>
    public void ReopenConnection()
    {
        if (_connection == null)
            throw new ObjectDisposedException(nameof(SqliteTempFileDatabase));

        _connection.Close();
        _connection.Dispose();
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        _connection.Open();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
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
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}


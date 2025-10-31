using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace IdeaBranch.TestHelpers.Database;

/// <summary>
/// Helper for creating in-memory SQLite databases per test fixture.
/// Manages a single shared connection with transaction-per-test isolation.
/// </summary>
public abstract class SqliteInMemoryDatabase : IDisposable
{
    private static SqliteConnection? _sharedConnection;
    private static readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Gets the shared in-memory SQLite connection.
    /// </summary>
    public static SqliteConnection SharedConnection
    {
        get
        {
            if (_sharedConnection == null)
            {
                lock (_lock)
                {
                    if (_sharedConnection == null)
                    {
                        _sharedConnection = new SqliteConnection("Data Source=:memory:");
                        _sharedConnection.Open();
                    }
                }
            }
            return _sharedConnection;
        }
    }

    /// <summary>
    /// Gets or sets the current transaction for the test.
    /// </summary>
    public DbTransaction? CurrentTransaction { get; set; }

    /// <summary>
    /// Initializes the database schema. Override this to create your schema.
    /// </summary>
    protected abstract void CreateSchema();

    /// <summary>
    /// Sets up the database for the test. Call this from [SetUp] or [OneTimeSetUp].
    /// </summary>
    public void SetUpDatabase()
    {
        CreateSchema();
    }

    /// <summary>
    /// Begins a transaction for the test. Call this from [SetUp].
    /// </summary>
    public void BeginTransaction()
    {
        CurrentTransaction = SharedConnection.BeginTransaction();
    }

    /// <summary>
    /// Rolls back the transaction and cleans up. Call this from [TearDown].
    /// </summary>
    public void TearDownTransaction()
    {
        CurrentTransaction?.Rollback();
        CurrentTransaction?.Dispose();
        CurrentTransaction = null;
    }

    /// <summary>
    /// Disposes the shared connection. Call this from [OneTimeTearDown].
    /// </summary>
    public static void DisposeSharedConnection()
    {
        if (_sharedConnection != null)
        {
            lock (_lock)
            {
                if (_sharedConnection != null)
                {
                    _sharedConnection.Dispose();
                    _sharedConnection = null;
                }
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                TearDownTransaction();
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


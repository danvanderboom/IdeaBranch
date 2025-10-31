using System.Data.Common;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace IdeaBranch.TestHelpers.Database;

/// <summary>
/// Base class for tests that require database isolation via transactions.
/// Uses a shared in-memory SQLite connection per fixture with transaction-per-test rollback.
/// </summary>
/// <typeparam name="T">The database test fixture type that manages the connection.</typeparam>
public abstract class DbTestBase<T> where T : SqliteInMemoryDatabase, new()
{
    private static T? _databaseFixture;
    private static readonly object _fixtureLock = new();

    /// <summary>
    /// Gets the database fixture instance.
    /// </summary>
    protected static T DatabaseFixture
    {
        get
        {
            if (_databaseFixture == null)
            {
                lock (_fixtureLock)
                {
                    if (_databaseFixture == null)
                    {
                        _databaseFixture = new T();
                        _databaseFixture.SetUpDatabase();
                    }
                }
            }
            return _databaseFixture;
        }
    }

    /// <summary>
    /// Gets the current transaction for this test.
    /// </summary>
    protected DbTransaction? CurrentTransaction => DatabaseFixture.CurrentTransaction;

    /// <summary>
    /// Gets the shared SQLite connection.
    /// </summary>
    protected SqliteConnection Connection => SqliteInMemoryDatabase.SharedConnection;

    /// <summary>
    /// Sets up the test with a transaction. Override to add custom setup.
    /// </summary>
    [SetUp]
    public virtual void SetUp()
    {
        DatabaseFixture.BeginTransaction();
    }

    /// <summary>
    /// Tears down the test by rolling back the transaction. Override to add custom teardown.
    /// </summary>
    [TearDown]
    public virtual void TearDown()
    {
        DatabaseFixture.TearDownTransaction();
    }

    /// <summary>
    /// Cleans up the database fixture. Call this from [OneTimeTearDown] if needed.
    /// </summary>
    [OneTimeTearDown]
    public static void OneTimeTearDown()
    {
        if (_databaseFixture != null)
        {
            lock (_fixtureLock)
            {
                if (_databaseFixture != null)
                {
                    SqliteInMemoryDatabase.DisposeSharedConnection();
                    _databaseFixture.Dispose();
                    _databaseFixture = null;
                }
            }
        }
    }
}


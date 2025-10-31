using System;
using System.Data.Common;
using FluentAssertions;
using IdeaBranch.TestHelpers.Database;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace IdeaBranch.TestHelpers.Examples;

/// <summary>
/// Example test fixture demonstrating in-memory database with transaction-per-test isolation.
/// </summary>
public class ExampleDatabaseFixture : SqliteInMemoryDatabase
{
    protected override void CreateSchema()
    {
        using var command = SharedConnection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS test_entities (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }
}

/// <summary>
/// Example test using DbTestBase with in-memory database and transaction isolation.
/// </summary>
public class DatabaseExample : DbTestBase<ExampleDatabaseFixture>
{
    [Test]
    public void InsertData_ShouldIsolatePerTest()
    {
        // Arrange
        var entityId = Guid.NewGuid().ToString();
        var entityName = "Test Entity 1";

        // Act
        using var command = Connection.CreateCommand();
        command.CommandText = "INSERT INTO test_entities (Id, Name, CreatedAt) VALUES (@Id, @Name, @CreatedAt)";
        command.Parameters.AddWithValue("@Id", entityId);
        command.Parameters.AddWithValue("@Name", entityName);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();

        // Assert
        using var selectCommand = Connection.CreateCommand();
        selectCommand.CommandText = "SELECT COUNT(*) FROM test_entities WHERE Id = @Id";
        selectCommand.Parameters.AddWithValue("@Id", entityId);
        var count = Convert.ToInt64(selectCommand.ExecuteScalar());
        count.Should().Be(1);
    }

    [Test]
    public void TransactionRollback_ShouldPreventDataLeakage()
    {
        // Arrange - This test runs after InsertData_ShouldIsolatePerTest
        // but should not see its data due to transaction rollback

        // Act
        using var command = Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM test_entities";
        var count = Convert.ToInt64(command.ExecuteScalar());

        // Assert - Should be 0 because previous test's transaction was rolled back
        count.Should().Be(0, "previous test data should be rolled back");
    }
}

/// <summary>
/// Example test using SqliteTempFileDatabase for parallel execution.
/// </summary>
public class TempFileDatabaseExample
{
    [Test]
    public void TempFileDatabase_ShouldCreateAndCleanup()
    {
        // Arrange
        using var testDb = new SqliteTempFileDatabase("example");
        
        // Act
        using var command = testDb.Connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS test_entities (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL
            );
            INSERT INTO test_entities (Id, Name) VALUES ('1', 'Test');
        ";
        command.ExecuteNonQuery();

        // Assert
        File.Exists(testDb.DbPath).Should().BeTrue();
        
        // Verify data
        using var selectCommand = testDb.Connection.CreateCommand();
        selectCommand.CommandText = "SELECT COUNT(*) FROM test_entities";
        var count = Convert.ToInt64(selectCommand.ExecuteScalar());
        count.Should().Be(1);
        
        // Dispose will clean up the file
    }

    [Test]
    public void TempFileDatabase_AfterDispose_ShouldDeleteFile()
    {
        // Arrange
        string dbPath;
        using (var testDb = new SqliteTempFileDatabase("example"))
        {
            dbPath = testDb.DbPath;
            File.Exists(dbPath).Should().BeTrue();
        }

        // Assert - File should be deleted after dispose
        File.Exists(dbPath).Should().BeFalse();
    }
}


# IdeaBranch.TestHelpers

Shared test infrastructure for IdeaBranch test projects, providing standardized patterns for test data generation, database isolation, and UI test cleanup.

## Overview

This project provides reusable test infrastructure:

- **Test Data Factories/Builders**: Fluent API for generating valid domain entities with sensible defaults
- **Isolated SQLite Test Databases**: In-memory and temp-file database helpers with transaction-per-test isolation
- **UI Test Cleanup**: Artifact capture on failure and storage cleanup for UI tests

## Test Data Factories/Builders

### EntityBuilder<T>

Base class for entity builders that generate valid domain entities for tests.

```csharp
public abstract class EntityBuilder<T>
{
    protected int Seed { get; }
    protected EntityBuilder(int? seed = null);
    public abstract T Build();
}
```

### TopicNodePayloadBuilder

Builder for creating `TestTopicNodePayload` instances.

```csharp
// Build with defaults
var payload = new TopicNodePayloadBuilder().Build();

// Override specific fields
var payload = new TopicNodePayloadBuilder()
    .WithTitle("Custom Topic")
    .WithPrompt("Custom prompt")
    .WithOrder(5)
    .Build();
```

### TopicTreeNodeBuilder

Builder for creating `TreeNode<TestTopicNodePayload>` instances with parent-child relationships.

```csharp
// Build simple node
var node = new TopicTreeNodeBuilder().Build();

// Build tree with children
var rootNode = new TopicTreeNodeBuilder()
    .WithNodeId(Guid.Parse("..."))
    .WithPayload(p => p
        .WithTitle("Root Topic")
        .WithPrompt("What would you like to explore?"))
    .WithChild(child => child
        .WithNodeId(Guid.Parse("..."))
        .WithPayload(p => p.WithTitle("Child Topic")))
    .Build();
```

### Usage Examples

See `Examples/TopicNodeBuilderExample.cs` for complete examples.

## Database Isolation

### In-Memory Database with Transaction-Per-Test

Use `DbTestBase<T>` for tests that need a shared in-memory database with transaction-per-test isolation.

1. Create a fixture class that extends `SqliteInMemoryDatabase`:

```csharp
public class MyDatabaseFixture : SqliteInMemoryDatabase
{
    protected override void CreateSchema()
    {
        using var command = SharedConnection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS my_table (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }
}
```

2. Inherit from `DbTestBase<T>` in your test class:

```csharp
public class MyTests : DbTestBase<MyDatabaseFixture>
{
    [Test]
    public void MyTest()
    {
        // Use Connection property - each test runs in its own transaction
        using var command = Connection.CreateCommand();
        command.CommandText = "INSERT INTO my_table (Id, Name) VALUES ('1', 'Test')";
        command.ExecuteNonQuery();
        
        // Transaction is automatically rolled back in TearDown
    }
}
```

**Benefits:**
- Single shared connection per fixture (efficient)
- Each test runs in its own transaction
- Automatic rollback in `[TearDown]` prevents data leakage
- Schema created once per fixture

### Temp-File Database Per Test

Use `SqliteTempFileDatabase` for parallel execution where each test needs its own database file.

```csharp
[Test]
public void MyParallelTest()
{
    using var testDb = new SqliteTempFileDatabase("mytest");
    
    // Use testDb.Connection for operations
    using var command = testDb.Connection.CreateCommand();
    command.CommandText = "CREATE TABLE ...";
    command.ExecuteNonQuery();
    
    // File is automatically deleted on dispose
}
```

**Benefits:**
- Each test has its own database file
- Safe for parallel execution
- Automatic file cleanup on dispose

### Usage Examples

See `Examples/DatabaseExample.cs` for complete examples.

## UI Test Cleanup

### UiTestBase

Base class for UI tests that provides artifact capture on failure and storage cleanup.

Extends `AppiumTestFixture` and adds:

- **Artifact capture on failure**: Screenshots and logs saved to `artifacts/tests/ui/{timestamp}/{TestName}/`
- **Storage cleanup**: Local and session storage cleared after each test

```csharp
public class MyUiTests : UiTestBase
{
    [Test]
    public void MyUiTest()
    {
        // Use Driver property from AppiumTestFixture
        var element = Driver.FindElement(...);
        element.Click();
        
        // On failure, screenshot and logs are automatically captured
        // Local/session storage is cleared in TearDown
    }
}
```

### Artifact Structure

On test failure, artifacts are saved to:

```
artifacts/tests/ui/{timestamp}/{TestName}/
├── screenshot.png       # Screenshot on failure
├── driver.log          # Driver/browser logs
└── test-context.txt    # Test context information
```

### CI Integration

Configure your CI pipeline to collect `artifacts/tests/**` on test failure.

## Project References

Add a project reference to `IdeaBranch.TestHelpers` in your test projects:

```xml
<ItemGroup>
  <ProjectReference Include="..\IdeaBranch.TestHelpers\IdeaBranch.TestHelpers.csproj" />
</ItemGroup>
```

## Lifecycle Notes

### Database Lifecycle (In-Memory)

1. **Fixture Setup** (`[OneTimeSetUp]`): Connection opened, schema created
2. **Test Setup** (`[SetUp]`): Transaction begun
3. **Test Execution**: Operations use the transaction
4. **Test Teardown** (`[TearDown]`): Transaction rolled back
5. **Fixture Teardown** (`[OneTimeTearDown]`): Connection disposed

### Database Lifecycle (Temp-File)

1. **Test Setup**: Temp file created, connection opened
2. **Test Execution**: Operations use the connection
3. **Test Teardown**: Connection closed, file deleted

### UI Test Lifecycle

1. **Test Setup** (`[SetUp]`): Driver initialized, app launched
2. **Test Execution**: UI operations
3. **Test Teardown** (`[TearDown]`): 
   - Artifacts captured (on failure)
   - Storage cleared
   - Driver disposed

## Best Practices

1. **Builders**: Use builders with defaults for most tests, override only what's necessary
2. **Database Isolation**: Prefer in-memory with transactions for most tests; use temp-file for parallel execution
3. **Artifacts**: Artifacts are only captured on failure; ensure CI collects `artifacts/tests/**`
4. **Seed Values**: Use explicit seed values in builders for deterministic test data when needed

## Examples

See the `Examples/` directory for complete working examples:
- `TopicNodeBuilderExample.cs` - Builder usage examples
- `DatabaseExample.cs` - Database isolation examples


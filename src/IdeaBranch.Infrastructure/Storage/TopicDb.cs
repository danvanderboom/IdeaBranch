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
    private const int CurrentSchemaVersion = 6;
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

        // Migration v4: Create tag_taxonomy_nodes table (must come before annotation_tags)
        if (fromVersion < 4)
        {
            ApplyMigrationV4();
        }

        // Migration v5: Create annotations tables
        if (fromVersion < 5)
        {
            ApplyMigrationV5();
        }

        // Migration v6: Create prompt_templates table
        if (fromVersion < 6)
        {
            ApplyMigrationV6();
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
    /// Applies migration v4: Creates the tag_taxonomy_nodes table.
    /// Must come before annotation_tags which references it.
    /// </summary>
    private void ApplyMigrationV4()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS tag_taxonomy_nodes (
                Id TEXT PRIMARY KEY,
                ParentId TEXT,
                Name TEXT NOT NULL,
                ""Order"" INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (ParentId) REFERENCES tag_taxonomy_nodes(Id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS idx_tag_taxonomy_nodes_parent_order 
                ON tag_taxonomy_nodes(ParentId, ""Order"");

            CREATE INDEX IF NOT EXISTS idx_tag_taxonomy_nodes_parent 
                ON tag_taxonomy_nodes(ParentId);

            CREATE INDEX IF NOT EXISTS idx_tag_taxonomy_nodes_name 
                ON tag_taxonomy_nodes(Name);
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Applies migration v5: Creates the annotations, annotation_tags, and annotation_values tables.
    /// </summary>
    private void ApplyMigrationV5()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS annotations (
                Id TEXT PRIMARY KEY,
                NodeId TEXT NOT NULL,
                StartOffset INTEGER NOT NULL,
                EndOffset INTEGER NOT NULL,
                Comment TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (NodeId) REFERENCES topic_nodes(NodeId) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS idx_annotations_node_id 
                ON annotations(NodeId);

            CREATE INDEX IF NOT EXISTS idx_annotations_node_span 
                ON annotations(NodeId, StartOffset, EndOffset);

            CREATE TABLE IF NOT EXISTS annotation_tags (
                AnnotationId TEXT NOT NULL,
                TagId TEXT NOT NULL,
                PRIMARY KEY (AnnotationId, TagId),
                FOREIGN KEY (AnnotationId) REFERENCES annotations(Id) ON DELETE CASCADE,
                FOREIGN KEY (TagId) REFERENCES tag_taxonomy_nodes(Id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS idx_annotation_tags_annotation_id 
                ON annotation_tags(AnnotationId);

            CREATE INDEX IF NOT EXISTS idx_annotation_tags_tag_id 
                ON annotation_tags(TagId);

            CREATE TABLE IF NOT EXISTS annotation_values (
                Id TEXT PRIMARY KEY,
                AnnotationId TEXT NOT NULL,
                ValueType TEXT NOT NULL,
                NumericValue REAL,
                GeospatialValue TEXT,
                TemporalValue TEXT,
                FOREIGN KEY (AnnotationId) REFERENCES annotations(Id) ON DELETE CASCADE,
                CHECK (ValueType IN ('numeric', 'geospatial', 'temporal'))
            );

            CREATE INDEX IF NOT EXISTS idx_annotation_values_annotation_id 
                ON annotation_values(AnnotationId);

            CREATE INDEX IF NOT EXISTS idx_annotation_values_type 
                ON annotation_values(ValueType);
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Applies migration v6: Creates the prompt_templates table.
    /// </summary>
    private void ApplyMigrationV6()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS prompt_templates (
                Id TEXT PRIMARY KEY,
                ParentId TEXT,
                Name TEXT NOT NULL,
                Body TEXT,
                ""Order"" INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (ParentId) REFERENCES prompt_templates(Id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS idx_prompt_templates_parent_order 
                ON prompt_templates(ParentId, ""Order"");

            CREATE INDEX IF NOT EXISTS idx_prompt_templates_parent 
                ON prompt_templates(ParentId);

            CREATE INDEX IF NOT EXISTS idx_prompt_templates_name 
                ON prompt_templates(Name);
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


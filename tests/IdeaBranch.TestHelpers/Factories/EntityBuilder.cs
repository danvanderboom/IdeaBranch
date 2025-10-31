namespace IdeaBranch.TestHelpers.Factories;

/// <summary>
/// Base class for entity builders that generate valid domain entities for tests.
/// Provides fluent API for overriding fields while maintaining invariants.
/// </summary>
/// <typeparam name="T">The entity type to build.</typeparam>
public abstract class EntityBuilder<T>
{
    /// <summary>
    /// Gets the seed value for deterministic generation.
    /// </summary>
    protected int Seed { get; }

    /// <summary>
    /// Initializes a new instance with an optional seed for deterministic generation.
    /// </summary>
    /// <param name="seed">The seed value. Defaults to 1.</param>
    protected EntityBuilder(int? seed = null)
    {
        Seed = seed ?? 1;
    }

    /// <summary>
    /// Builds and returns the entity instance.
    /// </summary>
    /// <returns>A valid entity instance with sensible defaults.</returns>
    public abstract T Build();
}


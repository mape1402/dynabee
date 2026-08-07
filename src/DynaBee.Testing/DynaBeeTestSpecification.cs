namespace DynaBee.Testing;

using DynaBee.FluentApi;
using DynaBee.FluentApi.Generation;

/// <summary>
/// Describes a dynamic assembly generation operation used by tests.
/// </summary>
public sealed class DynaBeeTestSpecification
{
    private readonly List<Action<IBeeAssemblyBuilder>> _configurations = new();
    private readonly Dictionary<string, string> _generatedSources = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="DynaBeeTestSpecification"/> class.
    /// </summary>
    /// <param name="assemblyName">Logical assembly name to generate.</param>
    public DynaBeeTestSpecification(string assemblyName)
    {
        AssemblyName = string.IsNullOrWhiteSpace(assemblyName)
            ? throw new ArgumentException(nameof(assemblyName))
            : assemblyName;
    }

    /// <summary>
    /// Gets the logical assembly name to generate.
    /// </summary>
    public string AssemblyName { get; }

    /// <summary>
    /// Gets configured assembly builder callbacks.
    /// </summary>
    public IReadOnlyList<Action<IBeeAssemblyBuilder>> Configurations => _configurations;

    /// <summary>
    /// Gets optional source files supplied by the consumer for snapshot writing.
    /// </summary>
    public IReadOnlyDictionary<string, string> GeneratedSources => _generatedSources;

    /// <summary>
    /// Adds an assembly builder configuration callback.
    /// </summary>
    /// <param name="configure">Configuration callback.</param>
    /// <returns>The same specification.</returns>
    public DynaBeeTestSpecification Configure(Action<IBeeAssemblyBuilder> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        _configurations.Add(configure);
        return this;
    }

    /// <summary>
    /// Applies a descriptor-driven generation plan to this specification.
    /// </summary>
    /// <param name="plan">Generation plan to apply.</param>
    /// <returns>The same specification.</returns>
    public DynaBeeTestSpecification UsePlan(DynaBeeGenerationPlan plan)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));

        if (!string.Equals(AssemblyName, plan.AssemblyName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Specification assembly '{AssemblyName}' does not match plan assembly '{plan.AssemblyName}'.");
        }

        return Configure(builder => plan.ApplyTo(builder));
    }

    /// <summary>
    /// Adds an optional generated source file for debugging or snapshots.
    /// </summary>
    /// <param name="relativePath">Relative source file path.</param>
    /// <param name="source">Source text.</param>
    /// <returns>The same specification.</returns>
    public DynaBeeTestSpecification AddGeneratedSource(string relativePath, string source)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException(nameof(relativePath));

        _generatedSources[relativePath] = source ?? throw new ArgumentNullException(nameof(source));
        return this;
    }

    /// <summary>
    /// Creates a specification from a fluent assembly configuration callback.
    /// </summary>
    /// <param name="assemblyName">Logical assembly name to generate.</param>
    /// <param name="configure">Configuration callback.</param>
    /// <returns>A test generation specification.</returns>
    public static DynaBeeTestSpecification Create(string assemblyName, Action<IBeeAssemblyBuilder> configure)
        => new DynaBeeTestSpecification(assemblyName).Configure(configure);

    /// <summary>
    /// Creates a specification from a descriptor-driven generation plan.
    /// </summary>
    /// <param name="plan">Generation plan to test.</param>
    /// <returns>A test generation specification.</returns>
    public static DynaBeeTestSpecification FromPlan(DynaBeeGenerationPlan plan)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));

        return new DynaBeeTestSpecification(plan.AssemblyName).UsePlan(plan);
    }
}


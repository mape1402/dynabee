namespace DynaBee.Testing;

using DynaBee.FluentApi.DependencyInjection;

/// <summary>
/// Generates DynaBee assemblies for tests.
/// </summary>
public interface IDynaBeeTestGenerator
{
    /// <summary>
    /// Generates an assembly in memory from a test specification.
    /// </summary>
    /// <param name="specification">Generation specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation result containing the assembly and diagnostics.</returns>
    Task<DynaBeeTestGenerationResult> GenerateAssemblyAsync(
        DynaBeeTestSpecification specification,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Default DynaBee test generator implementation.
/// </summary>
public sealed class DynaBeeTestGenerator : IDynaBeeTestGenerator
{
    private readonly IDynaBeeAssemblyBuilderFactory _builderFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynaBeeTestGenerator"/> class.
    /// </summary>
    /// <param name="builderFactory">Assembly builder factory.</param>
    public DynaBeeTestGenerator(IDynaBeeAssemblyBuilderFactory builderFactory)
    {
        _builderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
    }

    /// <inheritdoc/>
    public Task<DynaBeeTestGenerationResult> GenerateAssemblyAsync(
        DynaBeeTestSpecification specification,
        CancellationToken cancellationToken = default)
    {
        if (specification == null)
            throw new ArgumentNullException(nameof(specification));

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var builder = _builderFactory.Create(specification.AssemblyName).DisableCache();
            foreach (var configure in specification.Configurations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                configure(builder);
            }

            var assembly = builder.Build();
            return Task.FromResult(new DynaBeeTestGenerationResult(
                assembly,
                Array.Empty<DynaBeeTestDiagnostic>(),
                specification.GeneratedSources));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var diagnostics = new[]
            {
                new DynaBeeTestDiagnostic($"DynaBee test generation failed for assembly '{specification.AssemblyName}'", ex)
            };

            return Task.FromResult(new DynaBeeTestGenerationResult(null, diagnostics, specification.GeneratedSources));
        }
    }
}

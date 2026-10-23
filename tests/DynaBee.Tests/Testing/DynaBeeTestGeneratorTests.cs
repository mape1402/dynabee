namespace DynaBee.Tests.Testing;

using DynaBee.FluentApi.DependencyInjection;
using DynaBee.Testing;

public sealed class DynaBeeTestGeneratorTests
{
    [Fact]
    public void Constructor_Requires_A_Builder_Factory()
    {
        Assert.Throws<ArgumentNullException>(() => new DynaBeeTestGenerator(null!));
    }

    [Fact]
    public async Task GenerateAssemblyAsync_Requires_A_Specification()
    {
        var generator = CreateGenerator();

        await Assert.ThrowsAsync<ArgumentNullException>(() => generator.GenerateAssemblyAsync(null!));
    }

    [Fact]
    public async Task GenerateAssemblyAsync_Honors_A_Cancelled_Token()
    {
        var generator = CreateGenerator();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => generator.GenerateAssemblyAsync(new DynaBeeTestSpecification("Tests.Cancelled"), cancellation.Token));
    }

    [Fact]
    public async Task GenerateAssemblyAsync_Returns_A_Diagnostic_For_Configuration_Errors()
    {
        var specification = new DynaBeeTestSpecification("Tests.Failure")
            .Configure(_ => throw new InvalidOperationException("configuration failed"));

        var result = await CreateGenerator().GenerateAssemblyAsync(specification);

        Assert.Null(result.Assembly);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(DynaBeeTestDiagnosticSeverity.Error, diagnostic.Severity);
        Assert.IsType<InvalidOperationException>(diagnostic.Exception);
    }

    [Fact]
    public async Task GenerateAssemblyAsync_Preserves_Consumer_Source_Snapshots()
    {
        var specification = DynaBeeTestSpecification.Create("Tests.Sources", builder => builder.AddClass("Generated", _ => { }))
            .AddGeneratedSource("Generated.cs", "public sealed class Generated {}");

        var result = await CreateGenerator().GenerateAssemblyAsync(specification);

        Assert.NotNull(result.Assembly);
        Assert.Equal("public sealed class Generated {}", result.GeneratedSources["Generated.cs"]);
    }

    private static DynaBeeTestGenerator CreateGenerator()
        => new(new DynaBeeAssemblyBuilderFactory());
}

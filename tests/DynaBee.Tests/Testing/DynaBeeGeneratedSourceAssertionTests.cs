namespace DynaBee.Tests.Testing;

using DynaBee.Testing;
using DynaBee.Testing.Assertions;

public sealed class DynaBeeGeneratedSourceAssertionTests
{
    [Fact]
    public void ShouldMatchSource_Accepts_An_Exact_Snapshot()
    {
        var result = CreateResult("Generated.cs", "public sealed class Generated {}");

        result.ShouldMatchSource("Generated.cs", "public sealed class Generated {}");
    }

    [Fact]
    public void ShouldMatchSource_Requires_A_Result_And_Existing_Path()
    {
        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedSourceAssertions.ShouldMatchSource(null!, "Generated.cs", "source"));

        var result = CreateResult("Generated.cs", "source");
        Assert.Throws<InvalidOperationException>(() => result.ShouldMatchSource("Missing.cs", "source"));
    }

    [Fact]
    public void ShouldMatchSource_Reports_A_Different_Snapshot()
    {
        var result = CreateResult("Generated.cs", "actual");

        Assert.Throws<InvalidOperationException>(() => result.ShouldMatchSource("Generated.cs", "expected"));
    }

    private static DynaBeeTestGenerationResult CreateResult(string path, string source)
        => new(
            null!,
            Array.Empty<DynaBeeTestDiagnostic>(),
            new Dictionary<string, string> { [path] = source });
}

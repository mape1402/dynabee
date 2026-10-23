namespace DynaBee.Tests.Testing;

using DynaBee.FluentApi.DependencyInjection;
using DynaBee.Testing;

public sealed class DynaBeeTestGenerationResultTests
{
    [Fact]
    public void Constructor_Uses_Empty_Collections_When_Optional_Arguments_Are_Null()
    {
        var result = new DynaBeeTestGenerationResult(null!, null!, null!);

        Assert.Null(result.Assembly);
        Assert.Empty(result.Diagnostics);
        Assert.Empty(result.GeneratedSources);
    }

    [Fact]
    public void WriteGeneratedSourcesTo_Requires_A_Nonempty_Path()
    {
        var result = new DynaBeeTestGenerationResult(null!, Array.Empty<DynaBeeTestDiagnostic>());

        Assert.Throws<ArgumentException>(() => result.WriteGeneratedSourcesTo(null!));
        Assert.Throws<ArgumentException>(() => result.WriteGeneratedSourcesTo(" "));
    }

    [Fact]
    public void WriteGeneratedSourcesTo_Writes_Explicit_Nested_Sources()
    {
        using var directory = TemporaryDirectory.Create();
        var result = new DynaBeeTestGenerationResult(
            null!,
            Array.Empty<DynaBeeTestDiagnostic>(),
            new Dictionary<string, string>
            {
                ["snapshots/Generated.cs"] = "public sealed class Generated {}"
            });

        result.WriteGeneratedSourcesTo(directory.Path);

        Assert.Equal(
            "public sealed class Generated {}",
            File.ReadAllText(System.IO.Path.Combine(directory.Path, "snapshots", "Generated.cs"), System.Text.Encoding.UTF8));
    }

    [Fact]
    public void WriteGeneratedSourcesTo_Rejects_Rooted_Source_Paths()
    {
        using var directory = TemporaryDirectory.Create();
        var rootedPath = System.IO.Path.GetFullPath("Generated.cs");
        var result = new DynaBeeTestGenerationResult(
            null!,
            Array.Empty<DynaBeeTestDiagnostic>(),
            new Dictionary<string, string> { [rootedPath] = "source" });

        Assert.Throws<InvalidOperationException>(() => result.WriteGeneratedSourcesTo(directory.Path));
    }

    [Fact]
    public void WriteGeneratedSourcesTo_Writes_Diagnostics_When_Assembly_Is_Missing()
    {
        using var directory = TemporaryDirectory.Create();
        var result = new DynaBeeTestGenerationResult(
            null!,
            new[] { new DynaBeeTestDiagnostic("generation failed") });

        result.WriteGeneratedSourcesTo(directory.Path);

        var diagnosticsPath = System.IO.Path.Combine(directory.Path, "generation.diagnostics.txt");
        Assert.True(File.Exists(diagnosticsPath));
        Assert.Contains("generation failed", File.ReadAllText(diagnosticsPath));
    }

    [Fact]
    public async Task WriteGeneratedSourcesTo_Creates_Snapshots_For_All_Generated_Type_Forms()
    {
        using var directory = TemporaryDirectory.Create();
        var generator = new DynaBeeTestGenerator(new DynaBeeAssemblyBuilderFactory());
        var specification = DynaBeeTestSpecification.Create("Tests.Snapshots", builder => builder
            .AddClass("SnapshotClass", type => type.AddAutoProperty<string>("Name"))
            .AddInterface("ISnapshot", _ => { })
            .AddStruct("SnapshotStruct", _ => { })
            .AddEnum<int>("SnapshotEnum", values => values.AddValue("First", 1)));
        var result = await generator.GenerateAssemblyAsync(specification);

        result.WriteGeneratedSourcesTo(directory.Path);

        Assert.Contains("public class SnapshotClass", File.ReadAllText(System.IO.Path.Combine(directory.Path, "SnapshotClass.g.cs")));
        Assert.Contains("public interface ISnapshot", File.ReadAllText(System.IO.Path.Combine(directory.Path, "ISnapshot.g.cs")));
        Assert.Contains("public struct SnapshotStruct", File.ReadAllText(System.IO.Path.Combine(directory.Path, "SnapshotStruct.g.cs")));
        Assert.Contains("public enum SnapshotEnum", File.ReadAllText(System.IO.Path.Combine(directory.Path, "SnapshotEnum.g.cs")));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        private TemporaryDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryDirectory Create()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dynabee-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TemporaryDirectory(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}

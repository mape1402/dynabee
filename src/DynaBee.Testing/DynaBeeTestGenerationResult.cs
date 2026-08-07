namespace DynaBee.Testing;

using System.Text;

/// <summary>
/// Result of a DynaBee test generation operation.
/// </summary>
public sealed class DynaBeeTestGenerationResult
{
    private readonly IReadOnlyDictionary<string, string> _generatedSources;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynaBeeTestGenerationResult"/> class.
    /// </summary>
    /// <param name="assembly">Generated assembly context.</param>
    /// <param name="diagnostics">Generation diagnostics.</param>
    /// <param name="generatedSources">Optional generated sources.</param>
    public DynaBeeTestGenerationResult(
        IAssemblyContext assembly,
        IReadOnlyList<DynaBeeTestDiagnostic> diagnostics,
        IReadOnlyDictionary<string, string> generatedSources = null)
    {
        Assembly = assembly;
        Diagnostics = diagnostics ?? Array.Empty<DynaBeeTestDiagnostic>();
        _generatedSources = generatedSources ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets the generated assembly context, or <see langword="null"/> when generation failed.
    /// </summary>
    public IAssemblyContext Assembly { get; }

    /// <summary>
    /// Gets generation diagnostics.
    /// </summary>
    public IReadOnlyList<DynaBeeTestDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Writes generated source snapshots to a directory.
    /// </summary>
    /// <param name="path">Target directory path.</param>
    public void WriteGeneratedSourcesTo(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException(nameof(path));

        Directory.CreateDirectory(path);

        var sources = _generatedSources.Count > 0
            ? _generatedSources
            : CreateDiagnosticSources();

        foreach (var source in sources)
        {
            var relativePath = NormalizeRelativePath(source.Key);
            var filePath = Path.Combine(path, relativePath);
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, source.Value, Encoding.UTF8);
        }
    }

    private IReadOnlyDictionary<string, string> CreateDiagnosticSources()
    {
        if (Assembly == null)
            return new Dictionary<string, string>
            {
                ["generation.diagnostics.txt"] = string.Join(Environment.NewLine, Diagnostics.Select(x => x.ToString()))
            };

        return Assembly
            .Find(_ => true)
            .ToDictionary(
                type => $"{type.Name}.g.cs",
                type => DynaBeeGeneratedSourceWriter.WriteType(type),
                StringComparer.Ordinal);
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        if (Path.IsPathRooted(normalized))
            throw new InvalidOperationException("Generated source paths must be relative.");

        return normalized;
    }
}

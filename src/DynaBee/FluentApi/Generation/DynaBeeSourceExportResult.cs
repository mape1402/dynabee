namespace DynaBee.FluentApi.Generation
{
    /// <summary>
    /// Contains C# source exported from a declarative DynaBee generation plan.
    /// </summary>
    public sealed class DynaBeeSourceExportResult
    {
        internal DynaBeeSourceExportResult(string source, IReadOnlyList<DynaBeeSourceMapEntry> mappings)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
        }

        /// <summary>Gets the deterministic C# source.</summary>
        public string Source { get; }

        /// <summary>Gets mappings from source declarations to plan elements.</summary>
        public IReadOnlyList<DynaBeeSourceMapEntry> Mappings { get; }

        /// <summary>
        /// Writes the generated source to a UTF-8 file.
        /// </summary>
        /// <param name="path">Output file path.</param>
        public void WriteTo(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(nameof(path));

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, Source, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }
}

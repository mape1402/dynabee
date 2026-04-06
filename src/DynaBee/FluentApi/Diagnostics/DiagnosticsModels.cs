namespace DynaBee.FluentApi.Diagnostics
{
    public sealed class AssemblyDiagnostic
    {
        public string Name { get; init; }

        public string Version { get; init; }

        public IReadOnlyCollection<TypeDiagnostic> Types { get; init; } = Array.Empty<TypeDiagnostic>();
    }

    public sealed class TypeDiagnostic
    {
        public string Name { get; init; }

        public string FullName { get; init; }

        public string Kind { get; init; }

        public string AccessModifier { get; init; }

        public IReadOnlyCollection<string> Attributes { get; init; } = Array.Empty<string>();

        public IReadOnlyCollection<MemberDiagnostic> Members { get; init; } = Array.Empty<MemberDiagnostic>();
    }

    public sealed class MemberDiagnostic
    {
        public string Name { get; init; }

        public string Kind { get; init; }

        public string Signature { get; init; }

        public string AccessModifier { get; init; }

        public IReadOnlyCollection<string> Attributes { get; init; } = Array.Empty<string>();
    }
}
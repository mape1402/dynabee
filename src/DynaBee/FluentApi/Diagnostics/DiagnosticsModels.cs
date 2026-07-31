namespace DynaBee.FluentApi.Diagnostics
{
    /// <summary>
    /// Diagnostic snapshot of a generated assembly.
    /// </summary>
    public sealed class AssemblyDiagnostic
    {
        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the assembly version text.
        /// </summary>
        public string Version { get; init; }

        /// <summary>
        /// Gets metadata keys attached to the generated assembly context.
        /// </summary>
        public IReadOnlyCollection<string> MetadataKeys { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets diagnostic details for generated types.
        /// </summary>
        public IReadOnlyCollection<TypeDiagnostic> Types { get; init; } = Array.Empty<TypeDiagnostic>();
    }

    /// <summary>
    /// Diagnostic snapshot of a generated type.
    /// </summary>
    public sealed class TypeDiagnostic
    {
        /// <summary>
        /// Gets the type short name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the type full name.
        /// </summary>
        public string FullName { get; init; }

        /// <summary>
        /// Gets the type kind (class, interface, struct, enum, record, etc.).
        /// </summary>
        public string Kind { get; init; }

        /// <summary>
        /// Gets the type access modifier.
        /// </summary>
        public string AccessModifier { get; init; }

        /// <summary>
        /// Gets the full name of the generated type base type.
        /// </summary>
        public string BaseType { get; init; }

        /// <summary>
        /// Gets generated type interface full names.
        /// </summary>
        public IReadOnlyCollection<string> Interfaces { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets metadata keys attached to the generated type context.
        /// </summary>
        public IReadOnlyCollection<string> MetadataKeys { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets custom attributes declared on the type.
        /// </summary>
        public IReadOnlyCollection<string> Attributes { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets diagnostic details for members declared on the type.
        /// </summary>
        public IReadOnlyCollection<MemberDiagnostic> Members { get; init; } = Array.Empty<MemberDiagnostic>();
    }

    /// <summary>
    /// Diagnostic snapshot of a generated member.
    /// </summary>
    public sealed class MemberDiagnostic
    {
        /// <summary>
        /// Gets the member name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the member kind (method, property, field, etc.).
        /// </summary>
        public string Kind { get; init; }

        /// <summary>
        /// Gets a member signature summary.
        /// </summary>
        public string Signature { get; init; }

        /// <summary>
        /// Gets the member access modifier.
        /// </summary>
        public string AccessModifier { get; init; }

        /// <summary>
        /// Gets metadata keys attached to the generated member context.
        /// </summary>
        public IReadOnlyCollection<string> MetadataKeys { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets custom attributes declared on the member.
        /// </summary>
        public IReadOnlyCollection<string> Attributes { get; init; } = Array.Empty<string>();
    }
}

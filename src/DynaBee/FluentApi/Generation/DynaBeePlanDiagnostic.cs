namespace DynaBee.FluentApi.Generation
{
    /// <summary>
    /// Describes a validation finding for a <see cref="DynaBeeGenerationPlan"/>.
    /// </summary>
    public sealed class DynaBeePlanDiagnostic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynaBeePlanDiagnostic"/> class.
        /// </summary>
        /// <param name="severity">Finding severity.</param>
        /// <param name="code">Stable diagnostic code.</param>
        /// <param name="message">Human-readable explanation.</param>
        /// <param name="typeName">Optional logical generated type name.</param>
        /// <param name="memberName">Optional logical generated member name.</param>
        public DynaBeePlanDiagnostic(
            DynaBeePlanDiagnosticSeverity severity,
            string code,
            string message,
            string typeName = null,
            string memberName = null)
        {
            Severity = severity;
            Code = string.IsNullOrWhiteSpace(code) ? throw new ArgumentException(nameof(code)) : code;
            Message = string.IsNullOrWhiteSpace(message) ? throw new ArgumentException(nameof(message)) : message;
            TypeName = typeName;
            MemberName = memberName;
        }

        /// <summary>
        /// Gets the finding severity.
        /// </summary>
        public DynaBeePlanDiagnosticSeverity Severity { get; }

        /// <summary>
        /// Gets the stable diagnostic code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the human-readable explanation.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the logical generated type that produced the finding, when applicable.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the logical generated member that produced the finding, when applicable.
        /// </summary>
        public string MemberName { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            var location = string.IsNullOrWhiteSpace(TypeName)
                ? string.Empty
                : string.IsNullOrWhiteSpace(MemberName) ? $" ({TypeName})" : $" ({TypeName}.{MemberName})";
            return $"{Severity} {Code}{location}: {Message}";
        }
    }
}

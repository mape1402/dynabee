namespace DynaBee.FluentApi.Generation
{
    /// <summary>
    /// Contains the diagnostics collected while validating a generation plan.
    /// </summary>
    public sealed class DynaBeePlanValidationResult
    {
        internal DynaBeePlanValidationResult(IEnumerable<DynaBeePlanDiagnostic> diagnostics)
        {
            Diagnostics = (diagnostics ?? throw new ArgumentNullException(nameof(diagnostics))).ToArray();
        }

        /// <summary>
        /// Gets every validation finding in declaration order.
        /// </summary>
        public IReadOnlyList<DynaBeePlanDiagnostic> Diagnostics { get; }

        /// <summary>
        /// Gets whether validation produced one or more errors.
        /// </summary>
        public bool HasErrors => Diagnostics.Any(x => x.Severity == DynaBeePlanDiagnosticSeverity.Error);

        /// <summary>
        /// Gets whether validation produced one or more warnings.
        /// </summary>
        public bool HasWarnings => Diagnostics.Any(x => x.Severity == DynaBeePlanDiagnosticSeverity.Warning);

        /// <summary>
        /// Throws when validation produced one or more errors.
        /// </summary>
        /// <exception cref="DynaBeePlanValidationException">Thrown when the plan is invalid.</exception>
        public void ThrowIfErrors()
        {
            if (HasErrors)
                throw new DynaBeePlanValidationException(this);
        }
    }
}

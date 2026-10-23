namespace DynaBee.FluentApi.Extensions
{
    using DynaBee.FluentApi.Generation;

    /// <summary>
    /// Collects extension validation diagnostics for one plan build.
    /// </summary>
    public sealed class DynaBeeExtensionValidationContext
    {
        private readonly List<DynaBeePlanDiagnostic> _diagnostics = new();

        /// <summary>Gets extension diagnostics in reporting order.</summary>
        public IReadOnlyList<DynaBeePlanDiagnostic> Diagnostics => _diagnostics;

        /// <summary>Reports an extension validation error.</summary>
        public void Error(string code, string message, string typeName = null, string memberName = null)
            => _diagnostics.Add(new DynaBeePlanDiagnostic(DynaBeePlanDiagnosticSeverity.Error, code, message, typeName, memberName));

        /// <summary>Reports an extension validation warning.</summary>
        public void Warning(string code, string message, string typeName = null, string memberName = null)
            => _diagnostics.Add(new DynaBeePlanDiagnostic(DynaBeePlanDiagnosticSeverity.Warning, code, message, typeName, memberName));

        internal void ThrowIfErrors()
            => new DynaBeePlanValidationResult(_diagnostics).ThrowIfErrors();
    }
}

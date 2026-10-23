namespace DynaBee.FluentApi.Generation
{
    /// <summary>
    /// The exception thrown when a generation plan contains validation errors.
    /// </summary>
    public sealed class DynaBeePlanValidationException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynaBeePlanValidationException"/> class.
        /// </summary>
        /// <param name="validation">Validation result that caused the exception.</param>
        public DynaBeePlanValidationException(DynaBeePlanValidationResult validation)
            : base(BuildMessage(validation))
        {
            Validation = validation ?? throw new ArgumentNullException(nameof(validation));
        }

        /// <summary>
        /// Gets the validation result that caused the exception.
        /// </summary>
        public DynaBeePlanValidationResult Validation { get; }

        private static string BuildMessage(DynaBeePlanValidationResult validation)
        {
            if (validation == null)
                throw new ArgumentNullException(nameof(validation));

            return "DynaBee generation plan validation failed:" + Environment.NewLine +
                string.Join(Environment.NewLine, validation.Diagnostics
                    .Where(x => x.Severity == DynaBeePlanDiagnosticSeverity.Error)
                    .Select(x => x.ToString()));
        }
    }
}

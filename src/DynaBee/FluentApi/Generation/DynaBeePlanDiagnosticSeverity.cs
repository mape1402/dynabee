namespace DynaBee.FluentApi.Generation
{
    /// <summary>
    /// Describes the severity of a generation-plan diagnostic.
    /// </summary>
    public enum DynaBeePlanDiagnosticSeverity
    {
        /// <summary>
        /// Informational feedback that does not affect generation.
        /// </summary>
        Info,

        /// <summary>
        /// A condition that should be reviewed but does not prevent generation.
        /// </summary>
        Warning,

        /// <summary>
        /// A condition that prevents a plan from being emitted safely.
        /// </summary>
        Error
    }
}

namespace DynaBee.Testing;

/// <summary>
/// Indicates the severity of a DynaBee test-generation diagnostic.
/// </summary>
public enum DynaBeeTestDiagnosticSeverity
{
    /// <summary>Informational diagnostic.</summary>
    Info,

    /// <summary>Non-fatal diagnostic.</summary>
    Warning,

    /// <summary>Generation-blocking diagnostic.</summary>
    Error
}

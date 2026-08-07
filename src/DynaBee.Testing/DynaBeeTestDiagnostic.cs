namespace DynaBee.Testing;

/// <summary>
/// Represents a diagnostic produced while generating a test assembly.
/// </summary>
public sealed class DynaBeeTestDiagnostic
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynaBeeTestDiagnostic"/> class.
    /// </summary>
    /// <param name="message">Diagnostic message.</param>
    /// <param name="exception">Optional source exception.</param>
    public DynaBeeTestDiagnostic(string message, Exception exception = null)
    {
        Message = string.IsNullOrWhiteSpace(message) ? throw new ArgumentException(nameof(message)) : message;
        Exception = exception;
    }

    /// <summary>
    /// Gets the diagnostic message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the exception that produced the diagnostic, when available.
    /// </summary>
    public Exception Exception { get; }

    /// <inheritdoc/>
    public override string ToString()
        => Exception == null ? Message : $"{Message}: {Exception.Message}";
}

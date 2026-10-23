namespace DynaBee.Tests.Testing;

using DynaBee.Testing;
using DynaBee.Testing.Assertions;

public sealed class DynaBeeDiagnosticAssertionTests
{
    [Fact]
    public void ShouldBeEmpty_Accepts_An_Empty_Collection()
    {
        Array.Empty<DynaBeeTestDiagnostic>().ShouldBeEmpty();
    }

    [Fact]
    public void ShouldBeEmpty_Rejects_Null_And_Diagnostics()
    {
        Assert.Throws<ArgumentNullException>(() => DynaBeeDiagnosticAssertions.ShouldBeEmpty(null!));

        var error = Assert.Throws<InvalidOperationException>(() => new[] { Error("failure") }.ShouldBeEmpty());

        Assert.Contains("failure", error.Message);
    }

    [Fact]
    public void ShouldHaveNoErrors_Only_Rejects_Errors()
    {
        new[] { Warning("warning") }.ShouldHaveNoErrors();
        Assert.Throws<ArgumentNullException>(() => DynaBeeDiagnosticAssertions.ShouldHaveNoErrors(null!));

        var error = Assert.Throws<InvalidOperationException>(() => new[] { Error("failure") }.ShouldHaveNoErrors());

        Assert.Contains("failure", error.Message);
    }

    [Fact]
    public void ShouldHaveNoWarnings_Only_Rejects_Warnings()
    {
        new[] { Error("failure") }.ShouldHaveNoWarnings();
        Assert.Throws<ArgumentNullException>(() => DynaBeeDiagnosticAssertions.ShouldHaveNoWarnings(null!));

        var error = Assert.Throws<InvalidOperationException>(() => new[] { Warning("warning") }.ShouldHaveNoWarnings());

        Assert.Contains("warning", error.Message);
    }

    [Fact]
    public void Diagnostic_Validates_Message_And_Formats_Optional_Exception()
    {
        Assert.Throws<ArgumentException>(() => new DynaBeeTestDiagnostic(" "));

        Assert.Equal("Warning: message", Warning("message").ToString());
        Assert.Equal(
            "Error: failed: inner",
            new DynaBeeTestDiagnostic("failed", new InvalidOperationException("inner")).ToString());
    }

    private static DynaBeeTestDiagnostic Error(string message)
        => new(message);

    private static DynaBeeTestDiagnostic Warning(string message)
        => new(message, severity: DynaBeeTestDiagnosticSeverity.Warning);
}

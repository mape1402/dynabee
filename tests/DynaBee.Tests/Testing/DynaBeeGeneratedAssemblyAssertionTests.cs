namespace DynaBee.Tests.Testing;

using DynaBee.Testing.Assertions;
using NSubstitute;

public sealed class DynaBeeGeneratedAssemblyAssertionTests
{
    [Fact]
    public void ShouldExist_Returns_The_Assembly_And_Rejects_Null()
    {
        var assembly = Substitute.For<IAssemblyContext>();

        Assert.Same(assembly, assembly.ShouldExist());
        Assert.Throws<InvalidOperationException>(() => DynaBeeGeneratedAssemblyAssertions.ShouldExist(null!));
    }

    [Fact]
    public void ShouldContainType_Returns_The_Matching_Context()
    {
        var assembly = Substitute.For<IAssemblyContext>();
        var typeContext = Substitute.For<ITypeContext>();
        assembly.Find("Generated").Returns(typeContext);

        Assert.Same(typeContext, assembly.ShouldContainType("Generated"));
    }

    [Fact]
    public void ShouldContainType_Reports_Missing_And_Failing_Lookups()
    {
        var assembly = Substitute.For<IAssemblyContext>();
        assembly.Name.Returns("Tests.Generated");
        assembly.Find("Missing").Returns((ITypeContext)null!);
        assembly.Find("Broken").Returns(_ => throw new InvalidOperationException("lookup failed"));

        Assert.Throws<InvalidOperationException>(() => assembly.ShouldContainType("Missing"));

        var error = Assert.Throws<InvalidOperationException>(() => assembly.ShouldContainType("Broken"));

        Assert.Contains("Tests.Generated", error.Message);
        Assert.IsType<InvalidOperationException>(error.InnerException);
    }
}

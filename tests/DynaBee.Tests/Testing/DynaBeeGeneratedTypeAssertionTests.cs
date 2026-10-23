namespace DynaBee.Tests.Testing;

using DynaBee.Testing.Assertions;
using NSubstitute;

public sealed class DynaBeeGeneratedTypeAssertionTests
{
    [Fact]
    public void ShouldImplement_Supports_Exact_And_Generic_Interfaces()
    {
        Assert.Same(typeof(ConcreteContract), typeof(ConcreteContract).ShouldImplement(typeof(IContract)));
        Assert.Same(typeof(GenericContract), typeof(GenericContract).ShouldImplement(typeof(IGenericContract<>)));

        var context = Substitute.For<ITypeContext>();
        context.ClrType.Returns(typeof(ConcreteContract));

        Assert.Same(context, context.ShouldImplement(typeof(IContract)));
    }

    [Fact]
    public void ShouldImplement_Reports_Invalid_Inputs_And_Missing_Interfaces()
    {
        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldImplement((Type)null!, typeof(IContract)));
        Assert.Throws<ArgumentNullException>(() => typeof(ConcreteContract).ShouldImplement(null!));
        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldImplement((ITypeContext)null!, typeof(IContract)));

        Assert.Throws<InvalidOperationException>(() => typeof(PlainType).ShouldImplement(typeof(IContract)));
    }

    [Fact]
    public void ShouldInheritFrom_Supports_Exact_And_Generic_Base_Types()
    {
        Assert.Same(typeof(DirectChild), typeof(DirectChild).ShouldInheritFrom(typeof(Parent)));
        Assert.Same(typeof(GenericChild), typeof(GenericChild).ShouldInheritFrom(typeof(GenericParent<>)));

        var context = Substitute.For<ITypeContext>();
        context.ClrType.Returns(typeof(DirectChild));

        Assert.Same(context, context.ShouldInheritFrom(typeof(Parent)));
    }

    [Fact]
    public void ShouldInheritFrom_Reports_Invalid_Inputs_And_Missing_Base_Types()
    {
        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldInheritFrom((Type)null!, typeof(Parent)));
        Assert.Throws<ArgumentNullException>(() => typeof(DirectChild).ShouldInheritFrom(null!));
        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldInheritFrom((ITypeContext)null!, typeof(Parent)));

        Assert.Throws<InvalidOperationException>(() => typeof(PlainType).ShouldInheritFrom(typeof(Parent)));
    }

    [Fact]
    public void ShouldHaveConstructor_Validates_Clr_And_Context_Types()
    {
        var context = Substitute.For<ITypeContext>();
        context.ClrType.Returns(typeof(MemberShape));

        Assert.NotNull(typeof(MemberShape).ShouldHaveConstructor(typeof(string)));
        Assert.NotNull(typeof(MemberShape).ShouldHaveConstructor(null!));
        Assert.NotNull(context.ShouldHaveConstructor(typeof(string)));

        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldHaveConstructor((Type)null!, typeof(string)));
        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldHaveConstructor((ITypeContext)null!, typeof(string)));
        Assert.Throws<InvalidOperationException>(() => typeof(MemberShape).ShouldHaveConstructor(typeof(int)));
    }

    [Fact]
    public void ShouldHaveMethod_Validates_Clr_And_Context_Types()
    {
        var context = Substitute.For<ITypeContext>();
        context.ClrType.Returns(typeof(MemberShape));

        Assert.NotNull(typeof(MemberShape).ShouldHaveMethod(nameof(MemberShape.Echo), typeof(string)));
        Assert.NotNull(context.ShouldHaveMethod(nameof(MemberShape.Echo), typeof(string)));

        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldHaveMethod((Type)null!, "Echo"));
        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldHaveMethod((ITypeContext)null!, "Echo"));
        Assert.Throws<InvalidOperationException>(() => typeof(MemberShape).ShouldHaveMethod(nameof(MemberShape.Echo), typeof(int)));
    }

    [Fact]
    public void ShouldHaveProperty_Validates_Clr_And_Context_Types()
    {
        var context = Substitute.For<ITypeContext>();
        context.ClrType.Returns(typeof(MemberShape));

        Assert.NotNull(typeof(MemberShape).ShouldHaveProperty(nameof(MemberShape.Name), typeof(string)));
        Assert.NotNull(context.ShouldHaveProperty(nameof(MemberShape.Name), typeof(string)));

        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldHaveProperty((Type)null!, nameof(MemberShape.Name), typeof(string)));
        Assert.Throws<ArgumentNullException>(() => typeof(MemberShape).ShouldHaveProperty(nameof(MemberShape.Name), null!));
        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldHaveProperty((ITypeContext)null!, nameof(MemberShape.Name), typeof(string)));
        Assert.Throws<InvalidOperationException>(() => typeof(MemberShape).ShouldHaveProperty(nameof(MemberShape.Name), typeof(int)));
        Assert.Throws<InvalidOperationException>(() => typeof(MemberShape).ShouldHaveProperty("Missing", typeof(string)));
    }

    [Fact]
    public void ShouldHaveGenericArguments_Validates_Both_Shapes()
    {
        Assert.Same(typeof(GenericParent<int>), typeof(GenericParent<int>).ShouldHaveGenericArguments(typeof(int)));
        Assert.Same(typeof(string), typeof(string).ShouldHaveGenericArguments(null!));

        Assert.Throws<ArgumentNullException>(() => DynaBeeGeneratedTypeAssertions.ShouldHaveGenericArguments(null!, typeof(int)));
        Assert.Throws<InvalidOperationException>(() => typeof(GenericParent<int>).ShouldHaveGenericArguments(typeof(string)));
    }

    public interface IContract
    {
    }

    public interface IGenericContract<T>
    {
    }

    public sealed class ConcreteContract : IContract
    {
    }

    public sealed class GenericContract : IGenericContract<int>
    {
    }

    public class Parent
    {
    }

    public sealed class DirectChild : Parent
    {
    }

    public class GenericParent<T>
    {
    }

    public sealed class GenericChild : GenericParent<int>
    {
    }

    public sealed class PlainType
    {
    }

    public sealed class MemberShape
    {
        public MemberShape()
        {
        }

        public MemberShape(string value)
        {
            Name = value;
        }

        public string Name { get; set; }

        public string Echo(string value) => value;
    }
}

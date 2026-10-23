namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using System.Reflection.Emit;
using DynaBee.FluentApi;
using DynaBee.Infrastructure;
using DynaBee.Infrastructure.Configurators;
using DynaBee.Infrastructure.ContextBuilders;

public sealed class RuntimeInfrastructureCoverageTests
{
    [Fact]
    public void Attributes_Resolve_Null_Reference_Arguments_And_Reject_Invalid_Constructors()
    {
        var nullableReference = BeeAttribute.Of<ReferenceAttribute>((object)null!);

        Assert.NotNull(nullableReference.Build());
        Assert.Throws<InvalidOperationException>(() => BeeAttribute.Of<ValueAttribute>((object)null!).Build());
        Assert.Throws<InvalidOperationException>(() => BeeAttribute.Of<ValueAttribute>("invalid").Build());
        Assert.Throws<InvalidOperationException>(() => BeeAttribute.Of<ReferenceAttribute>("value").WithProperty("Missing", "value").Build());
        Assert.Throws<InvalidOperationException>(() => BeeAttribute.Of<ReferenceAttribute>("value").WithField("Missing", "value").Build());
        Assert.Throws<ArgumentException>(() => BeeAttribute.Of(typeof(string)));
    }

    [Fact]
    public void Lambda_Registry_Dispatches_Direct_Self_Bound_And_Invalid_Signatures()
    {
        var direct = LambdaMethodRegistry.Register((Func<int, int>)(value => value + 1));
        var selfBound = LambdaMethodRegistry.Register((Func<object, string>)(instance => instance?.ToString() ?? "none"));
        var incompatible = LambdaMethodRegistry.Register((Func<int, int, int>)((left, right) => left + right));

        Assert.Equal(3, LambdaMethodRegistry.Invoke(direct, null!, new object[] { 2 }));
        Assert.Equal("none", LambdaMethodRegistry.Invoke(selfBound, null!, null!));
        Assert.Throws<InvalidOperationException>(() => LambdaMethodRegistry.Invoke(incompatible, null!, Array.Empty<object>()));
        Assert.Throws<InvalidOperationException>(() => LambdaMethodRegistry.Invoke(-1, null!, Array.Empty<object>()));
        Assert.Throws<ArgumentNullException>(() => LambdaMethodRegistry.Register(null!));
    }

    [Fact]
    public void Class_Configurator_Rejects_Invalid_Structural_Inputs()
    {
        var configurator = new ClassConfigurator("Configured", ClassAccessModifier.Public);

        Assert.Throws<ArgumentNullException>(() => configurator.AddElementBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => configurator.WithParentType(null!));
        Assert.Throws<ArgumentNullException>(() => configurator.Implements(null!));
        Assert.Throws<ArgumentException>(() => configurator.Implements(typeof(string)));
        Assert.Throws<ArgumentNullException>(() => configurator.RegisterAs(null!));
        Assert.Throws<ArgumentException>(() => configurator.WithMetadata("", "value"));
        Assert.Throws<ArgumentNullException>(() => configurator.WithMetadata("key", null!));
        Assert.Throws<ArgumentNullException>(() => configurator.AddAttribute(null!));
        Assert.Throws<ArgumentNullException>(() => configurator.AddGenericParameter(null!));

        Assert.Same(configurator, configurator.Implements(typeof(IDisposable)));
        Assert.Same(configurator, configurator.Implements(typeof(IDisposable), registerInDi: false));
        Assert.Same(configurator, configurator.RegisterAs(typeof(IDisposable)));
        Assert.Same(configurator, configurator.RegisterAsConcrete(false));
    }

    [Fact]
    public void Generic_Parameters_And_Type_References_Resolve_Only_Valid_Shapes()
    {
        var parameter = new BeeGenericTypeParameterBuilder("T");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"DynaBee.Tests.TypeResolution.{Guid.NewGuid():N}"), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Types");
        var assemblyContext = new AssemblyContextBuilder("Types", assembly, module);
        var referencedType = module.DefineType("Referenced", TypeAttributes.Public);
        var ownerType = module.DefineType("Owner", TypeAttributes.Public);
        var owner = (TypeContextBuilder)assemblyContext.AddTypeBuilder("Owner", ownerType);
        assemblyContext.AddTypeBuilder("Referenced", referencedType);
        var generic = ownerType.DefineGenericParameters("T")[0];
        owner.RegisterGenericParameter("T", generic);

        Assert.Throws<ArgumentNullException>(() => parameter.Inherits(null!));
        Assert.Throws<ArgumentException>(() => parameter.Inherits(typeof(IDisposable)));
        Assert.Throws<ArgumentException>(() => parameter.Inherits(typeof(int)));
        Assert.Throws<ArgumentNullException>(() => parameter.Implements(null!));
        Assert.Throws<ArgumentException>(() => parameter.Implements(typeof(string)));
        Assert.Same(parameter, parameter.Inherits(typeof(object)).Implements<IDisposable>());
        Assert.Equal(referencedType, BeeType.Parse("Referenced").Resolve(owner));
        Assert.Equal(generic, BeeType.GenericParameter("T").Resolve(owner));
        Assert.Throws<ArgumentNullException>(() => ((BeeType)typeof(int)).Resolve(null!));
        Assert.Throws<InvalidOperationException>(() => BeeType.GenericParameter("Missing").Resolve(owner));
        Assert.Throws<InvalidOperationException>(() => BeeType.GenericParameter("T").Resolve(new DelegatingTypeContextBuilder()));
    }

    private sealed class DelegatingTypeContextBuilder : ITypeContextBuilder
    {
        public IAssemblyContextBuilder AssemblyBuilderContext => throw new NotSupportedException();

        public string Name => "Delegating";

        public TypeBuilder TypeBuilder => throw new NotSupportedException();

        public IElementContextBuilder AddElement(string name, ElementType elementType, ElementBuilderAction buildAction, IReadOnlyDictionary<string, object> metadata = null)
            => throw new NotSupportedException();

        public void SetMetadata(string key, object value)
            => throw new NotSupportedException();

        public ITypeContext Build()
            => throw new NotSupportedException();
    }

    public sealed class ReferenceAttribute : Attribute
    {
        public ReferenceAttribute(string value)
        {
        }
    }

    public sealed class ValueAttribute : Attribute
    {
        public ValueAttribute(int value)
        {
        }
    }
}

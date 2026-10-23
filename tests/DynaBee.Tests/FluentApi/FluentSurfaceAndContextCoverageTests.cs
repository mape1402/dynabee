namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using System.Reflection.Emit;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Body;
using DynaBee.FluentApi.Diagnostics;
using DynaBee.Infrastructure;
using DynaBee.Infrastructure.Configurators;
using DynaBee.Infrastructure.ContextBuilders;

public sealed class FluentSurfaceAndContextCoverageTests
{
    [Fact]
    public void FluentSurface_Emits_Configured_Attributes_Properties_Indexers_And_Interceptors()
    {
        VoidInterceptorRecorder.Reset();
        var voidMethod = typeof(VoidBase).GetMethod(nameof(VoidBase.Execute))!;
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Surface", type => type
                .AddAttribute<ObsoleteAttribute>("class")
                .AddAutoProperty("Attributed", typeof(int), BeeAttribute.Of<ObsoleteAttribute>("property"))
                .AddAutoProperty("NoAttributes", typeof(string), (BeeAttribute[])null!)
                .AddAutoProperty<int>("WriteOnly", hasGetter: false, hasSetter: true,
                    FieldAccessModifier.Private,
                    MethodAccessModifier.Private,
                    MethodAccessModifier.Public,
                    new[] { BeeAttribute.Of<ObsoleteAttribute>("write") })
                .AddIndexer<int, string>()
                .AddIndexer("Named", typeof(string), typeof(int)))
            .AddClass("Intercepted", type => type
                .Inherits<VoidBase>()
                .Intercept(voidMethod, interception => interception
                    .Before(body => body.Evaluate(body.StaticCall(typeof(VoidInterceptorRecorder), nameof(VoidInterceptorRecorder.Before), Type.EmptyTypes)))
                    .After(body => body.Evaluate(body.StaticCall(typeof(VoidInterceptorRecorder), nameof(VoidInterceptorRecorder.After), Type.EmptyTypes)))
                    .Finally(body => body.Evaluate(body.StaticCall(typeof(VoidInterceptorRecorder), nameof(VoidInterceptorRecorder.Finally), Type.EmptyTypes)))))
            .Build();

        var surfaceType = context.GetClrType("Surface");
        var surface = Activator.CreateInstance(surfaceType)!;
        surfaceType.GetProperty("Attributed")!.SetValue(surface, 3);
        surfaceType.GetProperty("WriteOnly")!.SetValue(surface, 4);
        surfaceType.GetProperty("Item")!.SetValue(surface, "value", new object[] { 1 });
        surfaceType.GetProperty("Named")!.SetValue(surface, 8, new object[] { "id" });

        Assert.Equal("class", surfaceType.GetCustomAttribute<ObsoleteAttribute>()!.Message);
        Assert.Equal("property", surfaceType.GetProperty("Attributed")!.GetCustomAttribute<ObsoleteAttribute>()!.Message);
        Assert.Equal(3, surfaceType.GetProperty("Attributed")!.GetValue(surface));
        Assert.Null(surfaceType.GetProperty("WriteOnly")!.GetGetMethod());
        Assert.Equal("value", surfaceType.GetProperty("Item")!.GetValue(surface, new object[] { 1 }));
        Assert.Equal(8, surfaceType.GetProperty("Named")!.GetValue(surface, new object[] { "id" }));

        var intercepted = (VoidBase)context.CreateInstance("Intercepted");
        intercepted.Execute();
        Assert.Equal(new[] { "before", "base", "after", "finally" }, VoidInterceptorRecorder.Events);
    }

    [Fact]
    public void Contexts_Expose_Metadata_Element_Kinds_And_Internal_Method_Caches()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .WithMetadata("assembly", "metadata")
            .AddClass("ContextType", type => type
                .WithMetadata("type", "metadata")
                .AddMethod("Value", typeof(int), method => method
                    .WithMetadata("method", "metadata")
                    .EmitsBody(body => body.Return(body.Constant(1)))))
            .Build();

        var typeContext = context.Find("ContextType");
        var methodContext = typeContext.FindOne("Value");

        Assert.Equal("metadata", context.GetMetadata("assembly"));
        Assert.Equal("metadata", typeContext.GetMetadata("type"));
        Assert.Equal("metadata", methodContext.GetMetadata("method"));
        Assert.Equal(ElementType.Method, methodContext.ElementType);
        Assert.Throws<KeyNotFoundException>(() => typeContext.GetMetadata("missing"));
        Assert.Throws<KeyNotFoundException>(() => methodContext.GetMetadata("missing"));

        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(NewAssemblyName()), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Coverage");
        var assemblyBuilder = new AssemblyContextBuilder("Coverage", assembly, module);
        var typeBuilder = module.DefineType("CoverageType", TypeAttributes.Public);
        var typeBuilderContext = (TypeContextBuilder)assemblyBuilder.AddTypeBuilder("CoverageType", typeBuilder);
        var method = typeBuilder.DefineMethod("Value", MethodAttributes.Public, typeof(int), Type.EmptyTypes);
        typeBuilderContext.RegisterMethod("Value", Type.EmptyTypes, method);
        var elementBuilder = new ElementContextBuilder("Value", ElementType.Method, _ => { }, typeBuilderContext);
        elementBuilder.SetMetadata("tag", "value");
        var element = elementBuilder.Build();
        var firstKey = new DynamicMethodKey("Value", Type.EmptyTypes);
        var secondKey = new DynamicMethodKey("Value", Type.EmptyTypes);
        var differentKey = new DynamicMethodKey("Other", Type.EmptyTypes);

        Assert.True(typeBuilderContext.TryGetMethod("Value", Type.EmptyTypes, out var cached));
        Assert.Same(method, cached);
        Assert.False(typeBuilderContext.TryGetMethod("Missing", Type.EmptyTypes, out _));
        Assert.Equal("value", element.GetMetadata("tag"));
        Assert.True(firstKey.Equals(secondKey));
        Assert.True(firstKey.Equals((object)secondKey));
        Assert.False(firstKey.Equals(differentKey));
    }

    [Fact]
    public void Diagnostics_Report_Real_Generated_Member_Access_And_Type_Kinds()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("DiagnosticClass", type => type
                .AddField<int>("PublicField", FieldAccessModifier.Public)
                .AddField<int>("PrivateField", FieldAccessModifier.Private)
                .AddField<int>("InternalField", FieldAccessModifier.Internal)
                .AddField<int>("ProtectedField", FieldAccessModifier.Protected)
                .AddField<int>("ProtectedInternalField", FieldAccessModifier.ProtectedInternal)
                .AddField<int>("PrivateProtectedField", FieldAccessModifier.PrivateProtected)
                .AddMethod("PublicMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.Public))
                .AddMethod("PrivateMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.Private))
                .AddMethod("InternalMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.Internal))
                .AddMethod("ProtectedMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.Protected))
                .AddMethod("ProtectedInternalMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.ProtectedInternal))
                .AddMethod("PrivateProtectedMethod", typeof(void), method => method.WithAccess(MethodAccessModifier.PrivateProtected)))
            .AddInterface("IDiagnostic", _ => { })
            .AddStruct("DiagnosticStruct", _ => { })
            .AddEnum<int>("DiagnosticEnum", enumBuilder => enumBuilder.AddValue("One", 1))
            .Build();

        var diagnostics = context.GetDiagnostics();
        var model = diagnostics.Types.Single(type => type.Name == "DiagnosticClass");
        var json = context.ToDiagnosticsJson(indented: false);

        Assert.Equal("Class", model.Kind);
        Assert.Equal("Public", model.AccessModifier);
        Assert.Contains(model.Members, member => member.Name == "PublicField" && member.AccessModifier == "Public");
        Assert.Contains(model.Members, member => member.Name == "PrivateField" && member.AccessModifier == "Private");
        Assert.Contains(model.Members, member => member.Name == "InternalField" && member.AccessModifier == "Internal");
        Assert.Contains(model.Members, member => member.Name == "ProtectedField" && member.AccessModifier == "Protected");
        Assert.Contains(model.Members, member => member.Name == "ProtectedInternalField" && member.AccessModifier == "ProtectedInternal");
        Assert.Contains(model.Members, member => member.Name == "PrivateProtectedField" && member.AccessModifier == "PrivateProtected");
        Assert.Contains(model.Members, member => member.Name == "PublicMethod" && member.AccessModifier == "Public");
        Assert.Contains(model.Members, member => member.Name == "PrivateMethod" && member.AccessModifier == "Private");
        Assert.Contains(model.Members, member => member.Name == "InternalMethod" && member.AccessModifier == "Internal");
        Assert.Contains(model.Members, member => member.Name == "ProtectedMethod" && member.AccessModifier == "Protected");
        Assert.Contains(model.Members, member => member.Name == "ProtectedInternalMethod" && member.AccessModifier == "ProtectedInternal");
        Assert.Contains(model.Members, member => member.Name == "PrivateProtectedMethod" && member.AccessModifier == "PrivateProtected");
        Assert.Contains(diagnostics.Types, type => type.Name == "IDiagnostic" && type.Kind == "Interface");
        Assert.Contains(diagnostics.Types, type => type.Name == "DiagnosticStruct" && type.Kind == "Struct");
        Assert.Contains(diagnostics.Types, type => type.Name == "DiagnosticEnum" && type.Kind == "Enum");
        Assert.Contains("DiagnosticClass", json);
        Assert.Throws<ArgumentNullException>(() => DynaBeeDiagnosticsExtensions.GetDiagnostics(null!));
    }

    [Fact]
    public void FluentBuilders_Validate_Interface_Operators_And_Generic_Property_Configuration()
    {
        Assert.Throws<ArgumentNullException>(() => new BeeAssemblyBuilder(NewAssemblyName()).AddClass("Invalid", type => type.ImplementInterface(null!)));
        Assert.Throws<ArgumentException>(() => new BeeAssemblyBuilder(NewAssemblyName()).AddClass("Invalid", type => type.ImplementInterface(typeof(string))));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BeeAssemblyBuilder(NewAssemblyName()).AddClass("Invalid", type => type.AddOperator((BeeOperatorKind)99, typeof(int), _ => { })));

        var generic = new BeeGenericTypeParameterBuilder("T").ValueType().Implements<IDisposable>();
        Assert.Contains(typeof(IDisposable), generic.InterfaceConstraints);

        var property = new BeeInterfacePropertyBuilder("Value", typeof(int))
            .AsWriteOnly()
            .WithSetter()
            .WithSetterAccess(MethodAccessModifier.Internal);
        Assert.False(property.HasGetter);
        Assert.True(property.HasSetter);
        Assert.Equal(MethodAccessModifier.Internal, property.SetterAccessModifier);

        var enumConfigurator = new EnumConfigurator("Ignored", typeof(int), ClassAccessModifier.Public);
        Assert.Same(enumConfigurator, enumConfigurator.AddElementBuilder(null!));
    }

    [Fact]
    public void FluentBuilders_Configure_Untyped_Properties_Indexers_And_Property_Overrides()
    {
        var baseProperty = typeof(AttributedOverrideBase).GetProperty(nameof(AttributedOverrideBase.Value))!;
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Configured", type => type
                .AddAutoProperty("Untyped", (BeeType)typeof(int))
                .AddProperty("ConfiguredProperty", (BeeType)typeof(string), property => property
                    .AddAttribute(BeeAttribute.Of<ObsoleteAttribute>("property"))
                    .AddAttribute<CLSCompliantAttribute>(attribute => attribute
                        .WithConstructorArgument(false))
                    .WithMetadata("source", "fluent"))
                .AddIndexer((BeeType)typeof(Guid), (BeeType)typeof(decimal)))
            .AddClass("Override", type => type
                .Inherits<AttributedOverrideBase>()
                .OverrideProperty(baseProperty, property => property
                    .AddAttribute(BeeAttribute.Of<ObsoleteAttribute>("override"))
                    .WithMetadata("source", "override")
                    .Getter(getter => getter.ReturnsConstant(7))))
            .Build();

        var configured = context.GetClrType("Configured");
        var instance = Activator.CreateInstance(configured)!;
        var index = Guid.NewGuid();
        configured.GetProperty("Untyped")!.SetValue(instance, 2);
        configured.GetProperty("Item")!.SetValue(instance, 3.5m, new object[] { index });

        Assert.Equal(2, configured.GetProperty("Untyped")!.GetValue(instance));
        Assert.Equal(3.5m, configured.GetProperty("Item")!.GetValue(instance, new object[] { index }));
        Assert.Equal("property", configured.GetProperty("ConfiguredProperty")!.GetCustomAttribute<ObsoleteAttribute>()!.Message);
        Assert.Equal("fluent", context.Find("Configured").FindOne("ConfiguredProperty").GetMetadata("source"));
        Assert.Equal(7, ((AttributedOverrideBase)context.CreateInstance("Override")).Value);
        Assert.Equal("override", context.GetClrType("Override").GetProperty(nameof(AttributedOverrideBase.Value))!.GetCustomAttribute<ObsoleteAttribute>()!.Message);
        Assert.Equal("override", context.Find("Override").FindOne(nameof(AttributedOverrideBase.Value)).GetMetadata("source"));
    }

    [Fact]
    public void Method_Interceptors_Validate_Callbacks_And_Support_All_NonVoid_Combinations()
    {
        var baseMethod = typeof(ValueBase).GetMethod(nameof(ValueBase.Calculate))!;
        var interception = new BeeMethodInterceptionBuilder(baseMethod);

        Assert.Throws<ArgumentNullException>(() => new BeeMethodInterceptionBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => interception.Before(null!));
        Assert.Throws<ArgumentNullException>(() => interception.After((Action<IBeeLocal, IBeeMethodBodyBuilder>)null!));
        Assert.Throws<ArgumentNullException>(() => interception.After((Action<IBeeMethodBodyBuilder>)null!));
        Assert.Throws<ArgumentNullException>(() => interception.Finally(null!));

        ValueInterceptorRecorder.Reset();
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Plain", type => type.Inherits<ValueBase>().Intercept(baseMethod, _ => { }))
            .AddClass("BeforeAfter", type => type.Inherits<ValueBase>().Intercept(baseMethod, builder => builder
                .Before(body => body.Evaluate(body.StaticCall(typeof(ValueInterceptorRecorder), nameof(ValueInterceptorRecorder.Before), Type.EmptyTypes)))
                .After((_, body) => body.Evaluate(body.StaticCall(typeof(ValueInterceptorRecorder), nameof(ValueInterceptorRecorder.After), Type.EmptyTypes)))))
            .AddClass("Finally", type => type.Inherits<ValueBase>().Intercept(baseMethod, builder => builder
                .Finally(body => body.Evaluate(body.StaticCall(typeof(ValueInterceptorRecorder), nameof(ValueInterceptorRecorder.Finally), Type.EmptyTypes)))))
            .Build();

        Assert.Equal(3, ((ValueBase)context.CreateInstance("Plain")).Calculate(2));
        Assert.Equal(4, ((ValueBase)context.CreateInstance("BeforeAfter")).Calculate(3));
        Assert.Equal(5, ((ValueBase)context.CreateInstance("Finally")).Calculate(4));
        Assert.Equal(new[] { "before", "after", "finally" }, ValueInterceptorRecorder.Events);
    }

    private static string NewAssemblyName()
        => $"DynaBee.Tests.FluentSurfaceContext.{Guid.NewGuid():N}";

    public class VoidBase
    {
        public virtual void Execute()
            => VoidInterceptorRecorder.Events.Add("base");
    }

    public static class VoidInterceptorRecorder
    {
        public static List<string> Events { get; } = new();

        public static void Reset()
            => Events.Clear();

        public static void Before()
            => Events.Add("before");

        public static void After()
            => Events.Add("after");

        public static void Finally()
            => Events.Add("finally");
    }

    public abstract class AttributedOverrideBase
    {
        public virtual int Value => 1;
    }

    public class ValueBase
    {
        public virtual int Calculate(int value)
            => value + 1;
    }

    public static class ValueInterceptorRecorder
    {
        public static List<string> Events { get; } = new();

        public static void Reset()
            => Events.Clear();

        public static void Before()
            => Events.Add("before");

        public static void After()
            => Events.Add("after");

        public static void Finally()
            => Events.Add("finally");
    }
}

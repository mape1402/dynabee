namespace DynaBee.Tests.FluentApi;

using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using DynaBee.FluentApi;
using DynaBee.FluentApi.Invocation;
using DynaBee.Infrastructure.Contexts;

public sealed class InvokerExpansionCoverageTests
{
    [Fact]
    public void Invokers_Cache_Descriptors_And_Validate_Runtime_Arguments()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");
        var first = context.CreateMethodInvoker("Dispatch", "Add", new[] { typeof(int), typeof(int) });
        var second = context.CreateMethodInvoker("Dispatch", "Add", new[] { typeof(int), typeof(int) });
        var descriptor = context.GetGeneratedMethodDescriptor("Dispatch", "Add", new[] { typeof(int), typeof(int) });
        var descriptors = context.GetGeneratedMethodDescriptors("Dispatch");

        Assert.Same(first, second);
        Assert.Equal(typeof(int), first.ReturnType);
        Assert.Equal(new[] { typeof(int), typeof(int) }, first.ParameterTypes);
        Assert.Equal("Add", descriptor.Name);
        Assert.Equal(typeof(int), descriptor.ReturnType);
        Assert.Equal(new[] { typeof(int), typeof(int) }, descriptor.ParameterTypes);
        Assert.Contains(descriptors, candidate => candidate.Name == "Add");
        Assert.Equal(5, first.Invoke(instance, new object[] { 2, 3 }));

        Assert.Contains("Target instance", Assert.Throws<InvalidOperationException>(() => first.Invoke(null!, new object[] { 2, 3 })).Message);
        Assert.Contains("Target instance", Assert.Throws<InvalidOperationException>(() => first.Invoke(new object(), new object[] { 2, 3 })).Message);
        Assert.Throws<ArgumentNullException>(() => first.Invoke(instance, null!));
        Assert.Contains("count mismatch", Assert.Throws<InvalidOperationException>(() => first.Invoke(instance, new object[] { 2 })).Message);
        Assert.Contains("cannot be assigned", Assert.Throws<InvalidOperationException>(() => first.Invoke(instance, new object[] { "2", 3 })).Message);
    }

    [Fact]
    public void Object_Adapters_Support_Multiple_Arguments_And_Produce_Clear_Errors()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");
        var add = context.CreateObjectAdapter2("Dispatch", instance, "Add", new[] { typeof(int), typeof(int) });
        var sum = context.CreateObjectAdapter3("Dispatch", instance, "Sum", new[] { typeof(int), typeof(int), typeof(int) });
        var list = context.CreateArgumentListAdapter("Dispatch", instance, "Add", new[] { typeof(int), typeof(int) });

        Assert.Equal(5, add(2, 3));
        Assert.Equal(6, sum(1, 2, 3));
        Assert.Equal(7, list.Invoke(new List<object> { 3, 4 }));
        Assert.Equal(typeof(int), list.ReturnType);
        Assert.Equal(new[] { typeof(int), typeof(int) }, list.ParameterTypes);
        Assert.Contains("cannot be assigned", Assert.Throws<InvalidOperationException>(() => add("wrong", 3)).Message);
        Assert.Contains("cannot be assigned", Assert.Throws<InvalidOperationException>(() => sum(1, null!, 3)).Message);

        var invalidCast = new DynaBeeObjectMethodAdapter(
            context.Name,
            "Dispatch",
            "Add",
            new[] { typeof(int) },
            typeof(int),
            _ => throw new InvalidCastException("bad cast"));
        var nullReference = new DynaBeeObjectMethodAdapter(
            context.Name,
            "Dispatch",
            "Add",
            new[] { typeof(string) },
            typeof(int),
            _ => throw new NullReferenceException("bad null"));

        Assert.Contains("cannot be assigned", Assert.Throws<InvalidOperationException>(() => invalidCast.Invoke(new object[] { "wrong" })).Message);
        Assert.Contains("adapter invocation failed", Assert.Throws<InvalidOperationException>(() => nullReference.Invoke(new object[] { "valid" })).Message);
        Assert.Throws<ArgumentNullException>(() => list.Invoke(null!));
        Assert.Contains("count mismatch", Assert.Throws<InvalidOperationException>(() => list.Invoke(new object[] { 1 })).Message);
        Assert.Throws<ArgumentNullException>(() => new DynaBeeObjectMethodAdapter("a", "t", "m", null!, typeof(int), _ => 1));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeObjectMethodAdapter("a", "t", "m", Type.EmptyTypes, null!, _ => 1));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeObjectMethodAdapter("a", "t", "m", Type.EmptyTypes, typeof(int), null!));
    }

    [Fact]
    public void Delegate_Factories_Validate_Runtime_Delegate_Shapes()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");

        Assert.Contains("parameters do not match", Assert.Throws<InvalidOperationException>(() => context.CreateBoundDelegate<Action<int>>("Dispatch", instance, "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Contains("has no Invoke method", Assert.Throws<InvalidOperationException>(() => context.CreateBoundDelegate(
            typeof(Delegate), "Dispatch", instance, "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Contains("has no Invoke method", Assert.Throws<InvalidOperationException>(() => context.CreateOpenDelegate<Delegate>(
            "Dispatch", "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Contains("parameter 1", Assert.Throws<InvalidOperationException>(() => context.CreateOpenDelegate<Func<object, string, int, int>>("Dispatch", "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Contains("return type", Assert.Throws<InvalidOperationException>(() => context.CreateOpenDelegate<Func<object, int, int, string>>("Dispatch", "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Throws<ArgumentNullException>(() => context.CreateObjectAdapter2("Dispatch", instance, "Add", null!));
        Assert.Throws<ArgumentNullException>(() => context.CreateObjectAdapter3("Dispatch", instance, "Sum", null!));
    }

    [Fact]
    public void Invokers_Reject_Ambiguous_Il_Method_Identities()
    {
        var context = new BeeAssemblyBuilder($"DynaBee.Tests.InvokerAmbiguity.{Guid.NewGuid():N}")
            .AddClass("Ambiguous", type => type
                .AddMethod("Format", typeof(string))
                .AddMethod("Format", typeof(object)))
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.CreateMethodInvoker("Ambiguous", "Format", Type.EmptyTypes));

        Assert.Contains("More than one overload matches", exception.Message);
    }

    [Fact]
    public void Factory_Delegates_Reject_Ambiguous_Il_Constructor_Identities()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"DynaBee.Tests.ConstructorAmbiguity.{Guid.NewGuid():N}"),
            AssemblyBuilderAccess.Run);
        var typeBuilder = assembly.DefineDynamicModule("Types").DefineType("Ambiguous", TypeAttributes.Public);
        DefineConstructor(typeBuilder);
        DefineConstructor(typeBuilder);
        var type = typeBuilder.CreateTypeInfo()!.AsType();
        var context = new AssemblyContext(
            assembly.GetName().Name!,
            assembly,
            new[] { new TypeContext("Ambiguous", type, Array.Empty<IElementContext>()) });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.CreateFactoryDelegate<Func<int, object>>("Ambiguous", new[] { typeof(int) }));

        Assert.Contains("More than one constructor matches", exception.Message);
    }

    [Fact]
    public void Object_Adapter_Compilation_Rejects_Inconsistent_Internal_Arity()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");
        var type = context.GetClrType("Dispatch");
        var method = type.GetMethod("Add", new[] { typeof(int), typeof(int) })!;
        var compile = typeof(AssemblyContextInvokerExtensions)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == "CompileObjectAdapter" && candidate.IsGenericMethodDefinition)
            .MakeGenericMethod(typeof(Func<object, object>));

        var exception = Assert.Throws<TargetInvocationException>(() => compile.Invoke(null, new object[]
        {
            context.Name,
            "Dispatch",
            "Add",
            new[] { typeof(int), typeof(int) },
            type,
            method,
            instance,
            new[] { Expression.Parameter(typeof(object), "only") }
        }));

        Assert.Contains("requires exactly", exception.InnerException!.Message);
    }

    [Fact]
    public void Delegate_Factories_Validate_Generated_Constructor_And_Method_Identity_Inputs()
    {
        var context = new BeeAssemblyBuilder($"DynaBee.Tests.InvokerConstructors.{Guid.NewGuid():N}")
            .AddClass("Dispatch", type => type
                .AddMethod("Echo", typeof(string), method => method
                    .WithParameter<string>("value")
                    .EmitsBody(body => body.Return(body.Parameter<string>("value")))))
            .AddClass("WithArgument", type => type
                .AddConstructor(constructor => constructor.WithParameter<string>("value")))
            .Build();
        var instance = context.CreateInstance("Dispatch");

        var factory = context.CreateFactoryDelegate<Func<string, object>>("WithArgument", new[] { typeof(string) });

        Assert.Equal(context.GetClrType("WithArgument"), factory("value").GetType());
        Assert.Contains("parameters do not match", Assert.Throws<InvalidOperationException>(() =>
            context.CreateFactoryDelegate<Func<object>>("WithArgument", new[] { typeof(string) })).Message);
        Assert.Throws<ArgumentException>(() => context.CreateBoundDelegate<Func<string, string>>("", instance, "Echo", new[] { typeof(string) }));
        Assert.Throws<ArgumentException>(() => context.CreateBoundDelegate<Func<string, string>>("Dispatch", instance, "", new[] { typeof(string) }));
        Assert.Throws<ArgumentNullException>(() => context.CreateBoundDelegate<Func<string, string>>("Dispatch", instance, "Echo", null!));
        Assert.Contains("cannot be null", Assert.Throws<InvalidOperationException>(() =>
            context.CreateBoundDelegate<Func<string, string>>("Dispatch", null!, "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("not assignable", Assert.Throws<InvalidOperationException>(() =>
            context.CreateBoundDelegate<Func<string, string>>("Dispatch", new object(), "Echo", new[] { typeof(string) })).Message);
    }

    [Fact]
    public void Descriptors_And_Object_Adapters_Protect_Their_Public_Contracts()
    {
        var parameterTypes = new List<Type> { typeof(string) };
        var descriptor = new DynaBeeGeneratedMethodDescriptor(typeof(string), "Format", parameterTypes, typeof(string));
        var adapter = new DynaBeeObjectMethodAdapter(
            "Coverage",
            "Adapter",
            "Echo",
            new[] { typeof(string) },
            typeof(string),
            arguments => arguments[0]?.ToString() ?? "none");

        parameterTypes[0] = typeof(int);

        Assert.Equal(typeof(string), descriptor.DeclaringType);
        Assert.Equal("Format", descriptor.Name);
        Assert.Equal(new[] { typeof(string) }, descriptor.ParameterTypes);
        Assert.Equal(typeof(string), descriptor.ReturnType);
        Assert.Equal("none", adapter.Invoke(new object[] { null! }));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGeneratedMethodDescriptor(null!, "Format", Type.EmptyTypes, typeof(string)));
        Assert.Throws<ArgumentException>(() => new DynaBeeGeneratedMethodDescriptor(typeof(string), "", Type.EmptyTypes, typeof(string)));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGeneratedMethodDescriptor(typeof(string), "Format", null!, typeof(string)));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeGeneratedMethodDescriptor(typeof(string), "Format", Type.EmptyTypes, null!));
    }

    private static IAssemblyContext CreateContext()
        => new BeeAssemblyBuilder($"DynaBee.Tests.InvokerExpansion.{Guid.NewGuid():N}")
            .AddClass("Dispatch", type => type
                .AddMethod("Add", typeof(int), method => method
                    .WithParameter<int>("left")
                    .WithParameter<int>("right")
                    .EmitsBody(body => body.Return(body.Add(body.Parameter<int>("left"), body.Parameter<int>("right")))))
                .AddMethod("Sum", typeof(int), method => method
                    .WithParameter<int>("first")
                    .WithParameter<int>("second")
                    .WithParameter<int>("third")
                    .EmitsBody(body => body.Return(body.Add(body.Add(body.Parameter<int>("first"), body.Parameter<int>("second")), body.Parameter<int>("third"))))))
            .Build();

    private static void DefineConstructor(TypeBuilder typeBuilder)
    {
        var constructor = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            new[] { typeof(int) });
        var il = constructor.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Ret);
    }
}

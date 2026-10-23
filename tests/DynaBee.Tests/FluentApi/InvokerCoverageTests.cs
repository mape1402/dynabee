namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;
using DynaBee.FluentApi.Invocation;
using System.Reflection;
using System.Reflection.Emit;

public sealed class InvokerCoverageTests
{
    [Fact]
    public void Typed_Bound_Invokers_Execute_One_Two_And_Three_Argument_Methods()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");

        var echo = context.CreateBoundMethodInvoker<string, string>("Dispatch", instance, "Echo");
        var add = context.CreateBoundMethodInvoker<int, int, int>("Dispatch", instance, "Add");
        var sum = context.CreateBoundMethodInvoker<int, int, int, int>("Dispatch", instance, "Sum");

        Assert.Equal("value", echo("value"));
        Assert.Equal(5, add(2, 3));
        Assert.Equal(6, sum(1, 2, 3));
    }

    [Fact]
    public void Bound_Open_And_Factory_Delegates_Execute_Without_Reflection_Invoke()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");

        var bound = context.CreateBoundDelegate<Func<string, string>>(
            "Dispatch", instance, "Echo", new[] { typeof(string) });
        var covariantBound = context.CreateBoundDelegate<Func<string, object>>(
            "Dispatch", instance, "Echo", new[] { typeof(string) });
        var discardReturn = context.CreateBoundDelegate<Action<string>>(
            "Dispatch", instance, "Echo", new[] { typeof(string) });
        var voidBound = context.CreateBoundDelegate<Action<string>>(
            "Dispatch", instance, "Void", new[] { typeof(string) });
        var open = context.CreateOpenDelegate<Func<object, string, string>>(
            "Dispatch", "Echo", new[] { typeof(string) });
        var openVoid = context.CreateOpenDelegate<Action<object, string>>(
            "Dispatch", "Echo", new[] { typeof(string) });
        var factory = context.CreateFactoryDelegate<Func<object>>("Factory", Type.EmptyTypes);
        var typedFactory = context.CreateFactoryDelegate(typeof(Func<object>), "Factory", Type.EmptyTypes);

        Assert.Equal("bound", bound("bound"));
        Assert.Equal("covariant", covariantBound("covariant"));
        discardReturn("ignored");
        voidBound("bound-void");
        Assert.Equal("bound-void", DispatchRecorder.LastValue);
        Assert.Equal("open", open(instance, "open"));
        openVoid(instance, "ignored");
        Assert.Equal(context.GetClrType("Factory"), factory().GetType());
        Assert.Equal(context.GetClrType("Factory"), ((Func<object>)typedFactory)().GetType());
    }

    [Fact]
    public void Object_Adapters_Support_Void_Returns_Nullable_Arguments_And_Validation()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");

        var nullable = context.CreateObjectAdapter(
            "Dispatch", instance, "Nullable", new[] { typeof(int?) });
        var voidAdapter = context.CreateObjectAdapter(
            "Dispatch", instance, "Void", new[] { typeof(string) });
        var arguments = context.CreateArgumentListAdapter(
            "Dispatch", instance, "Nullable", new[] { typeof(int?) });

        Assert.Equal("none", nullable(null!));
        Assert.Equal("7", nullable(7));
        Assert.Null(voidAdapter("recorded"));
        Assert.Equal("recorded", DispatchRecorder.LastValue);
        Assert.Equal("none", arguments.Invoke(new object[] { null! }));
        Assert.Throws<InvalidOperationException>(() => arguments.Invoke(new object[] { "wrong" }));
        Assert.Throws<InvalidOperationException>(() => arguments.Invoke(Array.Empty<object>()));
    }

    [Fact]
    public void Invoker_APIs_Report_Clear_Errors_For_Invalid_Requests()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");

        Assert.Throws<ArgumentNullException>(() => AssemblyContextInvokerExtensions.CreateMethodInvoker(null!, "Dispatch", "Echo", new[] { typeof(string) }));
        Assert.Throws<ArgumentNullException>(() => AssemblyContextInvokerExtensions.CreateFactoryDelegate(null!, typeof(Func<object>), "Factory", Type.EmptyTypes));
        Assert.Throws<ArgumentException>(() => context.CreateMethodInvoker("", "Echo", new[] { typeof(string) }));
        Assert.Throws<ArgumentException>(() => context.CreateMethodInvoker("Dispatch", "", new[] { typeof(string) }));
        Assert.Throws<ArgumentNullException>(() => context.CreateMethodInvoker("Dispatch", "Echo", null!));
        Assert.Contains("was not found", Assert.Throws<InvalidOperationException>(() => context.CreateMethodInvoker("Missing", "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("was not found", Assert.Throws<InvalidOperationException>(() => context.CreateMethodInvoker("Dispatch", "Missing", Type.EmptyTypes)).Message);
        Assert.Contains("No overload", Assert.Throws<InvalidOperationException>(() => context.CreateMethodInvoker("Dispatch", "Echo", Type.EmptyTypes)).Message);
        Assert.Contains("Target instance", Assert.Throws<InvalidOperationException>(() => context.CreateBoundMethodInvoker("Dispatch", new object(), "Echo", new[] { typeof(string) })).Message);
        Assert.Throws<ArgumentNullException>(() => context.CreateBoundMethodInvoker("Dispatch", null!, "Echo", new[] { typeof(string) }));
        Assert.Throws<ArgumentNullException>(() => context.CreateObjectAdapter("Dispatch", instance, "Echo", null!));
        Assert.Throws<ArgumentNullException>(() => AssemblyContextInvokerExtensions.CreateObjectAdapter(null!, "Dispatch", instance, "Echo", new[] { typeof(string) }));
        Assert.Contains("exactly one", Assert.Throws<InvalidOperationException>(() => context.CreateObjectAdapter("Dispatch", instance, "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Contains("exactly 2", Assert.Throws<InvalidOperationException>(() => context.CreateObjectAdapter2("Dispatch", instance, "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("exactly 3", Assert.Throws<InvalidOperationException>(() => context.CreateObjectAdapter3("Dispatch", instance, "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Contains("not a delegate", Assert.Throws<InvalidOperationException>(() => context.CreateBoundDelegate(typeof(string), "Dispatch", instance, "Echo", new[] { typeof(string) })).Message);
        Assert.Throws<ArgumentNullException>(() => context.CreateBoundDelegate(null!, "Dispatch", instance, "Echo", new[] { typeof(string) }));
        Assert.Contains("parameters do not match", Assert.Throws<InvalidOperationException>(() => context.CreateBoundDelegate<Func<int, string>>("Dispatch", instance, "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("not assignable", Assert.Throws<InvalidOperationException>(() => context.CreateBoundDelegate<Func<string, int>>("Dispatch", instance, "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("method returns void", Assert.Throws<InvalidOperationException>(() => context.CreateBoundDelegate<Func<string, int>>("Dispatch", instance, "Void", new[] { typeof(string) })).Message);
        Assert.Contains("must declare one target", Assert.Throws<InvalidOperationException>(() => context.CreateOpenDelegate<Func<string, string>>("Dispatch", "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("not compatible", Assert.Throws<InvalidOperationException>(() => context.CreateOpenDelegate<Func<IDisposable, string, string>>("Dispatch", "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("not assignable", Assert.Throws<InvalidOperationException>(() => context.CreateOpenDelegate<Func<object, string, int>>("Dispatch", "Void", new[] { typeof(string) })).Message);
        Assert.Contains("No constructor", Assert.Throws<InvalidOperationException>(() => context.CreateFactoryDelegate<Func<object, object>>("Factory", new[] { typeof(object) })).Message);
        Assert.Contains("not assignable", Assert.Throws<InvalidOperationException>(() => context.CreateFactoryDelegate<Func<string>>("Factory", Type.EmptyTypes)).Message);
        Assert.Throws<ArgumentNullException>(() => context.CreateFactoryDelegate(typeof(Func<object>), "Factory", null!));
        Assert.Contains("was not found", Assert.Throws<InvalidOperationException>(() => context.GetGeneratedMethodDescriptors(null!)).Message);
        Assert.Throws<ArgumentNullException>(() => AssemblyContextInvokerExtensions.GetGeneratedMethodDescriptors(null!, "Dispatch"));
    }

    [Fact]
    public void Object_Adapter_Arity_Errors_Keep_Diagnostics_When_Context_Is_Missing()
    {
        Assert.Contains("Assembly=''", Assert.Throws<InvalidOperationException>(() =>
            AssemblyContextInvokerExtensions.CreateObjectAdapter(null!, "Dispatch", new object(), "Echo", Type.EmptyTypes)).Message);
        Assert.Contains("Assembly=''", Assert.Throws<InvalidOperationException>(() =>
            AssemblyContextInvokerExtensions.CreateObjectAdapter2(null!, "Dispatch", new object(), "Echo", Type.EmptyTypes)).Message);
        Assert.Contains("Assembly=''", Assert.Throws<InvalidOperationException>(() =>
            AssemblyContextInvokerExtensions.CreateObjectAdapter3(null!, "Dispatch", new object(), "Echo", Type.EmptyTypes)).Message);
    }

    [Fact]
    public void Invoker_Keys_Use_Type_Method_And_Parameter_Signatures_For_Equality()
    {
        var first = InvokerKey.Create("Dispatch", "Echo", new[] { typeof(string) });
        var same = InvokerKey.Create("Dispatch", "Echo", new[] { typeof(string) });
        var differentType = InvokerKey.Create("Other", "Echo", new[] { typeof(string) });
        var differentMethod = InvokerKey.Create("Dispatch", "Other", new[] { typeof(string) });
        var differentParameters = InvokerKey.Create("Dispatch", "Echo", new[] { typeof(int) });

        Assert.True(first.Equals(same));
        Assert.True(first.Equals((object)same));
        Assert.False(first.Equals(differentType));
        Assert.False(first.Equals(differentMethod));
        Assert.False(first.Equals(differentParameters));
        Assert.False(first.Equals("not-a-key"));
        Assert.Equal(first.GetHashCode(), same.GetHashCode());
    }

    [Fact]
    public void Invoker_Runtime_Validation_And_Dispatch_Adapters_Handle_Every_Supported_Result_Shape()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");
        var nullableInvoker = context.CreateMethodInvoker("Dispatch", "Nullable", new[] { typeof(int?) });
        var voidInvoker = context.CreateMethodInvoker("Dispatch", "Void", new[] { typeof(string) });

        Assert.Equal("none", nullableInvoker.Invoke(instance, new object[] { null! }));
        Assert.Null(voidInvoker.Invoke(instance, new object[] { "recorded" }));
        Assert.Throws<ArgumentNullException>(() => nullableInvoker.Invoke(instance, null!));
        Assert.Contains("Argument count mismatch", Assert.Throws<InvalidOperationException>(() => nullableInvoker.Invoke(instance, Array.Empty<object>())).Message);
        Assert.Contains("cannot be assigned", Assert.Throws<InvalidOperationException>(() => nullableInvoker.Invoke(instance, new object[] { "wrong" })).Message);
        Assert.Contains("Target instance cannot be null", Assert.Throws<InvalidOperationException>(() => nullableInvoker.Invoke(null!, new object[] { null! })).Message);
        Assert.Contains("not assignable", Assert.Throws<InvalidOperationException>(() => nullableInvoker.Invoke(new object(), new object[] { null! })).Message);

        var virtualMethod = typeof(DispatchTarget).GetMethod(nameof(DispatchTarget.VirtualValue))!;
        var voidMethod = typeof(DispatchTarget).GetMethod(nameof(DispatchTarget.Record))!;
        var virtualDispatch = DynaBeeMethodDispatchFactory.Create(typeof(DispatchTarget), virtualMethod);
        var voidDispatch = DynaBeeMethodDispatchFactory.Create(typeof(DispatchTarget), voidMethod);
        var target = new DispatchTarget();

        Assert.Equal(4, virtualDispatch(target, new object[] { 3 }));
        Assert.Null(voidDispatch(target, new object[] { "done" }));
        Assert.Equal("done", target.LastValue);

        var adapter = new DynaBeeObjectMethodAdapter(
            "Coverage",
            "Dispatch",
            "Fail",
            new[] { typeof(int) },
            typeof(void),
            _ => throw new InvalidCastException("invalid"));
        Assert.Throws<ArgumentNullException>(() => adapter.Invoke(null!));
        Assert.Contains("Argument count mismatch", Assert.Throws<InvalidOperationException>(() => adapter.Invoke(Array.Empty<object>())).Message);
        Assert.Contains("cannot be assigned", Assert.Throws<InvalidOperationException>(() => adapter.Invoke(new object[] { "wrong" })).Message);

        Assert.Equal(string.Empty, AssemblyContextInvokerExtensions.FormatTypes(null!));
        Assert.Equal("<null>, System.Int32", AssemblyContextInvokerExtensions.FormatTypes(new Type[] { null!, typeof(int) }));
    }

    [Fact]
    public void Invocation_Adapters_Validate_Nullable_List_And_Internal_Failure_Shapes()
    {
        Assert.Throws<ArgumentNullException>(() => new DynaBeeObjectMethodAdapter("Assembly", "Type", "Method", null!, typeof(void), _ => null!));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeObjectMethodAdapter("Assembly", "Type", "Method", Type.EmptyTypes, null!, _ => null!));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeObjectMethodAdapter("Assembly", "Type", "Method", Type.EmptyTypes, typeof(void), null!));
        Assert.Throws<ArgumentNullException>(() => new DynaBeeBoundMethodInvoker("Type", "Method", null!, new object()));
        Assert.Throws<ArgumentNullException>(() => new GeneratedMethodResolution(null!, typeof(object).GetMethod(nameof(object.ToString))!));
        Assert.Throws<ArgumentNullException>(() => new GeneratedMethodResolution(CreateContext().Find("Dispatch"), null!));

        var nullableFailure = new DynaBeeObjectMethodAdapter(
            "Assembly", "Type", "Nullable", new[] { typeof(int?) }, typeof(void),
            _ => throw new NullReferenceException("failure"));
        var referenceFailure = new DynaBeeObjectMethodAdapter(
            "Assembly", "Type", "Reference", new[] { typeof(string) }, typeof(void),
            _ => throw new NullReferenceException("failure"));

        Assert.Contains("adapter invocation failed", Assert.Throws<InvalidOperationException>(() => nullableFailure.Invoke(new List<object> { 3 })).Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("adapter invocation failed", Assert.Throws<InvalidOperationException>(() => referenceFailure.Invoke(new List<object> { "value" })).Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Invocation_Delegates_Support_Exact_Runtime_Factories_And_Void_Open_Methods()
    {
        var context = CreateContext();
        var dispatch = context.GetClrType("Dispatch");
        var factoryType = context.GetClrType("Factory");
        var instance = context.CreateInstance("Dispatch");
        var exactFactoryDelegateType = typeof(Func<>).MakeGenericType(factoryType);

        var exactFactory = (Delegate)context.CreateFactoryDelegate(exactFactoryDelegateType, "Factory", Type.EmptyTypes);
        var voidOpen = context.CreateOpenDelegate<Action<object, string>>(
            "Dispatch", "Void", new[] { typeof(string) });
        var convertedOpen = context.CreateOpenDelegate<Func<object, string, object>>(
            "Dispatch", "Echo", new[] { typeof(string) });

        Assert.Equal(factoryType, exactFactory.DynamicInvoke()!.GetType());
        voidOpen(instance, "open-void");
        Assert.Equal("open-void", DispatchRecorder.LastValue);
        Assert.Equal("converted", convertedOpen(instance, "converted"));
        Assert.True(dispatch.IsInstanceOfType(instance));
    }

    [Fact]
    public void Invocation_Adapters_Support_Multiple_Arguments_List_Inputs_And_Null_Value_Errors()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");

        var twoArguments = context.CreateObjectAdapter2(
            "Dispatch", instance, "Add", new[] { typeof(int), typeof(int) });
        var threeArguments = context.CreateObjectAdapter3(
            "Dispatch", instance, "Sum", new[] { typeof(int), typeof(int), typeof(int) });
        var listAdapter = context.CreateArgumentListAdapter(
            "Dispatch", instance, "Add", new[] { typeof(int), typeof(int) });
        var bound = context.CreateBoundMethodInvoker(
            "Dispatch", instance, "Add", new[] { typeof(int), typeof(int) });
        var increment = context.CreateObjectAdapter(
            "Dispatch", instance, "Increment", new[] { typeof(int) });

        Assert.Equal(5, twoArguments(2, 3));
        Assert.Equal(6, threeArguments(1, 2, 3));
        Assert.Equal(9, listAdapter.Invoke(new List<object> { 4, 5 }));
        Assert.Equal(13, bound.Invoke(new List<object> { 6, 7 }));
        Assert.Contains("received '<null>'", Assert.Throws<InvalidOperationException>(() => increment(null!)).Message);
        Assert.Contains("received '<null>'", Assert.Throws<InvalidOperationException>(() =>
            context.CreateMethodInvoker("Dispatch", "Add", new[] { typeof(int), typeof(int) })
                .Invoke(instance, new List<object> { null!, 1 })).Message);

        var failingAdapter = new DynaBeeObjectMethodAdapter(
            "Coverage", "Dispatch", "Increment", new[] { typeof(int) }, typeof(int),
            _ => throw new InvalidCastException("invalid"));
        Assert.Contains("received '<null>'", Assert.Throws<InvalidOperationException>(() =>
            failingAdapter.Invoke(new object[] { null! })).Message);
    }

    [Fact]
    public void Runtime_Delegates_Without_Parameter_Metadata_Are_Supported()
    {
        var context = CreateContext();
        var instance = context.CreateInstance("Dispatch");
        var factoryDelegateType = CreateUnnamedDelegateType(typeof(object));
        var boundDelegateType = CreateUnnamedDelegateType(typeof(int), typeof(int));
        var openDelegateType = CreateUnnamedDelegateType(typeof(int), typeof(object), typeof(int));

        var factory = (Delegate)context.CreateFactoryDelegate(factoryDelegateType, "Factory", Type.EmptyTypes);
        var bound = (Delegate)context.CreateBoundDelegate(
            boundDelegateType, "Dispatch", instance, "Increment", new[] { typeof(int) });
        var open = CreateOpenDelegate(context, openDelegateType, "Dispatch", "Increment", new[] { typeof(int) });

        Assert.All(factoryDelegateType.GetMethod(nameof(Action.Invoke))!.GetParameters(), parameter => Assert.Null(parameter.Name));
        Assert.All(boundDelegateType.GetMethod(nameof(Action.Invoke))!.GetParameters(), parameter => Assert.Null(parameter.Name));
        Assert.Equal(context.GetClrType("Factory"), factory.DynamicInvoke()!.GetType());
        Assert.Equal(3, bound.DynamicInvoke(2));
        Assert.Equal(4, open.DynamicInvoke(instance, 3));
    }

    private static IAssemblyContext CreateContext()
    {
        DispatchRecorder.LastValue = null;
        return new BeeAssemblyBuilder($"DynaBee.Tests.InvokerCoverage.{Guid.NewGuid():N}")
            .AddClass("Dispatch", type => type
                .AddMethod("Echo", typeof(string), method => method
                    .WithParameter<string>("value")
                    .EmitsBody(body => body.Return(body.Parameter<string>("value"))))
                .AddMethod("Add", typeof(int), method => method
                    .WithParameter<int>("left")
                    .WithParameter<int>("right")
                    .EmitsBody(body => body.Return(body.Add(body.Parameter<int>("left"), body.Parameter<int>("right")))))
                .AddMethod("Increment", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsBody(body => body.Return(body.Add(body.Parameter<int>("value"), body.Constant(1)))))
                .AddMethod("Sum", typeof(int), method => method
                    .WithParameter<int>("first")
                    .WithParameter<int>("second")
                    .WithParameter<int>("third")
                    .EmitsBody(body => body.Return(body.Add(
                        body.Add(body.Parameter<int>("first"), body.Parameter<int>("second")),
                        body.Parameter<int>("third")))))
                .AddMethod("Nullable", typeof(string), method => method
                    .WithParameter<int?>("value")
                    .EmitsBody(body => body.Return(body.If(
                        body.IsNull(body.Parameter<int?>("value")),
                        body.Constant("none"),
                        body.StaticCall(typeof(Convert), nameof(Convert.ToString), new[] { typeof(int) }, body.Convert<int>(body.Parameter<int?>("value")))))))
                .AddVoidMethod("Void", method => method
                    .WithParameter<string>("value")
                    .EmitsBody(body => body.Evaluate(body.StaticCall(
                        typeof(DispatchRecorder),
                        nameof(DispatchRecorder.Record),
                        new[] { typeof(string) },
                        body.Parameter<string>("value"))))))
            .AddClass("Factory", type => type.AddConstructor())
            .Build();
    }

    private static Delegate CreateOpenDelegate(
        IAssemblyContext context,
        Type delegateType,
        string typeName,
        string methodName,
        IReadOnlyList<Type> parameterTypes)
    {
        var method = typeof(AssemblyContextInvokerExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(candidate => candidate.Name == nameof(AssemblyContextInvokerExtensions.CreateOpenDelegate)
                && candidate.IsGenericMethodDefinition);

        return (Delegate)method.MakeGenericMethod(delegateType)
            .Invoke(null, new object[] { context, typeName, methodName, parameterTypes })!;
    }

    private static Type CreateUnnamedDelegateType(Type returnType, params Type[] parameterTypes)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"DynaBee.Tests.UnnamedDelegate.{Guid.NewGuid():N}"),
            AssemblyBuilderAccess.Run);
        var builder = assembly.DefineDynamicModule("Delegates")
            .DefineType(
                $"UnnamedDelegate{Guid.NewGuid():N}",
                TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
                typeof(MulticastDelegate));
        var constructor = builder.DefineConstructor(
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName,
            CallingConventions.Standard,
            new[] { typeof(object), typeof(IntPtr) });
        constructor.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
        var invoke = builder.DefineMethod(
            nameof(Action.Invoke),
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
            returnType,
            parameterTypes);
        invoke.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
        return builder.CreateTypeInfo()!.AsType();
    }

    public static class DispatchRecorder
    {
        public static string LastValue { get; set; }

        public static void Record(string value)
            => LastValue = value;
    }

    public class DispatchTarget
    {
        public string LastValue { get; private set; }

        public virtual int VirtualValue(int value)
            => value + 1;

        public void Record(string value)
            => LastValue = value;
    }
}

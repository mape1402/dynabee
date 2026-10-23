namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;
using DynaBee.FluentApi.Invocation;

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
        var discardReturn = context.CreateBoundDelegate<Action<string>>(
            "Dispatch", instance, "Echo", new[] { typeof(string) });
        var open = context.CreateOpenDelegate<Func<object, string, string>>(
            "Dispatch", "Echo", new[] { typeof(string) });
        var openVoid = context.CreateOpenDelegate<Action<object, string>>(
            "Dispatch", "Echo", new[] { typeof(string) });
        var factory = context.CreateFactoryDelegate<Func<object>>("Factory", Type.EmptyTypes);
        var typedFactory = context.CreateFactoryDelegate(typeof(Func<object>), "Factory", Type.EmptyTypes);

        Assert.Equal("bound", bound("bound"));
        discardReturn("ignored");
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
        Assert.Throws<ArgumentException>(() => context.CreateMethodInvoker("", "Echo", new[] { typeof(string) }));
        Assert.Throws<ArgumentException>(() => context.CreateMethodInvoker("Dispatch", "", new[] { typeof(string) }));
        Assert.Throws<ArgumentNullException>(() => context.CreateMethodInvoker("Dispatch", "Echo", null!));
        Assert.Contains("was not found", Assert.Throws<InvalidOperationException>(() => context.CreateMethodInvoker("Missing", "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("was not found", Assert.Throws<InvalidOperationException>(() => context.CreateMethodInvoker("Dispatch", "Missing", Type.EmptyTypes)).Message);
        Assert.Contains("No overload", Assert.Throws<InvalidOperationException>(() => context.CreateMethodInvoker("Dispatch", "Echo", Type.EmptyTypes)).Message);
        Assert.Contains("Target instance", Assert.Throws<InvalidOperationException>(() => context.CreateBoundMethodInvoker("Dispatch", new object(), "Echo", new[] { typeof(string) })).Message);
        Assert.Throws<ArgumentNullException>(() => context.CreateBoundMethodInvoker("Dispatch", null!, "Echo", new[] { typeof(string) }));
        Assert.Contains("exactly one", Assert.Throws<InvalidOperationException>(() => context.CreateObjectAdapter("Dispatch", instance, "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Contains("exactly 2", Assert.Throws<InvalidOperationException>(() => context.CreateObjectAdapter2("Dispatch", instance, "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("exactly 3", Assert.Throws<InvalidOperationException>(() => context.CreateObjectAdapter3("Dispatch", instance, "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Contains("not a delegate", Assert.Throws<InvalidOperationException>(() => context.CreateBoundDelegate(typeof(string), "Dispatch", instance, "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("parameters do not match", Assert.Throws<InvalidOperationException>(() => context.CreateBoundDelegate<Func<int, string>>("Dispatch", instance, "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("not assignable", Assert.Throws<InvalidOperationException>(() => context.CreateBoundDelegate<Func<string, int>>("Dispatch", instance, "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("must declare one target", Assert.Throws<InvalidOperationException>(() => context.CreateOpenDelegate<Func<string, string>>("Dispatch", "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("not compatible", Assert.Throws<InvalidOperationException>(() => context.CreateOpenDelegate<Func<IDisposable, string, string>>("Dispatch", "Echo", new[] { typeof(string) })).Message);
        Assert.Contains("No constructor", Assert.Throws<InvalidOperationException>(() => context.CreateFactoryDelegate<Func<object, object>>("Factory", new[] { typeof(object) })).Message);
        Assert.Contains("not assignable", Assert.Throws<InvalidOperationException>(() => context.CreateFactoryDelegate<Func<string>>("Factory", Type.EmptyTypes)).Message);
        Assert.Throws<ArgumentNullException>(() => context.CreateFactoryDelegate(typeof(Func<object>), "Factory", null!));
        Assert.Contains("was not found", Assert.Throws<InvalidOperationException>(() => context.GetGeneratedMethodDescriptors(null!)).Message);
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

    public static class DispatchRecorder
    {
        public static string LastValue { get; set; }

        public static void Record(string value)
            => LastValue = value;
    }
}

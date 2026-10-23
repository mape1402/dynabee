namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;
using DynaBee.FluentApi.Invocation;

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
        Assert.Contains("parameter 1", Assert.Throws<InvalidOperationException>(() => context.CreateOpenDelegate<Func<object, string, int, int>>("Dispatch", "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Contains("return type", Assert.Throws<InvalidOperationException>(() => context.CreateOpenDelegate<Func<object, int, int, string>>("Dispatch", "Add", new[] { typeof(int), typeof(int) })).Message);
        Assert.Throws<ArgumentNullException>(() => context.CreateObjectAdapter2("Dispatch", instance, "Add", null!));
        Assert.Throws<ArgumentNullException>(() => context.CreateObjectAdapter3("Dispatch", instance, "Sum", null!));
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
}

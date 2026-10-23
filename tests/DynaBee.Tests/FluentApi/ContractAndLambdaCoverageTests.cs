namespace DynaBee.Tests.FluentApi;

using System.Reflection;
using DynaBee.FluentApi;

public sealed class ContractAndLambdaCoverageTests
{
    [Fact]
    public void InterfaceContracts_Implement_Overloads_Properties_And_Events()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Contract", type => type
                .ImplementInterface<IContract>(contract => contract
                    .RequireExplicitImplementations()
                    .Property(nameof(IContract.Value), property => property.WithGetter().WithSetter())
                    .Event(nameof(IContract.Changed), _ => { })
                    .Method(nameof(IContract.Format), new[] { typeof(int) }, method => method
                        .EmitsBody(body => body.Return(body.Convert<string>(body.Parameter<int>("value")))))
                    .Method(nameof(IContract.Format), new[] { typeof(string) }, method => method
                        .EmitsBody(body => body.Return(body.Concat(body.Constant("text:"), body.Parameter<string>("value")))))))
            .Build();

        var value = (IContract)context.CreateInstance("Contract");
        value.Value = 8;
        value.Changed += (_, _) => { };

        Assert.Equal(8, value.Value);
        Assert.Equal("8", value.Format(8));
        Assert.Equal("text:bee", value.Format("bee"));
        Assert.Contains(typeof(IContract), value.GetType().GetInterfaces());
    }

    [Fact]
    public void InterfaceContracts_Reject_Ambiguous_Missing_Duplicate_And_Incomplete_Configuration()
    {
        Assert.Throws<InvalidOperationException>(() => AddContract(contract => contract.RequireExplicitImplementations()));
        Assert.Throws<ArgumentException>(() => AddContract(contract => contract.Method("", _ => { })));
        Assert.Throws<MissingMethodException>(() => AddContract(contract => contract.Method("Missing", _ => { })));
        Assert.Throws<InvalidOperationException>(() => AddContract(contract => contract.Method(nameof(IContract.Format), _ => { })));
        Assert.Throws<ArgumentNullException>(() => AddContract(contract => contract.Method(nameof(IContract.Format), null!, _ => { })));
        Assert.Throws<MissingMethodException>(() => AddContract(contract => contract.Method(nameof(IContract.Format), new[] { typeof(decimal) }, _ => { })));
        Assert.Throws<ArgumentNullException>(() => AddContract(contract => contract.Method(nameof(IContract.Format), new[] { typeof(int) }, null!)));
        Assert.Throws<ArgumentException>(() => AddContract(contract => contract.Property("", _ => { })));
        Assert.Throws<MissingMemberException>(() => AddContract(contract => contract.Property("Missing", _ => { })));
        Assert.Throws<ArgumentNullException>(() => AddContract(contract => contract.Property(nameof(IContract.Value), null!)));
        Assert.Throws<InvalidOperationException>(() => AddContract(contract => contract
            .Property(nameof(IContract.Value), _ => { })
            .Property(nameof(IContract.Value), _ => { })));
        Assert.Throws<ArgumentException>(() => AddContract(contract => contract.Event("", _ => { })));
        Assert.Throws<MissingMemberException>(() => AddContract(contract => contract.Event("Missing", _ => { })));
        Assert.Throws<ArgumentNullException>(() => AddContract(contract => contract.Event(nameof(IContract.Changed), null!)));
        Assert.Throws<InvalidOperationException>(() => AddContract(contract => contract
            .Event(nameof(IContract.Changed), _ => { })
            .Event(nameof(IContract.Changed), _ => { })));
        Assert.Throws<NotSupportedException>(() => new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("Indexed", type => type.ImplementInterface<IMultiIndex>())
            .Build());
    }

    [Fact]
    public void Lambda_Overloads_With_Self_And_Injected_Dependency_Execute_Through_Generated_Methods()
    {
        var context = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("LambdaHost", type => type
                .Implements<IValueHost>()
                .AddAutoProperty<int>(nameof(IValueHost.Value))
                .Inject<OffsetDependency>("Offset")
                .AddMethod("One", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsLambdaWithSelf<IValueHost, int, int>((self, value) => self.Value + value))
                .AddMethod("Two", typeof(int), method => method
                    .WithParameter<int>("left")
                    .WithParameter<int>("right")
                    .EmitsLambdaWithSelf<IValueHost, int, int, int>((self, left, right) => self.Value + left + right))
                .AddMethod("Injected", typeof(int), method => method
                    .WithParameter<int>("value")
                    .EmitsInjectedLambda<OffsetDependency, int, int>("Offset", (dependency, value) => dependency.Add(value)))
                .AddMethod("Tagged", typeof(int), method => method
                    .AddAttribute<ObsoleteAttribute>("generated")
                    .WithMetadata("coverage.method", "tagged")
                    .EmitsBody(body => body.Return(body.Constant(1)))))
            .Build();

        var instance = context.CreateInstance("LambdaHost", new OffsetDependency(4));
        ((IValueHost)instance).Value = 3;
        var type = instance.GetType();

        Assert.Equal(8, type.GetMethod("One")!.Invoke(instance, new object[] { 5 }));
        Assert.Equal(10, type.GetMethod("Two")!.Invoke(instance, new object[] { 2, 5 }));
        Assert.Equal(10, type.GetMethod("Injected")!.Invoke(instance, new object[] { 6 }));
        Assert.Equal("generated", type.GetMethod("Tagged")!.GetCustomAttribute<ObsoleteAttribute>()!.Message);
        Assert.True(context.Find("LambdaHost").FindOne("Tagged").TryGetMetadata("coverage.method", out var metadata));
        Assert.Equal("tagged", metadata);
    }

    private static void AddContract(Action<BeeInterfaceContractBuilder> configure)
    {
        _ = new BeeAssemblyBuilder(NewAssemblyName())
            .AddClass("InvalidContract", type => type.ImplementInterface<IContract>(configure));
    }

    private static string NewAssemblyName()
        => $"DynaBee.Tests.ContractCoverage.{Guid.NewGuid():N}";

    public interface IContract
    {
        int Value { get; set; }

        event EventHandler Changed;

        string Format(int value);

        string Format(string value);
    }

    public interface IMultiIndex
    {
        int this[int first, int second] { get; }
    }

    public interface IValueHost
    {
        int Value { get; set; }
    }

    public sealed class OffsetDependency
    {
        private readonly int _offset;

        public OffsetDependency(int offset)
        {
            _offset = offset;
        }

        public int Add(int value)
            => value + _offset;
    }
}

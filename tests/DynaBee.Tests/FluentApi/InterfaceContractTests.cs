namespace DynaBee.Tests.FluentApi;

using DynaBee.FluentApi;
using DynaBee.FluentApi.Generation;
using System.Linq.Expressions;

public sealed class InterfaceContractTests
{
    [Fact]
    public void ImplementInterface_Generates_Properties_And_Overloaded_Methods()
    {
        var context = new BeeAssemblyBuilder("DynaBee.Tests.InterfaceContract")
            .AddClass("ContractImplementation", type => type
                .ImplementInterface<IContract>(contract => contract
                    .RequireExplicitImplementations()
                    .Property(nameof(IContract.Name), property => property.WithGetter().WithSetter())
                    .Method(nameof(IContract.Add), method => method
                        .EmitsExpression((Expression<Func<int, int, int>>)((left, right) => left + right)))
                    .Method(nameof(IContract.Format), new[] { typeof(int) }, method => method
                        .EmitsExpression((Expression<Func<int, string>>)(value => "int:" + value)))
                    .Method(nameof(IContract.Format), new[] { typeof(decimal) }, method => method
                        .EmitsExpression((Expression<Func<decimal, string>>)(value => "decimal:" + value)))))
            .Build();

        var value = context.CreateInstance<IContract>("ContractImplementation");
        value.Name = "DynaBee";

        Assert.Equal("DynaBee", value.Name);
        Assert.Equal(7, value.Add(3, 4));
        Assert.Equal("int:5", value.Format(5));
        Assert.StartsWith("decimal:2.5", value.Format(2.5m));
    }

    [Fact]
    public void ImplementInterface_Requires_Every_Member_When_Requested()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new BeeAssemblyBuilder("DynaBee.Tests.InterfaceContract.Missing")
                .AddClass("MissingContractMember", type => type
                    .ImplementInterface<IContract>(contract => contract
                        .RequireExplicitImplementations()
                        .Method(nameof(IContract.Add), method => method.EmitsBody(body => body.Return(body.Constant(0)))))));

        Assert.Contains(nameof(IContract.Name), exception.Message);
        Assert.Contains(nameof(IContract.Format), exception.Message);
    }

    [Fact]
    public void ImplementInterface_Rejects_Ambiguous_Overload_Selection()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new BeeAssemblyBuilder("DynaBee.Tests.InterfaceContract.Ambiguous")
                .AddClass("AmbiguousContractMember", type => type
                    .ImplementInterface<IContract>(contract => contract
                        .Method(nameof(IContract.Format), method => method.EmitsBody(body => body.Return(body.Constant("value")))))));

        Assert.Contains("overloaded", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ImplementInterface_Generates_Contract_Events_And_Indexers()
    {
        var context = new BeeAssemblyBuilder("DynaBee.Tests.InterfaceContract.RuntimeMembers")
            .AddClass("ObservableStore", type => type
                .ImplementInterface<IObservableStore>(contract => contract
                    .RequireExplicitImplementations()
                    .Event(nameof(IObservableStore.Changed), _ => { })))
            .Build();
        var store = context.CreateInstance<IObservableStore>("ObservableStore");
        EventHandler handler = (_, _) => { };

        store[3] = "bee";
        store.Changed += handler;

        Assert.Equal("bee", store[3]);
        store.Changed -= handler;
    }

    [Fact]
    public void GenerationPlan_Can_Declare_An_Interface_Contract()
    {
        var plan = new DynaBeeGenerationPlan("DynaBee.Tests.InterfaceContract.Plan")
            .AddClass("PlannedCalculator", type => type
                .ImplementInterface<ICalculator>(contract => contract
                    .RequireExplicitImplementations()
                    .Property(nameof(ICalculator.Name), property => property.WithGetter().WithSetter())
                    .Method(nameof(ICalculator.Sum), method => method
                        .EmitsBody(body => body.Return(body.Add(
                            body.Parameter<int>("x"),
                            body.Parameter<int>("y")))))));
        var context = plan.ApplyTo(new BeeAssemblyBuilder(plan.AssemblyName)).Build();

        var calculator = context.CreateInstance<ICalculator>("PlannedCalculator");

        Assert.Equal(5, calculator.Sum(2, 3));
    }

    public interface IContract
    {
        string Name { get; set; }

        int Add(int left, int right);

        string Format(int value);

        string Format(decimal value);
    }

    public interface ICalculator
    {
        string Name { get; set; }

        int Sum(int x, int y);
    }

    public interface IObservableStore
    {
        event EventHandler Changed;

        string this[int key] { get; set; }
    }
}

namespace DynaBee.Tests.Testing;

using DynaBee.FluentApi;
using DynaBee.Testing;
using DynaBee.Testing.Assertions;
using DynaBee.Testing.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;

public sealed class DynaBeeTestingTests
{
    [Fact]
    public async Task GenerateAssemblyAsync_Can_Generate_Assembly_In_Memory_And_Assert_Type_Shape()
    {
        var services = new ServiceCollection()
            .AddDynaBeeTesting()
            .BuildServiceProvider();
        var dynabeeTest = services.GetRequiredService<IDynaBeeTestGenerator>();
        var specification = DynaBeeTestSpecification.Create("DynaBee.Testing.Tests.Generated", builder => builder
            .AddClass("CreateCustomerCommandHandler", type => type
                .Inherits(typeof(GenericCreateCommandHandler<Customer, CreateCustomerCommand, CustomerCreated, TestContext>))
                .Implements(typeof(IRequestHandler<CreateCustomerCommand, CustomerCreated>))
                .AddConstructor(ctor => ctor
                    .WithParameter<TestContext>("context")
                    .CallsBase(
                        typeof(GenericCreateCommandHandler<Customer, CreateCustomerCommand, CustomerCreated, TestContext>)
                            .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(TestContext) }, null)!,
                        "context"))
                .AddMethod("Handle", typeof(CustomerCreated), method => method
                    .WithParameter<CreateCustomerCommand>("request")
                    .EmitsExpression((Expression<Func<CreateCustomerCommand, CustomerCreated>>)(request => new CustomerCreated())))));

        var result = await dynabeeTest.GenerateAssemblyAsync(specification);
        var assembly = result.Assembly;

        result.Diagnostics.ShouldBeEmpty();
        var generatedType = assembly.ShouldContainType("CreateCustomerCommandHandler");
        generatedType.ShouldImplement(typeof(IRequestHandler<,>));
        generatedType.ShouldInheritFrom(typeof(GenericCreateCommandHandler<,,,>));
        generatedType.ShouldHaveConstructor(typeof(TestContext));
    }

    [Fact]
    public async Task AddGeneratedAssembly_Registers_Generated_Types_In_DI()
    {
        var dynabeeTest = new ServiceCollection()
            .AddDynaBeeTesting()
            .BuildServiceProvider()
            .GetRequiredService<IDynaBeeTestGenerator>();
        var specification = DynaBeeTestSpecification.Create("DynaBee.Testing.Tests.DI", builder => builder
            .AddClass("GeneratedGreeter", type => type
                .Implements<IGreeter>()
                .AddMethod(nameof(IGreeter.Greet), typeof(string), method => method
                    .WithParameter<string>("name")
                    .EmitsExpression((Expression<Func<string, string>>)(name => "Hello " + name)))));

        var result = await dynabeeTest.GenerateAssemblyAsync(specification);

        var services = new ServiceCollection();
        services.AddGeneratedAssembly(result.Assembly);
        var provider = services.BuildServiceProvider();
        var greeter = provider.GetRequiredService<IGreeter>();

        Assert.Equal("Hello Ada", greeter.Greet("Ada"));
    }

    [Fact]
    public async Task GenerateAssemblyAsync_Returns_Diagnostics_When_Generation_Fails()
    {
        var dynabeeTest = new ServiceCollection()
            .AddDynaBeeTesting()
            .BuildServiceProvider()
            .GetRequiredService<IDynaBeeTestGenerator>();
        var specification = DynaBeeTestSpecification.Create("DynaBee.Testing.Tests.Failures", builder => builder
            .AddClass("Broken", type => type
                .Inherits(typeof(BaseWithoutDefaultConstructor))
                .AddConstructor()));

        var result = await dynabeeTest.GenerateAssemblyAsync(specification);

        Assert.Null(result.Assembly);
        Assert.NotEmpty(result.Diagnostics);
        Assert.Contains("DynaBee test generation failed", result.Diagnostics[0].Message);
    }

    [Fact]
    public async Task WriteGeneratedSourcesTo_Writes_Provided_And_Diagnostic_Sources()
    {
        var dynabeeTest = new ServiceCollection()
            .AddDynaBeeTesting()
            .BuildServiceProvider()
            .GetRequiredService<IDynaBeeTestGenerator>();
        var specification = DynaBeeTestSpecification.Create("DynaBee.Testing.Tests.Sources", builder => builder
                .AddClass("GeneratedGreeter", type => type
                    .AddAutoProperty<string>("Name")))
            .AddGeneratedSource("snapshots/GeneratedGreeter.cs", "public sealed class GeneratedGreeter {}\n");
        var outputPath = Path.Combine(Path.GetTempPath(), "dynabee-testing-" + Guid.NewGuid().ToString("N"));

        var result = await dynabeeTest.GenerateAssemblyAsync(specification);
        result.WriteGeneratedSourcesTo(outputPath);

        Assert.True(File.Exists(Path.Combine(outputPath, "snapshots", "GeneratedGreeter.cs")));
        Directory.Delete(outputPath, recursive: true);
    }

    [Fact]
    public async Task WriteGeneratedSourcesTo_Can_Create_Shape_Snapshot_When_No_Source_Is_Provided()
    {
        var dynabeeTest = new ServiceCollection()
            .AddDynaBeeTesting()
            .BuildServiceProvider()
            .GetRequiredService<IDynaBeeTestGenerator>();
        var specification = DynaBeeTestSpecification.Create("DynaBee.Testing.Tests.ShapeSnapshots", builder => builder
            .AddClass("GeneratedGreeter", type => type
                .AddAutoProperty<string>("Name")));
        var outputPath = Path.Combine(Path.GetTempPath(), "dynabee-testing-" + Guid.NewGuid().ToString("N"));

        var result = await dynabeeTest.GenerateAssemblyAsync(specification);
        result.Diagnostics.ShouldBeEmpty();
        result.WriteGeneratedSourcesTo(outputPath);

        var snapshot = Path.Combine(outputPath, "GeneratedGreeter.g.cs");
        Assert.True(File.Exists(snapshot));
        Assert.Contains("GeneratedGreeter", File.ReadAllText(snapshot));
        Directory.Delete(outputPath, recursive: true);
    }

    public interface IRequestHandler<in TRequest, out TResponse>
    {
        TResponse Handle(TRequest request);
    }

    public interface IGreeter
    {
        string Greet(string name);
    }

    public abstract class GenericCreateCommandHandler<TEntity, TCommand, TResult, TContext>
    {
        protected GenericCreateCommandHandler(TContext context)
        {
            Context = context;
        }

        protected TContext Context { get; }
    }

    public sealed class Customer
    {
    }

    public sealed class CreateCustomerCommand
    {
    }

    public sealed class CustomerCreated
    {
    }

    public sealed class TestContext
    {
    }

    public abstract class BaseWithoutDefaultConstructor
    {
        protected BaseWithoutDefaultConstructor(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}


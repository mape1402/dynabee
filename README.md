# DynaBee

**DynaBee** is a lightweight .NET library that uses `Reflection.Emit` to generate dynamic types at runtime.

It is designed for scenarios where you need to build classes, methods, properties, and contracts programmatically, with a fluent API that is simple to use and easy to extend.

The recommended application model is DI-first: define `DynaBeeProfile` classes, let DynaBee discover them, and resolve generated assembly contexts through `IDynaBeeAssemblyCatalog`.

## What It Solves

- Runtime type generation without producing intermediate source code.
- Fluent creation of classes, interfaces, structs, enums, and records.
- Method implementation through IL, lambdas, expression trees, or high-level method body builders.
- Dependency injection integration.
- Typed metadata for external extensions (for example EF or other frameworks).
- Assembly cache/versioning to reduce type build overhead.

## Requirements

- .NET SDK 10.0+ recommended for development.
- The library multi-targets: `net8.0`, `net9.0`, and `net10.0`.

## Installation

```bash
dotnet add package DynaBee
```

## Getting Started

### 1) Define a profile

```csharp
using DynaBee.FluentApi;
using DynaBee.FluentApi.DependencyInjection;
using System.Linq.Expressions;

public sealed class SalesProfile : DynaBeeProfile
{
    public SalesProfile() : base("Demo.Sales")
    {
    }

    public override void Configure(IBeeAssemblyBuilder builder)
    {
        builder
            .AddClass("Calculator", c => c
                .Implements<ICalculator>(registerInDi: true)
                .RegisterAsConcrete(false)
                .AddMethod(nameof(ICalculator.Sum), typeof(int), m => m
                    .WithParameter<int>("x")
                    .WithParameter<int>("y")
                    .EmitsExpression((Expression<Func<int, int, int>>)((x, y) => x + y))))
            .AddClass("InvoiceService", c => c
                .Implements<IInvoiceService>(registerInDi: true)
                .Implements<IInternalContract>(registerInDi: false)
                .RegisterAsConcrete(false)
                .Inject<IUnitOfWork>("UnitOfWork")
                .AddMethod(nameof(IInvoiceService.Commit), typeof(int), m => m
                    .EmitsInjectedLambda<IUnitOfWork, int>("UnitOfWork", uow => uow.SaveChanges())));
    }
}

public interface ICalculator
{
    int Sum(int x, int y);
}

public interface IInvoiceService
{
    int Commit();
}

public interface IInternalContract
{
    string Hidden();
}

public interface IUnitOfWork
{
    int SaveChanges();
}
```

### 2) Register DynaBee through DI

Profiles are the recommended way to organize dynamic types in larger applications.
Each profile belongs to exactly one logical dynamic assembly. DynaBee discovers
profiles, groups them by assembly name, builds each assembly context, and registers
generated types in DI.

```csharp
using DynaBee.FluentApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IUnitOfWork>(new UnitOfWork());

services.AddDynaBeeProfiles(
    ServiceLifetime.Transient,
    typeof(SalesProfile).Assembly);

var provider = services.BuildServiceProvider();
var catalog = provider.GetRequiredService<IDynaBeeAssemblyCatalog>();

var salesContext = catalog.GetContext("Demo.Sales");
var calculator = provider.GetRequiredService<ICalculator>();
var invoiceService = provider.GetRequiredService<IInvoiceService>();

var total = calculator.Sum(5, 3);        // 8
var rows = invoiceService.Commit();      // Calls IUnitOfWork.SaveChanges()

public sealed class UnitOfWork : IUnitOfWork
{
    public int SaveChanges() => 42;
}
```

### 3) Explicit registry setup

If you prefer explicit setup, `AddDynaBeeRegistry(...)` creates a single
mutable registry/provider pair for one logical dynamic assembly and registers
the initial generated types automatically.

```csharp
using DynaBee.FluentApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

var services = new ServiceCollection();

services.AddDynaBeeRegistry("Demo.Runtime", registry =>
{
    registry.Configure(builder => builder
        .AddClass("Greeter", c => c
            .AddMethod("SayHello", typeof(string), m => m
                .WithParameter<string>("name")
                .EmitsExpression((Expression<Func<string, string>>)(name => "Hello " + name)))));
});

var provider = services.BuildServiceProvider();
var context = provider.GetRequiredService<IAssemblyContext>();
```

### 4) Method body builder for mapper generation

`EmitsBody(...)` lets integrations build complete method bodies without using
IL opcodes. It supports parameters, locals, object construction, instance/static
property and field access, constants, default values, nullable checks, enum and
numeric conversions, assignments, conditionals, method calls, access to the
generated instance through `Self()`, side-effect evaluation, and returns.

```csharp
using DynaBee.FluentApi;
using DynaBee.FluentApi.DependencyInjection;
using System.Linq.Expressions;

public sealed class MappingProfile : DynaBeeProfile
{
    public MappingProfile() : base("Demo.Mapping")
    {
    }

    public override void Configure(IBeeAssemblyBuilder builder)
    {
        builder.AddClass("UserMapper", c => c
            .AddMethod("Map", typeof(UserDto), m => m
                .WithParameter<User>("source")
                .EmitsBody(body =>
                {
                    var source = body.Parameter<User>("source");
                    var destination = body.DeclareLocal<UserDto>("destination");

                    body.Assign(destination, body.New<UserDto>());
                    body.Assign(
                        body.Property(destination, nameof(UserDto.DisplayName)),
                        body.Concat(
                            body.Property(source, nameof(User.FirstName)),
                            body.Constant(" "),
                            body.Property(source, nameof(User.LastName))));
                    body.Assign(
                        body.Property(destination, nameof(UserDto.Total)),
                        body.Convert<decimal>(body.Property(source, nameof(User.Total))));
                    body.Assign(
                        body.Property(destination, nameof(UserDto.Name)),
                        body.If(
                            body.IsNull(body.Property(source, nameof(User.Name))),
                            body.Constant("Unknown"),
                            body.Property(source, nameof(User.Name))));
                    body.Return(destination);
                }));
    }
}
```

### 5) DI-based resolver method bodies

DynaBee does not own service lifetimes. Instead, generated method bodies can call
whatever service provider or resolver API your integration chooses. This keeps
framework-specific concerns outside the core library while still avoiding direct
IL in integrations.

```csharp
using DynaBee.FluentApi;
using DynaBee.FluentApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public sealed class ResolverProfile : DynaBeeProfile
{
    public ResolverProfile() : base("Demo.Resolvers")
    {
    }

    public override void Configure(IBeeAssemblyBuilder builder)
    {
        var getRequiredService = typeof(ServiceProviderServiceExtensions)
            .GetMethods()
            .Single(x => x.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService)
                && x.IsGenericMethodDefinition
                && x.GetParameters().Length == 1)
            .MakeGenericMethod(typeof(OrderTotalTextResolver));

        var resolveMethod = typeof(IValueResolver<Order, OrderDto, string>)
            .GetMethod(nameof(IValueResolver<Order, OrderDto, string>.Resolve))!;

        builder.AddClass("OrderMapper", c => c
            .AddMethod("Map", typeof(OrderDto), m => m
                .WithParameter<Order>("source")
                .WithParameter<IMapContext>("mapContext")
                .EmitsBody(body =>
                {
                    var source = body.Parameter<Order>("source");
                    var mapContext = body.Parameter<IMapContext>("mapContext");
                    var destination = body.DeclareLocal<OrderDto>("destination");
                    var resolver = body.DeclareLocal<OrderTotalTextResolver>("resolver");

                    body.Assign(destination, body.New<OrderDto>());
                    body.Assign(
                        resolver,
                        body.StaticCall(
                            getRequiredService,
                            body.Property(mapContext, nameof(IMapContext.Services))));
                    body.Assign(
                        body.Property(destination, nameof(OrderDto.TotalText)),
                        body.Call(resolver, resolveMethod, source, destination, mapContext));
                    body.Return(destination);
                })));
    }
}

public interface IMapContext
{
    IServiceProvider Services { get; }
}

public interface IValueResolver<in TSource, in TDestination, out TMember>
{
    TMember Resolve(TSource source, TDestination destination, IMapContext context);
}

public sealed class OrderTotalTextResolver
    : IValueResolver<Order, OrderDto, string>
{
    public string Resolve(Order source, OrderDto destination, IMapContext context)
        => source.Total.ToString("C");
}
```

Generated methods can also call collaborators stored on the generated instance:

```csharp
builder.AddClass("OrderMapper", c => c
    .AddAutoProperty<IOrderFormatter>("Formatter")
    .AddMethod("Format", typeof(string), m => m
        .WithParameter<Order>("source")
        .EmitsBody(body =>
        {
            var formatter = body.Property(body.Self(), "Formatter");
            var source = body.Parameter<Order>("source");
            var format = typeof(IOrderFormatter).GetMethod(nameof(IOrderFormatter.Format))!;

            body.Return(body.Call(formatter, format, source));
        })));
```

### 6) Collection mapping method bodies

Collection-oriented generated methods can use loops, ordered comparisons,
indexed access, runtime-sized arrays, and constructor calls with arguments.
This lets integrations generate array/list mapping logic without helper
delegates, reflection invocation, expression compilation, or raw IL.

```csharp
public sealed class CollectionProfile : DynaBeeProfile
{
    public CollectionProfile() : base("Demo.Collections")
    {
    }

    public override void Configure(IBeeAssemblyBuilder builder)
    {
        builder.AddClass("ArrayCopier", c => c
            .AddMethod("Copy", typeof(int[]), m => m
                .WithParameter<int[]>("source")
                .EmitsBody(body =>
                {
                    var source = body.Parameter<int[]>("source");
                    var destination = body.DeclareLocal<int[]>("destination");
                    var index = body.DeclareLocal<int>("i");

                    body.If(body.IsNull(source), whenTrue: branch =>
                    {
                        branch.Return(branch.Constant(null, typeof(int[])));
                    });

                    body.Assign(
                        destination,
                        body.NewArray<int>(body.Property(source, nameof(Array.Length))));

                    body.For(
                        initialize: loop => loop.Assign(index, loop.Constant(0)),
                        condition: loop => loop.LessThan(
                            index,
                            loop.Property(source, nameof(Array.Length))),
                        increment: loop => loop.Assign(
                            index,
                            loop.Add(index, loop.Constant(1))),
                        body: loop => loop.Assign(
                            loop.Index(destination, index),
                            loop.Index(source, index)));

                    body.Return(destination);
                })));
    }
}
```

The same primitives can generate list transformations with per-item method calls:

```csharp
var addMethod = typeof(List<OrderItemDto>)
    .GetMethod(nameof(List<OrderItemDto>.Add))!;
var mapMethod = typeof(IItemMapper)
    .GetMethod(nameof(IItemMapper.Map))!;

builder.AddClass("ItemMapperAdapter", c => c
    .AddMethod("MapItems", typeof(List<OrderItemDto>), m => m
        .WithParameter<List<OrderItem>>("source")
        .WithParameter<IItemMapper>("mapper")
        .EmitsBody(body =>
        {
            var source = body.Parameter<List<OrderItem>>("source");
            var mapper = body.Parameter<IItemMapper>("mapper");
            var destination = body.DeclareLocal<List<OrderItemDto>>("destination");
            var index = body.DeclareLocal<int>("i");

            body.If(body.IsNull(source), whenTrue: branch =>
            {
                branch.Return(branch.Constant(null, typeof(List<OrderItemDto>)));
            });

            body.Assign(
                destination,
                body.New(
                    typeof(List<OrderItemDto>),
                    body.Property(source, nameof(List<OrderItem>.Count))));

            body.For(
                initialize: loop => loop.Assign(index, loop.Constant(0)),
                condition: loop => loop.LessThan(
                    index,
                    loop.Property(source, nameof(List<OrderItem>.Count))),
                increment: loop => loop.Assign(
                    index,
                    loop.Add(index, loop.Constant(1))),
                body: loop => loop.Evaluate(loop.Call(
                    destination,
                    addMethod,
                    loop.Call(mapper, mapMethod, loop.Index(source, index)))));

            body.Return(destination);
        })));
```

For non-indexed sources such as `IEnumerable<T>`, use `ForEach(...)`. DynaBee
emits the enumerator pattern and disposes the enumerator when enumeration ends.

```csharp
var addMethod = typeof(List<string>)
    .GetMethod(nameof(List<string>.Add))!;

builder.AddClass("EnumerableCopier", c => c
    .AddMethod("Copy", typeof(List<string>), m => m
        .WithParameter<IEnumerable<string>>("source")
        .EmitsBody(body =>
        {
            var source = body.Parameter<IEnumerable<string>>("source");
            var destination = body.DeclareLocal<List<string>>("destination");

            body.Assign(destination, body.New<List<string>>());
            body.ForEach(source, "item", (item, loop) =>
            {
                loop.Evaluate(loop.Call(destination, addMethod, item));
            });
            body.Return(destination);
        })));
```

Method bodies can also express richer computed values without falling back to raw IL:

```csharp
builder.AddClass("ExpressionMapper", c => c
    .AddMethod("Compute", typeof(int), m => m
        .WithParameter<int>("x")
        .WithParameter<int>("y")
        .EmitsBody(body =>
        {
            var x = body.Parameter<int>("x");
            var y = body.Parameter<int>("y");

            body.Return(body.Add(
                body.Multiply(body.Subtract(x, y), body.Constant(2)),
                body.Modulo(x, y)));
        }))
    .AddMethod("NameOrDefault", typeof(string), m => m
        .WithParameter<string>("name")
        .EmitsBody(body =>
        {
            body.Return(body.Coalesce(
                body.Parameter<string>("name"),
                body.Constant("Unknown")));
        })));
```

### 7) Cached method invokers

DynaBee can create cached invokers for generated methods. The invoker resolves
reflection metadata once, compiles a dispatch bridge, and avoids `MethodInfo.Invoke(...)`
during repeated calls.

```csharp
using DynaBee.FluentApi.Invocation;

var mapper = context.CreateInstance("UserToUserDtoMapper");

var invoker = context.CreateBoundMethodInvoker(
    "UserToUserDtoMapper",
    mapper,
    "Map",
    new[] { typeof(User), typeof(IMapContext) });

var result = invoker.Invoke(new object[] { user, mapContext });
```

Multi-source methods use the same API:

```csharp
var invoker = context.CreateBoundMethodInvoker(
    "OrderCustomerToOrderDtoMapper",
    mapper,
    "Map",
    new[] { typeof(Order), typeof(Customer), typeof(IMapContext) });

var result = invoker.Invoke(new object[] { order, customer, mapContext });
```

## Real-World Use Cases

### 1) Plugin systems
Generate adapter types at runtime for plugin contracts discovered dynamically.

### 2) Multi-tenant applications
Create tenant-specific behavior types (validation rules, policy handlers, mapping profiles) without shipping many static assemblies.

### 3) Runtime API clients / SDK wrappers
Build strongly-typed runtime clients from metadata or schemas loaded from external systems.

### 4) Dynamic domain models
Generate entities or value objects from configuration (for example low-code platforms or metadata-driven apps).

### 5) Test doubles and runtime stubs
Create dynamic implementations for integration testing, custom mocks, or simulation environments.

### 6) High-performance dispatch layers
Emit optimized execution paths for expression-based pipelines where reflection-only invocation is too expensive.

### 7) Framework integrations via metadata
Attach typed metadata in Fluent API, then consume it in external packages (for example EF mapping hints like table/column/type, custom serialization hints, validation hints).

### 8) Metadata-driven EF or API model bootstrapping
Use profiles to group dynamic entity definitions by logical assembly, then resolve the generated `IAssemblyContext` through `IDynaBeeAssemblyCatalog` while bootstrapping framework integrations.

## Benchmarks

Command:

```bash
dotnet run -c Release -f net8.0 --project benchmarks/DynaBee.Benchmarks/DynaBee.Benchmarks.csproj -- --filter *
```

Measured results:

| Benchmark | Mean | Allocated |
|---|---:|---:|
| `CreateInstance` | 90.70 ns | 200 B |
| `CallViaInterface` | 1.52 ns | 0 B |
| `CallViaReflection` | 25.17 ns | 24 B |
| `BuildClass_NoCache` | 276.40 us | 10,569 B |
| `BuildClass_FromCache` | 43.16 ns | 144 B |

Notes:

- `BuildClass_NoCache` and `BuildClass_FromCache` were executed with `ShortRun`.
- Cache dramatically reduces repeated build cost.

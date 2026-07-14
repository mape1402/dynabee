# DynaBee

**DynaBee** is a lightweight .NET library that uses `Reflection.Emit` to generate dynamic types at runtime.

It is designed for scenarios where you need to build classes, methods, properties, and contracts programmatically, with a fluent API that is simple to use and easy to extend.

The recommended application model is DI-first: define `DynaBeeProfile` classes, let DynaBee discover them, and resolve generated assembly contexts through `IDynaBeeAssemblyCatalog`.

## What It Solves

- Runtime type generation without producing intermediate source code.
- Fluent creation of classes, interfaces, structs, enums, and records.
- Method implementation through IL, lambdas, or expression trees.
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

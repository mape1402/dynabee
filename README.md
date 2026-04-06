# DynaBee

**DynaBee** is a lightweight .NET library that uses `Reflection.Emit` to generate dynamic types at runtime.

It is designed for scenarios where you need to build classes, methods, properties, and contracts programmatically, with a fluent API that is simple to use and easy to extend.

## What It Solves

- Runtime type generation without producing intermediate source code.
- Fluent creation of classes, interfaces, structs, enums, and records.
- Method implementation through IL, lambdas, or expression trees.
- Dependency injection integration.
- Typed metadata for external extensions (for example EF or other frameworks).
- Assembly cache/versioning to reduce type build overhead.

## Requirements

- .NET SDK 8.0+ recommended for development.
- The library multi-targets: `net6.0`, `net7.0`, `net8.0`, and `net9.0`.

## Installation

```bash
dotnet add package DynaBee
```

## Getting Started

### 1) Simple class with property and method

```csharp
using DynaBee.FluentApi;
using System.Linq.Expressions;

var context = DynaBeeBuilder
    .CreateAssembly("Demo.Basic")
    .AddClass("Person", c => c
        .AddAutoProperty<string>("Name")
        .AddMethod("SayHello", typeof(string), m => m
            .WithParameter<string>("target")
            .EmitsExpression((Expression<Func<string, string>>)(target => "Hello " + target))))
    .Build();

var personType = context.GetClrType("Person");
var person = Activator.CreateInstance(personType)!;
personType.GetProperty("Name")!.SetValue(person, "Mario");
var msg = personType.GetMethod("SayHello")!.Invoke(person, new object[] { "DynaBee" });
```

### 2) Implement an interface

```csharp
using DynaBee.FluentApi;
using System.Linq.Expressions;

var context = DynaBeeBuilder
    .CreateAssembly("Demo.Contracts")
    .AddClass("Calculator", c => c
        .Implements<ICalculator>()
        .AddMethod("Sum", typeof(int), m => m
            .WithParameter<int>("x")
            .WithParameter<int>("y")
            .EmitsExpression((Expression<Func<int, int, int>>)((x, y) => x + y))))
    .Build();

var calc = context.CreateInstance<ICalculator>("Calculator");
var result = calc.Sum(3, 4); // 7

public interface ICalculator
{
    int Sum(int x, int y);
}
```

### 3) Dependency injection + per-interface registration control

```csharp
using DynaBee.FluentApi;
using DynaBee.FluentApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var context = DynaBeeBuilder
    .CreateAssembly("Demo.DI")
    .AddClass("InvoiceService", c => c
        .Implements<IInvoiceService>(registerInDi: true)
        .Implements<IInternalContract>(registerInDi: false)
        .RegisterAsConcrete(false)
        .Inject<IUnitOfWork>("UnitOfWork")
        .AddMethod("Commit", typeof(int), m => m
            .EmitsInjectedLambda<IUnitOfWork, int>("UnitOfWork", uow => uow.SaveChanges())))
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IUnitOfWork>(new UnitOfWork());
services.AddDynaBee(context); // Respects fluent per-interface DI settings
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

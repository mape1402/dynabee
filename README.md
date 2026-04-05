# 🐝 DynaBee

**DynaBee** is a lightweight .NET library that leverages **Reflection.Emit** to dynamically generate types at runtime. 

[![Build](https://github.com/mape1402/dynabee/actions/workflows/publish.yaml/badge.svg)](https://github.com/mape1402/dynabee/actions/workflows/publish.yaml)
[![NuGet](https://img.shields.io/nuget/v/DynaBee.svg)](https://www.nuget.org/packages/DynaBee/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

---

Designed for advanced scenarios such as runtime code generation, dynamic proxies, or custom serializers, DynaBee enables high-performance dynamic type creation without sacrificing control or flexibility.

---

## ✨ Features

- 🔧 *Build types on the fly.*
-  ⚡ *Optimize for speed with IL generation.*
-  🐝 *Perfect for developers who demand power and precision in dynamic scenarios.*
- 🔌 DI-friendly: plug it into any `IServiceProvider`
- 🧼 Zero dependencies (except DI abstractions)
- 🧪 Battle-tested with xUnit & NSubstitute

---

## 📦 Installation

```bash
dotnet add package DynaBee

```

## ?? Quick Start

```csharp
using DynaBee.FluentApi;
using System.Reflection.Emit;

var context = DynaBeeBuilder
    .CreateAssembly("Demo.Assembly")
    .AddClass("Person", c => c
        .Implements<IMyContract>()
        .Inherits<MyBaseClass>()
        .AddAutoProperty<string>("Name")
        .AddMethod("SayHello", typeof(string), m => m
            .WithParameter<string>("to")
            .Emits(il =>
            {
                il.Emit(OpCodes.Ldstr, "Hello ");
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) }));
                il.Emit(OpCodes.Ret);
            })))
    .Build();

var personType = context.GetClrType("Person");
var person = context.CreateInstance("Person");
```

```csharp
// Lambda implementation (no IL)
.AddMethod("MultiplyByTwo", typeof(int), m => m
    .WithParameter<int>("x")
    .EmitsLambda((Func<int, int>)(x => x * 2)))

// Expression tree implementation translated to IL
.AddMethod("Sum", typeof(int), m => m
    .WithParameter<int>("x")
    .WithParameter<int>("y")
    .EmitsExpression((System.Linq.Expressions.Expression<Func<int, int, int>>)((x, y) => x + y)))
```
## 🛠️ Upcoming Features

- **Some new features...**
   Some new feature...


# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [v1.2.6] - 2026-07-19

### Added
- Added runtime delegate-type overloads for bound generated method delegates through `CreateBoundDelegate(Type, ...)`.
- Added runtime delegate-type overloads for generated constructor factories through `CreateFactoryDelegate(Type, ...)`.
- Added fixed-arity object adapters for two-argument and three-argument generated methods through `CreateObjectAdapter2(...)` and `CreateObjectAdapter3(...)`.
- Added high-arity object method adapters through `IDynaBeeObjectMethodAdapter` and `CreateArgumentListAdapter(...)`.
- Added invocation benchmarks comparing reflection, generated method invokers, fixed object adapters, and argument-list adapters.
- Added tests for runtime delegate creation, runtime factory creation, multi-argument object adapters, context-aware adapters, high-arity adapters, and clear adapter argument errors.

### Changed
- Object adapter dispatch now validates argument assignability with Dynabee resolution errors instead of leaking raw cast failures.

## [v1.2.5] - 2026-07-19

### Added
- Added generated method descriptors through `DynaBeeGeneratedMethodDescriptor`.
- Added generated method descriptor lookup helpers for single overloads and generated types.
- Added typed bound delegate creation through `CreateBoundDelegate<TDelegate>(...)`.
- Added typed open-instance delegate creation through `CreateOpenDelegate<TDelegate>(...)`.
- Added typed constructor factory delegate creation through `CreateFactoryDelegate<TDelegate>(...)`.
- Added object-based single-argument adapters through `CreateObjectAdapter(...)`.
- Added overload-safe generated method registration support.
- Added tests for bound delegates, open delegates, overloaded method delegates, typed factories, high-arity delegates, object adapters, and generated method descriptors.

### Changed
- Generated type contexts now support multiple method elements with the same generated name while preserving clear single-element lookup errors for ambiguous names.

## [v1.2.4] - 2026-07-17

### Added
- Added `IBeeMethodBodyBuilder.ForEach(...)` for enumerating arrays, lists, and `IEnumerable<T>`-compatible sources without materializing a temporary list.
- Added normal enumerator disposal semantics for generated `ForEach(...)` loops.
- Added richer value expression primitives for generated method bodies: `Subtract(...)`, `Multiply(...)`, `Divide(...)`, `Modulo(...)`, `AndAlso(...)`, `OrElse(...)`, `Not(...)`, and `Coalesce(...)`.
- Added tests for direct `IEnumerable<T>` enumeration, enumerator disposal, arithmetic expressions, boolean short-circuit expressions, reference coalescing, and nullable coalescing.

## [v1.2.3] - 2026-07-17

### Added
- Added method body loop generation through `IBeeMethodBodyBuilder.For(...)`.
- Added ordered comparison expressions: `LessThan(...)`, `LessThanOrEqual(...)`, `GreaterThan(...)`, and `GreaterThanOrEqual(...)`.
- Added indexed access expressions through `Index(...)` for one-dimensional arrays and indexer properties such as `List<T>[int]`.
- Added runtime-sized array creation through `NewArray(...)`.
- Added constructor calls with arguments through `New(Type, params IBeeValueExpression[])`.
- Added collection mapping tests for array copy, list copy, list item transformation, and collection member assignment.

### Changed
- Constructor, array length, index, loop condition, and comparison generation now fail early for unsupported or incompatible operands before emitting IL.

## [v1.2.2] - 2026-07-16

### Added
- Added high-level method call expressions to `IBeeMethodBodyBuilder` through `Call(...)` and `StaticCall(...)`.
- Added `Self()` to method bodies so generated methods can access properties and fields on their own generated instance.
- Added `Evaluate(...)` for emitting method calls or value expressions used only for side effects.
- Added support for instance method calls, static method calls, closed generic `MethodInfo` calls, and interface method calls from generated method bodies.
- Added generated-member lookup support so `Self()` can read generated properties and fields while the dynamic type is still being emitted.
- Added tests for instance calls, static generic service-provider calls, interface resolver calls, self property access, method-result assignment, and incompatible call argument validation.

### Changed
- Method body call generation now validates target methods, open generics, argument counts, and argument type compatibility before emitting IL.
- Dynamic type element build order now ensures fields and properties are defined before method bodies that may access them through `Self()`.

## [v1.2.1] - 2026-07-16

### Added
- Added cached generated method invoker abstractions through `IDynaBeeMethodInvoker` and `IDynaBeeBoundMethodInvoker`.
- Added `IAssemblyContext` invoker extensions for creating unbound and instance-bound generated method invokers.
- Added typed bound invoker delegate overloads for one, two, and three method arguments.
- Added dynamic-method based dispatch so repeated invocations avoid `MethodInfo.Invoke(...)`, repeated method lookup, and repeated delegate creation.
- Added clear invoker resolution and invocation errors including assembly name, type name, method name, and requested parameter types.
- Added tests for single-source runtime invocation, multi-source runtime invocation, invoker caching, and error behavior.

## [v1.2.0] - 2026-07-16

### Added
- Added `BeeMethodBuilder.EmitsBody(...)` and public method body builder abstractions for generating method bodies without direct IL opcodes.
- Added body builder support for parameters, locals, object construction, constants, default values, property and field access, assignment, conditionals, conversions, string concatenation, numeric addition, null checks, and returns.
- Added separate public body builder contracts for value expressions, assignable expressions, locals, parameters, and method bodies.
- Added body builder support for nullable conversions, nullable null checks, enum conversions, and static property/field access.

## [v1.1.0] - 2026-07-13

### Added
- Added profile-based dynamic assembly configuration through `IDynaBeeProfile` and `DynaBeeProfile`.
- Added `IBeeAssemblyBuilder` so profiles configure assemblies through an abstraction.
- Added automatic profile discovery and grouping by profile assembly name via `AddDynaBeeProfiles(...)`.
- Added `IAssemblyContextRegistry` and `IAssemblyContextProvider` for mutable runtime definitions with immutable context snapshots.
- Added `IDynaBeeAssemblyCatalog` to resolve generated registries, providers, and contexts by logical assembly name.
- Added `AddDynaBeeRegistry(...)` for explicit, backward-compatible registry setup with automatic initial DI registration.
- Added a DI-friendly assembly builder factory abstraction.

### Changed
- `DynaBeeProfile` now pins each profile to a single logical assembly name, keeping all types declared by that profile grouped consistently.
- Profile, registry, and service-registration flows now create assembly builders through `IDynaBeeAssemblyBuilderFactory` instead of depending on the static `DynaBeeBuilder` entry point.
- Removed `net6.0` and `net7.0` targets, and added `net10.0` support.
- README now documents automatic profile discovery, assembly grouping, registry usage, and existing direct builder compatibility.

### Compatibility
- Existing `DynaBeeBuilder.CreateAssembly(...)` and `services.AddDynaBee(...)` flows remain supported.
- Existing per-interface DI registration metadata continues to be honored when generated types are registered.

## [v1.0.1] - 2026-04-05

### Fixed
- Fixed invalid IL emission in expression-based methods when binary expressions use overloaded operators (for example `string` concatenation and `decimal` arithmetic).
- `EmitsExpression(...)` now emits a method call to the operator implementation when `BinaryExpression.Method` is present, instead of always emitting raw arithmetic opcodes.
- Added regression coverage for expression-based string concatenation to prevent `InvalidProgramException`/runtime IL failures in real usage.

## [v1.0.0] - 2026-04-05

### Added
- Extended Fluent API to support dynamic `class`, `interface`, `struct`, `enum`, and record-like generation.
- Added support for access modifiers on classes, methods, properties, and fields.
- Added fluent attribute configuration for classes, methods, and properties.
- Added record semantics support including `Equals(object)`, `GetHashCode()`, `ToString()`, and `Deconstruct(...)`.
- Added assembly build cache/version support via `WithVersion(...)`, `EnableCache()`, and `DisableCache()`.
- Added diagnostics models and JSON export helpers for generated assemblies and members.
- Added dependency injection integration extensions for generated types.
- Added per-interface DI registration control from Fluent API and concrete-type registration toggle.
- Added typed metadata extensibility (`BeeMetadataKey<T>`) for types and members.
- Added metadata read/write support in context/builders to enable external integrations (e.g. EF-style extensions).
- Added benchmark suite using BenchmarkDotNet with generation, instantiation, and invocation scenarios.
- Added complete API reference document (`docs/DynaBee.API.md`) in Microsoft-style format.

### Changed
- Updated README with installation, requirements, getting started examples, real-world use cases, and benchmark results.
- Improved XML documentation coverage for public Fluent API and diagnostics components.

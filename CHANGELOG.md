# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

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

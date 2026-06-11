# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.0.0]

A major modernization of the toolbox. The libraries are retargeted to **.NET 10**
(with the pure libraries also multi-targeting `netstandard2.0`), the repository is
restructured into `src/` and `test/`, two Entity Framework packages are removed,
three new packages are added, and a full NuGet publishing pipeline to a private
**GitHub Packages** feed is introduced. The major version bump reflects the
**breaking changes**: the removed packages, the new package split, and the
framework retarget.

### Added

- **`JDMallen.Toolbox.AI`** (new package): minimal, trim-safe chat completion
  clients for Anthropic and Azure OpenAI over raw HTTP with source-generated
  JSON, with no heavyweight SDK dependencies.
- **`JDMallen.Toolbox.Data.Abstractions`** (new package): repository-pattern,
  entity-model, and query-parameter abstractions split out of the core library
  so consumers can depend on the contracts without the implementations.
- **`JDMallen.Toolbox.EFCore.SqlServer`** (new package): SQL Server provider
  package that brings in `Microsoft.EntityFrameworkCore.SqlServer`, keeping the
  core `JDMallen.Toolbox.EFCore` package provider-agnostic.
- **Expanded ASP.NET Core minimal-API helpers** in `JDMallen.Toolbox.AspNetCore`:
  JWT authentication configuration, endpoint registration extensions, health
  checks, OpenAPI helpers, rate limiting, pagination, `ProblemDetails` results, a
  FluentValidation-backed request-validation filter, and claims-principal and
  identity-result extensions.
- **Hosting background services** in `JDMallen.Toolbox.Hosting`:
  `ScheduledBackgroundService` (cron scheduling via `ncrontab`, with configurable
  overlap behavior), `ScopedBackgroundService`, an `ITimeProvider` abstraction
  with `SystemTimeProvider`, and an extensive `README.md`.
- **EF Core enhancements** in `JDMallen.Toolbox.EFCore`: an auditable-entity
  save-changes interceptor, automatic entity-configuration discovery, and
  soft-delete query filters.
- **`MiniGuid` comparison support**: `IComparable` implementation and the full
  set of comparison operators (`<`, `<=`, `>`, `>=`).
- **Test suites** under `test/`: `JDMallen.Toolbox.AI.Tests`,
  `JDMallen.Toolbox.AspNetCore.Tests`, `JDMallen.Toolbox.EFCore.Tests`, and
  `JDMallen.Toolbox.Hosting.Tests`.
- **Automated NuGet publishing**: `scripts/publish-nuget.sh` packs every packable
  project and pushes to the private GitHub Packages feed, and
  `.github/workflows/release.yml` runs it on every push to `main`, tagging the
  commit and creating a GitHub release whose notes are this file's section for
  the released version. The single `<Version>` in `Directory.Build.props` is the
  source of truth, and routine merges that don't bump it are no-ops.
- **Embedded symbols with Source Link**: PDBs are embedded in the assemblies
  (GitHub Packages has no symbol server) and Source Link maps them to GitHub, so
  debuggers can fetch the exact source for a build.
- **XML documentation** across the public API of every package.
- A root **`.editorconfig`** codifying the analyzer-severity policy, plus a
  committed **`nuget.config`** describing the GitHub Packages and nuget.org feeds
  (the token is supplied from the environment, never committed).

### Changed

- **Upgraded to .NET 10 and C# latest.** The pure libraries
  (`JDMallen.Toolbox`, `JDMallen.Toolbox.Data.Abstractions`,
  `JDMallen.Toolbox.Hosting`) multi-target `netstandard2.0;net10.0`; the EF Core
  and ASP.NET Core libraries target `net10.0`.
- **Restructured the repository** into `src/` and `test/` (projects previously
  lived at the repository root).
- **Authentication DTOs converted to records** with init-only properties.
- **Refactored `EFContextBase`** and modernized internals across the codebase:
  source-generated logging, extension methods, and broad analyzer cleanup.
- **Upgraded dependencies** to the `10.0.x` line (`Microsoft.Extensions.*`,
  `Microsoft.EntityFrameworkCore.*`, ASP.NET Core Identity and authentication
  packages).
- **Symbol packaging** switched from separate `.snupkg` symbol packages to
  embedded PDBs.

### Removed

- **`JDMallen.Toolbox.EFCore.Patterns`** package — its functionality was
  consolidated into `JDMallen.Toolbox.EFCore` / `EFContextBase`.
- **`JDMallen.Toolbox.EFCore.Services`** package — likewise removed during the
  EF Core refactor.

[3.0.0]: https://github.com/jdmallen/toolbox/releases/tag/3.0.0

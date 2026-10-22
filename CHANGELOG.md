# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.2.0]

One additive feature. No breaking changes and no behavior changes to existing
APIs.

### Added

`JDMallen.Toolbox.EFCore`:

- **`SoftDeleteFilterOptions`** and an
  **`AddSoftDeleteQueryFilter(ModelBuilder, SoftDeleteFilterOptions)`**
  overload, the soft-delete counterpart to 4.1.0's `AuditTimestampOptions`.
  Every root, non-owned entity type that maps a `bool` property named
  `PropertyName` (default `IsDeleted`, CLR or shadow) gets a
  `!EF.Property<bool>(e, PropertyName)` query filter, whether or not it
  implements `IEntityModel`; types without the property, or with a non-`bool`
  one, are skipped. An optional `FilterName` registers it as an EF Core named
  query filter, so `IgnoreQueryFilters([FilterName])` drops just this one. The
  parameterless overload is unchanged: `IEntityModel` entities only, unnamed.

## [4.1.0]

Three additive features, each replacing code an app had to write itself. No
breaking changes and no behavior changes to existing APIs.

### Added

`JDMallen.Toolbox.EFCore`:

- **`AuditTimestampOptions`** and an
  **`AuditableEntitySaveChangesInterceptor(AuditTimestampOptions,
  TimeProvider?)`** constructor. With options, the interceptor stamps the
  named created/modified properties on any tracked entity that has them, not
  only `IEntityModel` implementations, so entities named `CreatedAtUTC` /
  `UpdatedAtUTC` (for example) get audit timestamps too. `DateTime` and
  `DateTime?` both work. `StampModifiedOnCreate = false` leaves a nullable
  modified timestamp null until the first update. The parameterless
  constructor is unchanged: `IEntityModel` entities with the `DateCreated` /
  `DateModified` names.

`JDMallen.Toolbox.AspNetCore`:

- **Typed problem results**: `BadRequestProblem`, `ForbiddenProblem`,
  `NotFoundProblem`, `ConflictProblem`, and `InternalServerErrorProblem`, on a
  shared `TypedProblemResult` base. Same status, title, and detail as the
  matching `ProblemDetailsResults` method, but each documents its own status in
  OpenAPI (`IEndpointMetadataProvider`), so it can sit in a `Results<...>`
  union without `.ProducesProblem(...)`. Execution goes through
  `TypedResults.Problem`, so `AddProblemDetails` customizations still apply.
- **`ToPagedListAsync(query, request, defaultPageSize, cancellationToken)`**
  overload, for apps whose default page size isn't 10. The request's own
  `PageSize` still wins, and the 100 cap still applies; a default outside
  1-100 throws `ArgumentOutOfRangeException`. The original overload now
  delegates to it with the new `PaginationExtensions.DefaultPageSize` (10).

## [4.0.0]

`JDMallen.Toolbox` becomes a small, dependency-light toolkit worth installing on
its own: `MiniGuid`, LINQ helpers for any `IQueryable<T>`, and null-aware
string extensions. The siblings build on it instead of carrying their own
copies, and eight bugs are fixed, including JWT tokens that couldn't be signed
or read back and password scores that depended on the server's culture. All
eight packages move to 4.0.0; `AI`, `Data.Abstractions`, `EFCore.Identity`, and
`EFCore.SqlServer` have no API changes.

### Breaking

`JDMallen.Toolbox`:

- **Removed `DateTimeExtensions` and `StructExtensions.ToUnixTimestamp`.** Use
  `DateTimeOffset.ToUnixTimeSeconds()` / `FromUnixTimeSeconds()`, available on
  netstandard2.0 too.
- **`StructExtensions` is renamed `StringExtensions`** (same namespace).
  Extension-method calls such as `value.HasValue()` compile unchanged; direct
  static calls through the old class name don't.
- **Removed `ReflectionExtensions.IsVirtual`, `UtilityMethods`,
  `Models.EndpointConfiguration`, and `Resources.ResourceLoader`.**
  `UtilityMethods.GetExpressionFromPropertyName` is replaced by
  `JDMallen.Toolbox.Linq.PropertyPath.ToLambda`. The password word lists that
  `ResourceLoader` exposed are now internal to `JDMallen.Toolbox.AspNetCore`.
- **Removed `SqliteEntityModel`, `SqliteEntityModel<TId>`, `MySqlEntityModel`,
  and `MySqlEntityModel<TId>`.** Derive from `EntityModel<TId>`.
- **`ReplaceVisitor` is internal.** `ExpressionExtensions.Replace` still works.
- **`MiniGuid` serializes with System.Text.Json, not Newtonsoft.Json.**
  `MiniGuidJsonConverter` is now a sealed System.Text.Json
  `JsonConverter<MiniGuid>`, applied by attribute, and `MiniGuid` serializes as
  its 22-character string (System.Text.Json used to write an object with `Guid`
  and `Value`). Newtonsoft.Json users register their own converter; the package
  README has a short one.
- **Dependencies removed:** `Newtonsoft.Json`,
  `Microsoft.Extensions.Configuration.Binder`,
  `Microsoft.Extensions.Logging.Abstractions`,
  `System.ComponentModel.Annotations`, and `JetBrains.Annotations`. Code that
  used them through this package must reference them directly. The only new
  dependency is `System.Text.Json`, on netstandard2.0 only.

`JDMallen.Toolbox.EFCore`:

- **Removed `Filters.Filters` and `Extensions.QueryExtensions`.** `Search`, the
  `CreatedAfter` / `CreatedBefore` / `ModifiedAfter` / `ModifiedBefore`
  filters, and `OrderBy(string, bool)` (now `OrderByProperty`) live in
  `JDMallen.Toolbox.Linq`, which this package still brings in.

`JDMallen.Toolbox.AspNetCore`:

- **`AddJwtAuthentication(configuration, signingCredentials, jwtSectionName =
  "Jwt")`**: the signing credentials are now required and second (they were
  optional and last); null throws `ArgumentNullException`. Calls that name the
  `signingCredentials:` argument still compile.
- **Removed `JwtOptions.SigningCredentials`.** The credentials passed to
  `AddJwtAuthentication` are registered as the new `JwtSigningCredentials`.
- **`JwtTokenFactory`'s constructor** takes `(IOptions<JwtOptions>,
  JwtSigningCredentials)`. Resolve `IJwtTokenFactory` from DI instead of
  constructing it.
- **Removed `RequireJwtAuth`** (obsolete since 3.0).
- **`JwtClaims` is a static class**, like the other constant holders.

`JDMallen.Toolbox.Hosting` (netstandard2.0 build only; net10.0 is unchanged):

- **Removed `ITimeProvider` and `SystemTimeProvider`.** The base service
  constructors and the `TimeProvider` property use `System.TimeProvider` on
  every target, from the `Microsoft.Bcl.TimeProvider` package on
  netstandard2.0.

### Fixed

- **JWT tokens issued from DI were unsigned.** `AddJwtAuthentication`
  validated with the passed credentials but never gave them to
  `JwtTokenFactory`, so a factory resolved from DI signed with null
  credentials. Both now use the one registered `JwtSigningCredentials`, and
  `AddJwtAuthentication` registers `IJwtTokenFactory` (unless one is already
  registered).
- **`GetUserIdAsGuid()` failed on the library's own tokens.**
  `GenerateToken` wrote the email into the `id` claim and dropped the user ID.
  It now carries the identity's `id` claim, falling back to
  `ClaimTypes.NameIdentifier`.
- **`GenerateToken` threw on every identity from `GenerateClaimsIdentity`**
  ("Sequence contains more than one matching element"), because that identity
  held two Name claims. It now holds one.
- **`OAuthConfiguration.GoogleScopes` never bound from configuration** (it was
  get-only). It now has a setter, like `GitHubScopes`.
- **Password scores depended on the server's culture.** `CustomPasswordValidator`
  parsed its frequency table with the current culture, so under de-DE scores
  were wildly inflated and under fr-FR parsing failed. The table is now parsed
  with the invariant culture; scores under en-US are unchanged.
- **`OrderBy(string, bool)` threw `RuntimeBinderException` for non-public entity
  types.** It bound through `dynamic`; `OrderByProperty` builds the call with
  expression trees and works for any type (and without `Microsoft.CSharp`).
- **The Hosting and EFCore.SqlServer READMEs** had markdown mangled by a
  formatter, and the Hosting one claimed `netstandard2.1`; it targets
  `netstandard2.0;net10.0`.
- **`JDMallen.Toolbox.EFCore.SqlServer` pulled in a vulnerable
  `System.Security.Cryptography.Pkcs`.** EF Core SqlServer 10.0.13 depends on
  `Microsoft.Data.SqlClient` 6.1.7, which brings Pkcs 9.0.11 (CVE-2026-70162
  and CVE-2026-84008, both denial of service). The package now references the
  patched 10.0.13 directly.

### Changed

- **`Search` `Contains` and `StartsWith` now translate to SQL and ignore case
  everywhere.** They used the `StringComparison` overloads, which EF Core
  can't translate. Both sides are now upper-cased (`UPPER()` in SQL). A
  database index can't serve these searches, and non-ASCII case folding
  depends on the provider. `Exact` is unchanged.
- **The date filters emit a single comparison** (`>=` or `>`) instead of a
  conditional; results are the same.
- **`iat` is truncated to the second** (`DateTimeOffset.ToUnixTimeSeconds()`);
  3.x rounded up, which could stamp an `iat` up to a second in the future.
- **The invalid-cron-schedule error log names the schedule**: `Unable to parse
  cron schedule "<value>"; defaulting to "*/1 * * * *"` (EventId 404).
- **`JDMallen.Toolbox` ships a README**, and its package description and the
  EFCore one describe what the packages now contain.

### Added

- **`JDMallen.Toolbox.Linq`**: `Search`, `OrderByProperty`, the four date
  filters, and `PropertyPath.ToLambda`, for any `IQueryable<T>`.
- **`[NotNullWhen]` on the string extensions** (.NET Core 3.0+), so
  `if (name.HasValue())` tells the compiler `name` isn't null.
- **`JwtSigningCredentials`** in `JDMallen.Toolbox.AspNetCore.Options`.
- **Tests**: a new `JDMallen.Toolbox.Tests` project (run against the
  netstandard2.0 build with `-p:ToolboxTargetFramework=netstandard2.0`), SQLite
  translation tests for the LINQ helpers, an end-to-end JWT round trip, culture
  tests for the password validator, and Hosting tests for the invalid-schedule
  fallback (which also run against the netstandard2.0 build).
- **Package validation** against 3.2.2 in every packable project, with each
  intended break recorded in `CompatibilitySuppressions.xml`.

### Removed

- The `JDMallen.Toolbox.Hosting.TestWorker` console harness (never packed); its
  invalid-schedule check is now covered by the Hosting tests.

### Migrating from 3.x

1. `using JDMallen.Toolbox.EFCore.Filters;` or `...EFCore.Extensions;` for the
   query helpers becomes `using JDMallen.Toolbox.Linq;`, and
   `.OrderBy("Name", ascending)` becomes `.OrderByProperty("Name", ascending)`.
2. Delete any manual `JwtTokenFactory` registration and its `JwtOptions` with
   `SigningCredentials`; inject the `IJwtTokenFactory` that
   `AddJwtAuthentication` registers.
3. `ToUnixTimestamp()` becomes
   `new DateTimeOffset(value).ToUnixTimeSeconds()`.
4. If you serialize `MiniGuid` with Newtonsoft.Json, add the converter from the
   `JDMallen.Toolbox` README; with System.Text.Json, expect the string form.
5. Add direct references for anything you used through the removed
   dependencies (most often `Newtonsoft.Json`).


## [3.2.2]

Bug fix: password validation no longer hangs. Every release from 3.0.0 through
3.2.1 is affected, so upgrade if you use the password validator.

### Fixed

- **`CustomPasswordValidator` never returned for passwords of two or more
  characters.** The embedded-resource reader in `JDMallen.Toolbox`
  (`ResourceLoader`) never stopped at end of file: after the last line it
  yielded empty strings forever. `CheckPassword` reads the whole frequency
  table, so it (and `ValidateAsync`, which calls it) spun until the process ran
  out of memory. This hit every Identity sign-up or password change wired
  through `AddCustomIdentity` in `JDMallen.Toolbox.EFCore.Identity`. Each
  resource is now read once into an array. `ResourceLoader.CommonPasswords`
  and `ResourceLoader.FrequencyTable` keep the same `IEnumerable<string>` type,
  and password scores are unchanged.

### Added

- Regression tests for `CheckPassword`: known passwords with their entropy,
  strength, and error flags, and a 10-second timeout guard that fails rather
  than hangs if the bug returns.

## [3.2.1]

Documentation release: the consumer-facing packages now have READMEs, and each
one ships inside its package so nuget.org shows it on the package page. No
library code or public API changes.

### Added

- **READMEs for the five packages that lacked one**: `JDMallen.Toolbox.AI`,
  `JDMallen.Toolbox.AspNetCore`, `JDMallen.Toolbox.Data.Abstractions`,
  `JDMallen.Toolbox.EFCore`, and `JDMallen.Toolbox.EFCore.Identity`. Each covers
  installation, usage examples, target frameworks, and dependencies.

### Changed

- **Package readmes.** Each of those projects, plus `JDMallen.Toolbox.Hosting`
  and `JDMallen.Toolbox.EFCore.SqlServer` (which already had READMEs), sets
  `PackageReadmeFile` and packs its own `README.md`. The base `JDMallen.Toolbox`
  library is still published as a dependency of the others, without a README.

## [3.2.0]

Adds image input to `JDMallen.Toolbox.AI`, so vision-capable models can be sent
photos alongside the prompt. Additive only: existing callers compile and behave
exactly as before.

### Added

- **`ImageInput`** (`Data` as raw bytes, `MediaType` such as `image/jpeg`) and
  a **`CompletionRequest.Images`** init property, empty by default:
  `new CompletionRequest(system, user, 500) { Images = [image] }`. It is an
  init property rather than a constructor parameter, so the constructor that
  3.1 callers compiled against is unchanged.
- **Per-provider wire mapping.** With images attached, the user message
  `content` becomes an array with the images first, then the prompt text:
  - `AnthropicChatClient`: `image` blocks with a base64 `source`.
  - `OpenAICompatibleChatClient` and `AzureOpenAIChatClient`: `image_url`
    parts carrying `data:` URIs. Self-hosted runners need a vision model (for
    llama.cpp, one loaded with its `mmproj` projector).

### Unchanged

- Requests with no images still send `content` as a plain string, byte for byte
  the same as 3.1, since some self-hosted runners reject the array form.

## [3.1.1]

Maintenance release: dependency updates across every project and a
language-modernization pass. No public API changes, and no behavior changes to
any of the three chat completion clients.

### Changed

- **Runtime dependencies updated.** The `Microsoft.AspNetCore.*`,
  `Microsoft.Extensions.*`, and `Microsoft.EntityFrameworkCore*` packages move
  from 10.0.2 to **10.0.11**; `Microsoft.IdentityModel.Tokens` and
  `System.IdentityModel.Tokens.Jwt` from 8.15.0 to **8.22.0**; and
  `JetBrains.Annotations` from 2025.2.4 to **2026.2.0**. `FluentValidation`,
  `Newtonsoft.Json`, `ncrontab`, and `System.ComponentModel.Annotations` are
  unchanged.
- **Test tooling updated** across all four test projects:
  `Microsoft.NET.Test.Sdk` 17.14.1 → **18.9.0**, `xunit.runner.visualstudio`
  3.1.4 → **4.0.0**, `coverlet.collector` 6.0.4 → **10.0.1**, and
  `Microsoft.Extensions.TimeProvider.Testing` 10.7.0 → **10.9.0**. The two
  tooling-only packages now carry the `PrivateAssets`/`IncludeAssets` metadata
  the current test project template emits. `xunit` itself stays on 2.9.3.
- **Language modernization.** Constant-only holders — `JwtClaimTypes`,
  `OAuthSchemes`, `OtherJwtClaimTypes`, and the TestWorker `Program` — are now
  `static class`, so they can no longer be instantiated or derived from.
  `.ToArray()` and `.ToList()` call sites in `JwtTokenFactory`,
  `CustomPasswordValidator`, and two test helpers became collection expressions,
  and `SystemTimeProvider.GetUtcNow` is expression-bodied. Inside
  `OpenAICompatibleChatClient`, a guard clause became a ternary and a nested
  `if` became a `continue`; the wire format is untouched.
- **Solution organization.** The eight non-AI projects — `JDMallen.Toolbox`,
  `.AspNetCore`, `.Data.Abstractions`, `.EFCore`, `.EFCore.Identity`,
  `.EFCore.SqlServer`, `.Hosting`, and `.Hosting.TestWorker` — now sit in the
  `src` solution folder next to `JDMallen.Toolbox.AI`, which was already there,
  so the solution tree finally matches the on-disk layout. `CHANGELOG.md` joins
  the other root files in Solution Items. This is a solution-file change only:
  nothing moved on disk, and no project path, package, or reference changed.

## [3.1.0]

Adds a third chat completion client to **`JDMallen.Toolbox.AI`**, covering any
server that speaks the OpenAI `/v1/chat/completions` API — openai.com itself, or
a self-hosted runner such as llama.cpp, llama-swap, Ollama, vLLM, or LM Studio.
No existing API changed, so this is additive for current consumers.

### Added

- **`OpenAICompatibleChatClient`** and **`OpenAICompatibleClientOptions`**
  (`BaseUrl`, `Model`, optional `ApiKey`). The client posts to
  `{BaseUrl}/chat/completions` with the model named in the body, and reads
  `choices[0].message.content`. It differs from `AzureOpenAIChatClient` in the
  three places Azure diverges from stock OpenAI: the deployment is not in the
  path, auth is `Authorization: Bearer` rather than an `api-key` header, and no
  `api-version` query string is sent.
- **Optional authentication.** When `ApiKey` is null, empty, or whitespace, no
  `Authorization` header is sent at all — self-hosted runners typically accept
  anonymous requests, and an empty bearer token is worse than none.
- **Reasoning-model handling.** Thinking models are common on self-hosted
  runners and otherwise return unusable completions, so the client drops the
  `reasoning_content` field some servers emit and strips inline `<think>`,
  `<thinking>`, and `<reasoning>` blocks from the content. A *leading* unclosed
  opening tag means generation was truncated mid-thought (usually by
  `MaxTokens`); the remainder is discarded and the caller gets the empty string
  it already handles, rather than reasoning passed off as an answer. An unclosed
  tag anywhere else is left alone — it is far more likely to be part of the
  answer, such as a shell command or an XML snippet.

## [3.0.1]

Distribution-only change — no library code or public API changes. The packages
now publish to the public **nuget.org** feed instead of a private **GitHub
Packages** feed, and CI authenticates with nuget.org **trusted publishing**
(OIDC) rather than a stored token.

### Changed

- **Publishing moved to nuget.org.** `scripts/publish-nuget.sh` and the release
  workflow now pack and push to `https://api.nuget.org/v3/index.json` (public,
  anonymous restore) instead of the private GitHub Packages feed. The script's
  auth switched from a GitHub token (`--token` / `$GITHUB_PACKAGES_TOKEN`) to a
  nuget.org API key (`--api-key` / `$NUGET_ORG_API_KEY`).
- **CI uses trusted publishing (OIDC).** `.github/workflows/release.yml` mints a
  short-lived nuget.org API key from the job's OIDC token via `NuGet/login`
  (`id-token: write`), so no long-lived `GITHUB_TOKEN`, PAT, or stored secret is
  needed; the `packages: write` permission is dropped.
- **`nuget.config`** trimmed to the single nuget.org source; the GitHub Packages
  source and its `%GITHUB_PACKAGES_TOKEN%` credentials are removed.

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

[3.0.1]: https://github.com/jdmallen/toolbox/releases/tag/3.0.1
[3.0.0]: https://github.com/jdmallen/toolbox/releases/tag/3.0.0

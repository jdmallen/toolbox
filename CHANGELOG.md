# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

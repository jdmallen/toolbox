# JDMallen.Toolbox.AspNetCore

ASP.NET Core building blocks: minimal-API endpoint organization, request
validation, `ProblemDetails` results, pagination, JWT authentication and token
generation, OAuth providers, claims helpers, and an entropy-based Identity
password validator.

## Installation

```bash
dotnet add package JDMallen.Toolbox.AspNetCore
```

## Minimal APIs

Namespace `JDMallen.Toolbox.AspNetCore.MinimalApi` and its sub-namespaces.

### Endpoint classes

Implement `IEndpoint` (a `static abstract` `Map` method) to keep each endpoint
in its own class, then register it with `MapEndpoint<T>()`:

```csharp
public sealed class GetOrder : IEndpoint
{
	public static void Map(IEndpointRouteBuilder app)
		=> app.MapAuthorizedGroup("/orders")
			.MapGet("/{id:guid}", Handle)
			.WithCommonResponses(includeAuth: true);

	private static async Task<IResult> Handle(Guid id, AppDbContext db)
		=> await db.Orders.FindAsync(id) is { } order
			? TypedResults.Ok(order)
			: ProblemDetailsResults.NotFound($"Order {id} not found.");
}

app.MapEndpoint<GetOrder>();
```

- `MapAuthorizedGroup(prefix)`: a route group with `RequireAuthorization()`.
- `MapPublicGroup(prefix)`: a route group with `AllowAnonymous()`.

### Request validation

```csharp
app.MapPost("/register", Register)
	.WithRequestValidation<RegisterDto>();
```

`RequestValidationFilter<TRequest>` finds the `TRequest` argument and validates
it. If an `IValidator<TRequest>` (FluentValidation) is registered, it is used;
otherwise the request's DataAnnotations attributes are checked. Failures return
a 400 `ValidationProblem` keyed by property name.

### ProblemDetails results

`ProblemDetailsResults` returns RFC 7807 responses with a standard title:
`BadRequest`, `Conflict`, `Forbidden`, `NotFound`, and `InternalServerError`,
each taking a `detail` and optional `instance`.

To turn an Identity result into a response:

```csharp
IdentityResult result = await userManager.CreateAsync(user, dto.Password);
return result.Match(() => TypedResults.Created($"/users/{user.Id}"));
// on failure: 400 ValidationProblem with errors grouped by IdentityError.Code
```

`ToValidationProblem()` and `ToErrorDictionary()` are also available.

### Pagination

```csharp
app.MapGet("/orders", async ([AsParameters] PagedRequest paging, AppDbContext db,
	CancellationToken cancellationToken)
	=> TypedResults.Ok(await db.Orders
		.OrderBy(o => o.DateCreated)
		.ToPagedListAsync(paging, cancellationToken)));
```

`Page` defaults to 1 and `PageSize` to 10, capped at
`IPagedRequest.MaxPageSize` (100). A page or page size of zero or less throws
`ArgumentOutOfRangeException`. The returned `PagedList<T>` carries `Items`,
`Page`, `PageSize`, `TotalItems`, `TotalPages`, `HasNextPage`, and
`HasPreviousPage`.

### Ownership checks

`EnsureUserOwnsEntityFilter<TEntity>` returns 404 if the entity does not exist
and 403 if its `UserId` is not the caller's. `TEntity` implements `IOwnedEntity`
(a `Guid UserId`) and has a `Guid` `Id`. The entity ID comes from an `id` route
value or from an argument with an `Id` property.

```csharp
app.MapPut("/orders/{id:guid}", UpdateOrder)
	.AddEndpointFilter<EnsureUserOwnsEntityFilter<Order>>();
```

The filter resolves `DbContext` itself from DI, so register your context under
that type as well:
`services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>())`.

### Health checks, OpenAPI, rate limiting

```csharp
builder.Services.AddHealthChecks()
	.AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["liveness"]);
builder.Services.AddOpenApiWithJwt("Orders API", "v1");
builder.Services.AddAuthenticationRateLimiting(permitLimit: 5, windowSeconds: 60);

app.UseRateLimiter();
app.MapHealthCheckEndpoints();   // /health/live and /health/ready
app.MapPost("/login", Login).WithAuthRateLimit();
```

- `MapHealthCheckEndpoints` maps checks tagged `liveness` and `readiness` to two
  paths (both overridable).
- `AddOpenApiWithJwt` sets the document title and version and adds a `Bearer`
  JWT security scheme.
- `AddAuthenticationRateLimiting` adds a fixed-window policy named `auth` (queue
  limit 2), which `WithAuthRateLimit()` applies to an endpoint.
- `WithCommonResponses(includeAuth)` documents 500, plus 401 and 403 when
  `includeAuth` is true.

## JWT Authentication

```json
{
	"Jwt": {
		"Issuer": "https://api.example.com",
		"Audience": "https://app.example.com",
		"Subject": "api",
		"ValidForSpan": "00:30:00"
	}
}
```

```csharp
var signingCredentials = new SigningCredentials(
	new SymmetricSecurityKey(Convert.FromBase64String(signingKey)),
	SecurityAlgorithms.HmacSha256);

builder.Services.AddJwtAuthentication(
	builder.Configuration,
	signingCredentials);   // section name defaults to "Jwt"
```

This binds `JwtOptions` from the section and registers JWT bearer
authentication that validates issuer, audience, lifetime (5-minute clock skew),
and signing key, with HTTPS metadata required. Signing credentials can't come
from configuration, so they're a required argument (null throws
`ArgumentNullException`). They're registered once, as `JwtSigningCredentials`,
and used both to validate incoming tokens and to sign issued ones.

### Issuing tokens

`AddJwtAuthentication` also registers `IJwtTokenFactory` (unless you registered
your own first), so inject it wherever you issue tokens:

```csharp
ClaimsIdentity identity = tokenFactory.GenerateClaimsIdentity(user.Email!, user.Id);
string jwt = tokenFactory.GenerateToken(identity);
```

Each token gets a fresh `jti` (a `MiniGuid`), `iat` (whole Unix seconds),
`nbf`, and an expiry of `ValidForSpan` (default 30 minutes). The identity's
user ID (its `id` claim, or else `ClaimTypes.NameIdentifier`) goes into the
token's `id` claim, so `User.GetUserIdAsGuid()` works on requests that carry
it. Any `sid`, `email`, and `role` claims on the identity are copied too.

## OAuth Providers

```csharp
var oauth = builder.Configuration.GetSection("OAuth").Get<OAuthConfiguration>()!;

builder.Services.AddAuthentication()
	.AddOAuthGitHub(oauth)    // callback path /signin-github
	.AddOAuthGoogle(oauth);
```

Each throws `InvalidOperationException` if its client ID or secret is missing.
GitHub maps `id`, `email`, `login`, `html_url`, `avatar_url`, and `name` to
claims (the last four as `urn:github:*`). Scheme names are in `OAuthSchemes`.

## Claims and Identity Helpers

```csharp
Guid userId = httpContext.User.GetUserIdAsGuid();   // throws if absent or invalid
if (httpContext.User.TryGetUserIdAsGuid(out Guid id)) { /* ... */ }

string? email = httpContext.User.GetEmail();
IEnumerable<string> roles = httpContext.User.GetRoles();
```

The user ID is read from the `id` claim, falling back to
`ClaimTypes.NameIdentifier`. For MVC, `ModelState.AddIdentityErrors(result)`
copies Identity errors into model state.

## Password Validation

`CustomPasswordValidator<TUser>` extends Identity's `PasswordValidator<TUser>`
with three checks, configured by `PasswordComplexityOptions`:

| Check            | Option          | Default |
|------------------|-----------------|---------|
| Minimum length   | `MinimumLength` | 8       |
| Entropy estimate | `BitsThreshold` | 50 bits |
| Common password  | (embedded list) | always  |

`CheckPassword(password)` also works standalone and returns a `PasswordResult`
with `BitsOfEntropy`, a `PasswordStrength` rating (`Poor` through `Excellent`),
and `PasswordError` flags. `CustomIdentityErrorDescriber` supplies matching
error messages. `JDMallen.Toolbox.EFCore.Identity` registers the validator in
one call.

## Models and DTOs

- **`IdUser`, `IdRole`**: `IdentityUser<Guid>` and `IdentityRole<Guid>` with an
  unmapped `ShortId` exposing the key as a `MiniGuid`.
- **`LoginDto`, `RegisterDto`, `GitHubCallbackDto`**: request records with
  `[Required]` members, ready for `WithRequestValidation<T>()`.
- **Constants**: `JwtClaimTypes` (registered JWT claim names), `OtherJwtClaimTypes`,
  `OAuthSchemes`, `JwtClaims`.

## Target Framework

- `net10.0`

## Dependencies

- `JDMallen.Toolbox`
- `Microsoft.AspNetCore.Authentication.JwtBearer`, `.Google`, `.Facebook`,
  `.Twitter`
- `Microsoft.AspNetCore.OpenApi`
- `Microsoft.Extensions.Identity.Core` and `.Stores`
- `Microsoft.EntityFrameworkCore`
- `System.IdentityModel.Tokens.Jwt`, `Microsoft.IdentityModel.Tokens`
- `FluentValidation`

## License

MIT. See the repository root for the full license text.

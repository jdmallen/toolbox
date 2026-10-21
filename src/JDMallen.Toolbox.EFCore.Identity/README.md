# JDMallen.Toolbox.EFCore.Identity

One-call registration of ASP.NET Core Identity backed by Entity Framework Core,
wired to the entropy-based password validator from
`JDMallen.Toolbox.AspNetCore`.

## Installation

```bash
dotnet add package JDMallen.Toolbox.EFCore.Identity
```

## Quick Start

Use the GUID-keyed `IdUser` and `IdRole` from `JDMallen.Toolbox.AspNetCore` with
an `IdentityDbContext`:

```csharp
using JDMallen.Toolbox.AspNetCore.Models;
using JDMallen.Toolbox.AspNetCore.Options;
using JDMallen.Toolbox.EFCore.Identity.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options)
	: IdentityDbContext<IdUser, IdRole, Guid>(options);
```

```csharp
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.Configure<PasswordComplexityOptions>(o =>
{
	o.MinimumLength = 12;
	o.BitsThreshold = 60;
});

builder.Services.AddCustomIdentity<AppDbContext, IdUser, IdRole>(
	identityOptions: o => o.User.RequireUniqueEmail = true,
	hasherOptions: o => o.IterationCount = 600_000);
```

Each call registers, in order: `AddIdentity<TUser, TRole>()`,
`AddEntityFrameworkStores<TDbContext>()`, `AddDefaultTokenProviders()`, the user
store, the password validator, and the error describer. The returned
`IdentityBuilder` accepts further configuration.

## Overloads

| Type parameters                                           | Key          | Validator                        | Error describer | Identity entities |
|-----------------------------------------------------------|--------------|----------------------------------|-----------------|-------------------|
| `<TDbContext, TUser, TRole>`                              | `Guid`       | `CustomPasswordValidator<TUser>` | default         | default           |
| `<TDbContext, TUser, TRole, TId, TUserStore, TValidator>` | any `struct` | yours                            | default         | default           |
| Full 12-parameter form                                    | any `struct` | yours                            | yours           | yours             |

Use the full form when you have custom `IdentityUserClaim`, `IdentityUserRole`,
`IdentityUserLogin`, `IdentityUserToken`, or `IdentityRoleClaim` types, or want
`CustomIdentityErrorDescriber` registered as the error describer.

All overloads take two optional callbacks: `Action<IdentityOptions>` and
`Action<PasswordHasherOptions>`.

## Password Validation

The simplest overload adds `CustomPasswordValidator<TUser>` on top of Identity's
built-in rules. It rejects a password that is:

- on the embedded common-passwords list,
- shorter than `PasswordComplexityOptions.MinimumLength` (default 8), or
- below `PasswordComplexityOptions.BitsThreshold` bits of estimated entropy
  (default 50).

Configure `PasswordComplexityOptions` as shown above; the validator reads it
through `IOptions<PasswordComplexityOptions>`. Identity's own
`IdentityOptions.Password` rules (digits, uppercase, and so on) still apply, so
relax those if entropy should be the only complexity gate.

## Target Framework

- `net10.0`

## Dependencies

- `JDMallen.Toolbox.AspNetCore`
- `JDMallen.Toolbox.EFCore`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

## License

MIT. See the repository root for the full license text.

# JDMallen.Toolbox.EFCore

Entity Framework Core conventions for `IEntityModel` entities: a base
`DbContext` that discovers entity configuration, stamps audit timestamps on
save, and hides soft-deleted rows, plus a few `IQueryable` helpers.

Provider-agnostic: add the EF Core provider you use (or
`JDMallen.Toolbox.EFCore.SqlServer` for SQL Server).

## Installation

```bash
dotnet add package JDMallen.Toolbox.EFCore
```

## EFContextBase

Derive your context from `EFContextBase` and the conventions come for free:

```csharp
using JDMallen.Toolbox.EFCore.Config;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options)
	: EFContextBase(options)
{
	public DbSet<Order> Orders => Set<Order>();
}
```

What it does:

1. **Configuration discovery.** `OnModelCreating` calls
   `ApplyConfigurationsFromAssembly` on the derived context's assembly, so every
   `IEntityTypeConfiguration<T>` there is applied.
2. **Soft-delete filter.** Every root entity type implementing `IEntityModel`
   gets a global query filter of `e => !e.IsDeleted`.
3. **Audit timestamps.** `OnConfiguring` registers
   `AuditableEntitySaveChangesInterceptor`, which sets `DateCreated` and
   `DateModified` on added entities and `DateModified` on modified ones, in UTC.
4. **`IContext`.** The context implements `IContext` from
   `JDMallen.Toolbox.Data.Abstractions`.

If you override `OnModelCreating` or `OnConfiguring`, call the base method so
these still apply.

### Soft deletes

The filter only hides rows; it does not turn `Remove` into an update. To soft
delete, set `IsDeleted = true` and save. To see deleted rows, opt out of the
filter:

```csharp
var everything = await context.Orders.IgnoreQueryFilters().ToListAsync();
```

### Deterministic timestamps in tests

Override `TimeProvider` to control the clock the interceptor uses:

```csharp
public class TestDbContext(DbContextOptions<TestDbContext> options, TimeProvider clock)
	: EFContextBase(options)
{
	protected override TimeProvider TimeProvider => clock;
}
```

`FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` works well
here.

### Without the base class

The pieces work on their own with any `DbContext`:

```csharp
// In OnConfiguring, or via AddDbContext(o => o.AddInterceptors(...))
optionsBuilder.AddInterceptors(new AuditableEntitySaveChangesInterceptor());

// In OnModelCreating
modelBuilder.AddSoftDeleteQueryFilter();
```

## Query Helpers

### Date filters

Available on any `IQueryable<T>` where `T : IEntityModel`:

```csharp
using JDMallen.Toolbox.EFCore.Filters;

var recent = context.Orders
	.CreatedAfter(DateTime.UtcNow.AddDays(-7))
	.ModifiedBefore(cutoff, inclusive: true);
```

`CreatedAfter`, `CreatedBefore`, `ModifiedAfter`, and `ModifiedBefore` all take
an optional `inclusive` flag (default `false`).

### Search

```csharp
using JDMallen.Toolbox.Models;

var matches = context.Orders.Search(
	o => o.CustomerName,
	"smith",
	SearchStyle.StartsWith);
```

A null or whitespace search value returns the query unchanged; otherwise the
value is trimmed. `Exact` uses `==`. `Contains` and `StartsWith` use the
`StringComparison.CurrentCultureIgnoreCase` overloads, so check that your
provider can translate them to SQL before relying on them.

### Dynamic ordering

```csharp
using JDMallen.Toolbox.EFCore.Extensions;

var sorted = context.Orders.OrderBy("Customer.LastName", ascending: false);
```

The field name can be a dotted property path. A null or whitespace name returns
the query unchanged.

## Target Framework

- `net10.0`

## Dependencies

- `JDMallen.Toolbox`
- `JDMallen.Toolbox.Data.Abstractions`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Relational`

## License

MIT. See the repository root for the full license text.

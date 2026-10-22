# JDMallen.Toolbox.EFCore

Entity Framework Core conventions for `IEntityModel` entities: a base
`DbContext` that discovers entity configuration, stamps audit timestamps on
save, and hides soft-deleted rows.

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

Search, sort-by-property-name, and the `CreatedAfter` / `ModifiedBefore` date
filters live in the base `JDMallen.Toolbox` package, under
`JDMallen.Toolbox.Linq`, because they work on any `IQueryable<T>`. This
package depends on it, so they're already available:

```csharp
using JDMallen.Toolbox.Linq;

var recent = context.Orders
	.CreatedAfter(DateTime.UtcNow.AddDays(-7))
	.OrderByProperty("Customer.LastName", ascending: false);
```

They translate to SQL, including the case-insensitive `Contains` and
`StartsWith` searches. See the `JDMallen.Toolbox` README for details. Before
4.0.0 they were `JDMallen.Toolbox.EFCore.Filters.Filters` and
`JDMallen.Toolbox.EFCore.Extensions.QueryExtensions.OrderBy`.

## Target Framework

- `net10.0`

## Dependencies

- `JDMallen.Toolbox`
- `JDMallen.Toolbox.Data.Abstractions`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Relational`

## License

MIT. See the repository root for the full license text.

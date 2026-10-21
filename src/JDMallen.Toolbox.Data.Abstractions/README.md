# JDMallen.Toolbox.Data.Abstractions

Dependency-free contracts for entity models and data contexts. Reference this
package from domain or application layers that need the interfaces without
pulling in Entity Framework Core or any implementation.

## Installation

```bash
dotnet add package JDMallen.Toolbox.Data.Abstractions
```

## Interfaces

All types live in `JDMallen.Toolbox.Data.Abstractions.Interfaces`.

| Interface           | Purpose                                                                 |
|---------------------|-------------------------------------------------------------------------|
| `IModel`            | Marker for any model in the application                                 |
| `IEntityModel`      | A persisted entity: `DateCreated`, `DateModified`, `IsDeleted`          |
| `IEntityModel<TId>` | Adds a value-type primary key, `TId Id`                                 |
| `IAggregateRoot`    | Marker for a DDD aggregate root (extends `IEntityModel`)                |
| `IContext`          | A unit of work: `IDisposable` plus `SaveChangesAsync(CancellationToken)` |

The entity properties are read-only on the interface. Implementations decide how
they get set; `JDMallen.Toolbox.EFCore` stamps the timestamps through the change
tracker on save.

`TId` is constrained to `struct`, so keys are value types such as `int`,
`long`, or `Guid`, never `string`.

## Usage

Implement the interfaces directly:

```csharp
using JDMallen.Toolbox.Data.Abstractions.Interfaces;

public class Invoice : IEntityModel<Guid>, IAggregateRoot
{
	public Guid Id { get; private set; }
	public DateTime DateCreated { get; private set; }
	public DateTime DateModified { get; private set; }
	public bool IsDeleted { get; private set; }
}
```

Or inherit a ready-made base class from `JDMallen.Toolbox`
(`EntityModel<TId>`, `SqliteEntityModel<TId>`, `MySqlEntityModel<TId>`).

`IContext` lets services depend on "something that can save" rather than a
concrete `DbContext`:

```csharp
public class InvoiceService(IContext context)
{
	public Task<int> CommitAsync(CancellationToken cancellationToken)
		=> context.SaveChangesAsync(cancellationToken);
}
```

`EFContextBase` in `JDMallen.Toolbox.EFCore` implements `IContext`.

## Target Frameworks

- `netstandard2.0`
- `net10.0`

## Dependencies

None.

## License

MIT. See the repository root for the full license text.

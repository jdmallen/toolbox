# JDMallen.Toolbox

A small, dependency-light toolkit: URL-friendly GUIDs (`MiniGuid`), LINQ
helpers that work on any `IQueryable<T>` (in memory or through a query provider
such as EF Core), and a few null-aware string extensions.

## Installation

```bash
dotnet add package JDMallen.Toolbox
```

## MiniGuid

`MiniGuid` (`JDMallen.Toolbox.Structs`) wraps a `Guid` and its 22-character,
URL-safe Base64 encoding (`+` and `/` become `-` and `_`, padding dropped).

```csharp
using JDMallen.Toolbox.Structs;

MiniGuid id = MiniGuid.NewGuid();
string shortForm = id.Value;        // e.g. "x4m2Gx9oGkuKC3i2ZJ8Q7w"
Guid guid = id.Guid;

MiniGuid parsed = new MiniGuid(shortForm);
bool same = parsed == id;           // true
```

- Implicit conversions to and from `string` and `Guid`.
- Equality, ordering (`<`, `>`, `CompareTo`), and hashing follow the underlying
  `Guid`, and work against a plain `Guid` too.
- `MiniGuid.Encode(Guid)` and `MiniGuid.Decode(string)` do the conversion
  without creating a `MiniGuid`. `Decode` returns `Guid.Empty` for null or
  white space.

### JSON

`MiniGuid` serializes with System.Text.Json as its 22-character string, through
a converter applied by attribute, so no setup is needed. It also works as a
dictionary key. Reading a value that isn't a 22-character MiniGuid string
throws `JsonException`.

```csharp
JsonSerializer.Serialize(new { Id = MiniGuid.NewGuid() });
// {"Id":"x4m2Gx9oGkuKC3i2ZJ8Q7w"}
```

If you serialize with Newtonsoft.Json, register a converter like this one:

```csharp
using JDMallen.Toolbox.Structs;
using Newtonsoft.Json;

public sealed class MiniGuidNewtonsoftConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
		=> objectType == typeof(MiniGuid) || objectType == typeof(MiniGuid?);

	// Plain if statements on purpose: in "cond ? null : new MiniGuid(...)" the
	// null converts through MiniGuid's implicit string operator, so a JSON null
	// would come back as a MiniGuid wrapping Guid.Empty instead of null.
	public override object? ReadJson(
		JsonReader reader,
		Type objectType,
		object? existingValue,
		JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}

		return new MiniGuid((string)reader.Value!);
	}

	public override void WriteJson(
		JsonWriter writer,
		object? value,
		JsonSerializer serializer)
	{
		if (value is MiniGuid miniGuid)
		{
			writer.WriteValue(MiniGuid.Encode(miniGuid.Guid));
		}
		else
		{
			writer.WriteNull();
		}
	}
}
```

## LINQ helpers

All live in `JDMallen.Toolbox.Linq` and build plain expression trees, so EF Core
and other providers translate them to SQL.

### Search

```csharp
using JDMallen.Toolbox.Linq;
using JDMallen.Toolbox.Models;

var matches = context.Orders.Search(
	order => order.CustomerName,
	"smith",
	SearchStyle.StartsWith);
```

- A null or white-space search value returns the query unchanged; otherwise
  the value is trimmed.
- `Exact` is a plain `==`: ordinal in memory, and governed by the column's
  collation in a database.
- `Contains` and `StartsWith` ignore case by upper-casing both sides (`UPPER()`
  in SQL), because providers don't translate the `StringComparison` overloads.
  A database index can't serve these searches, and non-ASCII case folding
  depends on the provider (SQLite's `upper()` only folds ASCII).

### Sorting by a property name

```csharp
var sorted = context.Orders.OrderByProperty("Customer.LastName", ascending: false);
```

The path can name a property or field, with dots for nested members. A null or
white-space path returns the query unchanged; an unknown member throws
`ArgumentException`. The result is a real `OrderBy` call, so `ThenBy` can
follow it, and it works for non-public entity types.

`PropertyPath.ToLambda<T>("Customer.LastName")` (or
`PropertyPath.ToLambda(type, path)`) returns the underlying member-access
lambda if you need it elsewhere.

### Date filters

For any `IQueryable<T>` where `T : IEntityModel`:

```csharp
var recent = context.Orders
	.CreatedAfter(DateTime.UtcNow.AddDays(-7))
	.ModifiedBefore(cutoff, inclusive: true);
```

`CreatedAfter`, `CreatedBefore`, `ModifiedAfter`, and `ModifiedBefore` take an
optional `inclusive` flag (default `false`), which picks `>=`/`<=` over `>`/`<`
before the expression is built, so the SQL holds a single comparison.

### Expression composition

`Compose` and `Replace` (`JDMallen.Toolbox.Extensions`) splice one expression
into another. `Search` uses them; they're public for building similar helpers.

```csharp
Expression<Func<Order, string>> name = order => order.CustomerName;
Expression<Func<Order, bool>> isSmith = name.Compose(value => value == "Smith");
```

## String extensions

`HasValue()`, `IsNullOrEmpty()`, and `IsNullOrWhiteSpace()`
(`JDMallen.Toolbox.Extensions`) are instance-style wrappers for the static
`string` checks. They accept a null receiver, and on .NET Core 3.0+ they carry
`NotNullWhen` annotations, so the compiler knows the string isn't null after a
successful check:

```csharp
string? name = GetName();

if (name.HasValue())
{
	Console.WriteLine(name.Length); // no nullable warning
}
```

## Entity models

`EntityModel` and `EntityModel<TId>` (`JDMallen.Toolbox.Implementations`) are
base classes implementing `IEntityModel` and `IEntityModel<TId>` from
`JDMallen.Toolbox.Data.Abstractions`: `DateCreated` and `DateModified`
(defaulting to `DateTime.UtcNow`), `IsDeleted`, and a protected-set `Id`.
`JDMallen.Toolbox.EFCore` stamps the timestamps and filters soft-deleted rows.

## Target Frameworks

- `netstandard2.0`
- `net10.0`

## Dependencies

- `JDMallen.Toolbox.Data.Abstractions`
- `System.Text.Json` (netstandard2.0 only; built into .NET Core 3.0+)

## License

MIT. See the repository root for the full license text.

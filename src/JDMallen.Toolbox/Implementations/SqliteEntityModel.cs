using System.ComponentModel;

namespace JDMallen.Toolbox.Implementations;

/// <summary>
/// Base class for SQLite entity models with automatic timestamp tracking.
/// </summary>
public abstract class SqliteEntityModel : EntityModel;

/// <summary>
/// SQLite-specific entity model with generic identifier support.
/// Provides a string representation of the ID for REST API usage and data binding.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class SqliteEntityModel<TId>
	: EntityModel<TId>
	where TId : struct
{
	/// <summary>
	/// Gets or sets the identifier as a string for REST API binding and data
	/// conversion.
	/// </summary>
	/// <remarks>
	/// The getter converts the typed ID to its string representation.
	/// The setter uses TypeDescriptor to safely convert the string back to the target
	/// type.
	/// If conversion fails, the ID is set to its default value.
	/// </remarks>
	public string IdText
	{
		get => Id.ToString() ?? string.Empty;
		set
		{
			try
			{
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(TId));
				Id = (TId)converter.ConvertFromString(value)!;
			}

			// Best-effort REST/JSON binding: any failure to convert the supplied
			// string to TId falls back to default rather than surfacing.
#pragma warning disable CA1031 // Do not catch general exception types
			catch
			{
				Id = default;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}
	}
}

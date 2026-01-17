using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using Newtonsoft.Json;

namespace JDMallen.Toolbox.Implementations;

/// <summary>
/// Base class for MySQL entity models with automatic timestamp tracking.
/// </summary>
public abstract class MySqlEntityModel : EntityModel
{
}

/// <summary>
/// MySQL-specific entity model with generic identifier support.
/// Provides a string representation of the ID for REST API usage.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class MySqlEntityModel<TId>
	: EntityModel<TId>, IEntityModel<TId>
	where TId : struct
{
	/// <summary>
	/// Gets or sets the identifier as a string for REST API binding and JSON serialization.
	/// </summary>
	/// <remarks>
	/// The getter converts the typed ID to its string representation.
	/// The setter uses TypeDescriptor to safely convert the string back to the target type.
	/// If conversion fails, the ID is set to its default value.
	/// </remarks>
	[JsonIgnore]
	public string IdText
	{
		get => Id.ToString();
		/// <summary>
		/// Sets the identifier from a string value with type conversion.
		/// </summary>
		protected set
		{
			try
			{
				var converter = TypeDescriptor.GetConverter(typeof(TId));
				Id = (TId)converter.ConvertFromString(value);
			}
			catch
			{
				Id = default;
			}
		}
	}

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public new TId Id { get; set; }
}
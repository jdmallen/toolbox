namespace JDMallen.Toolbox.Data.Abstractions.Interfaces;

/// <summary>
/// A set of parameters a user may set in order query a set of entities.
/// </summary>
public interface IQueryParameters
{
	/// <summary>
	/// Gets or sets a filter for entities created before this date and time.
	/// </summary>
	DateTime? DateCreatedBefore { get; set; }

	/// <summary>
	/// Gets or sets a filter for entities created after this date and time.
	/// </summary>
	DateTime? DateCreatedAfter { get; set; }

	/// <summary>
	/// Gets or sets a filter for entities modified before this date and time.
	/// </summary>
	DateTime? DateModifiedBefore { get; set; }

	/// <summary>
	/// Gets or sets a filter for entities modified after this date and time.
	/// </summary>
	DateTime? DateModifiedAfter { get; set; }

	/// <summary>
	/// Gets or sets the number of entities to skip for pagination.
	/// </summary>
	int? Skip { get; set; }

	/// <summary>
	/// Gets or sets the maximum number of entities to return.
	/// </summary>
	int? Take { get; set; }

	/// <summary>
	/// Gets or sets the property name to sort by.
	/// </summary>
	string SortBy { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to sort in ascending order.
	/// If false, sorts in descending order.
	/// </summary>
	bool SortAscending { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to include soft-deleted entities.
	/// </summary>
	bool IsDeleted { get; set; }
}

/// <summary>
/// A set of parameters a user may set in order query a set of entities with typed primary keys.
/// </summary>
/// <typeparam name="TId">The primary key type.</typeparam>
public interface IQueryParameters<TId> : IQueryParameters
	where TId : struct
{
	/// <summary>
	/// Gets or sets a single ID to filter by.
	/// </summary>
	TId? Id { get; set; }

	/// <summary>
	/// Gets or sets a collection of IDs to filter by (OR operation).
	/// </summary>
	ICollection<TId> Ids { get; set; }
}

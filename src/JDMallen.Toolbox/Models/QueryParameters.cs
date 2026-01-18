using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JDMallen.Toolbox.Models;

/// <summary>
/// Base class for query parameters used to filter and sort entity collections.
/// </summary>
[UsedImplicitly]
public abstract class QueryParameters : IQueryParameters
{
	/// <summary>
	/// Gets or sets the search style to use for string matching operations.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public SearchStyle SearchStyle { get; set; } = SearchStyle.Exact;

	/// <summary>
	/// Gets or sets the upper bound date filter for the creation date.
	/// </summary>
	public DateTime? DateCreatedBefore { get; set; }

	/// <summary>
	/// Gets or sets the lower bound date filter for the creation date.
	/// </summary>
	public DateTime? DateCreatedAfter { get; set; }

	/// <summary>
	/// Gets or sets the upper bound date filter for the modification date.
	/// </summary>
	public DateTime? DateModifiedBefore { get; set; }

	/// <summary>
	/// Gets or sets the lower bound date filter for the modification date.
	/// </summary>
	public DateTime? DateModifiedAfter { get; set; }

	/// <summary>
	/// Gets or sets the number of items to skip from the beginning of the result set.
	/// </summary>
	public int? Skip { get; set; }

	/// <summary>
	/// Gets or sets the maximum number of items to return.
	/// </summary>
	public int? Take { get; set; }

	/// <summary>
	/// Gets or sets the property name to sort results by.
	/// </summary>
	public string? SortBy { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to sort in ascending (true) or descending (false) order.
	/// </summary>
	public bool SortAscending { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether to include soft-deleted items in the results.
	/// </summary>
	public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// Base class for query parameters with generic identifier support.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
[UsedImplicitly]
public abstract class QueryParameters<TId>
	: QueryParameters, IQueryParameters<TId>
	where TId : struct
{
	/// <summary>
	/// Initializes a new instance of the <see cref="QueryParameters{TId}"/> class.
	/// </summary>
	/// <summary>
	/// Initializes a new instance of the <see cref="QueryParameters{TId}"/> class.
	/// </summary>
	protected QueryParameters()
	{
		Ids = new List<TId>();
	}

	/// <summary>
	/// Gets or sets a single entity identifier to filter by.
	/// </summary>
	public TId? Id { get; set; }

	/// <summary>
	/// Gets or sets a collection of entity identifiers to filter by.
	/// </summary>
	public ICollection<TId> Ids { get; set; }
}
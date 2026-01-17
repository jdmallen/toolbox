using System;
using System.Collections.Generic;
using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JDMallen.Toolbox.Models;

/// <inheritdoc />
/// <summary>
/// </summary>
public abstract class QueryParameters : IQueryParameters
{
	[JsonConverter(typeof(StringEnumConverter))]
	public SearchStyle SearchStyle { get; set; } = SearchStyle.Exact;

	public DateTime? DateCreatedBefore { get; set; }

	public DateTime? DateCreatedAfter { get; set; }

	public DateTime? DateModifiedBefore { get; set; }

	public DateTime? DateModifiedAfter { get; set; }

	public int? Skip { get; set; }

	public int? Take { get; set; }

	public string SortBy { get; set; }

	public bool SortAscending { get; set; } = true;

	public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// Base class for query parameters with generic identifier support.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class QueryParameters<TId>
	: QueryParameters, IQueryParameters<TId>
	where TId : struct
{
	/// <summary>
	/// Initializes a new instance of the <see cref="QueryParameters{TId}"/> class.
	/// </summary>
	protected QueryParameters()
	{
		Ids = new List<TId>();
	}

	public TId? Id { get; set; }

	public ICollection<TId> Ids { get; set; }
}
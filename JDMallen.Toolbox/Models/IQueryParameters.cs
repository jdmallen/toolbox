using System;
using System.Collections.Generic;

namespace JDMallen.Toolbox.Models
{
	/// <summary>
	/// A set of parameters a user may set in order query a set of entities.
	/// </summary>
    public interface IQueryParameters
    {
	    bool IncludeNestedEntities { get; set; }

		bool TrackEntities { get; set; }

		DateTime? DateCreated { get; set; }

		DateTime? DateModified { get; set; }

		int? Skip { get; set; }

		int? Take { get; set; }

		string SortBy { get; set; }

		string SortDirection { get; set; }
    }

	/// <inheritdoc />
	/// <summary>
	/// </summary>
	/// <typeparam name="TId">The type of the primary key to search.</typeparam>
	public interface IQueryParameters<TId> : IQueryParameters
	{
		TId Id { get; set; }

		IEnumerable<TId> Ids { get; set; }
	}
}

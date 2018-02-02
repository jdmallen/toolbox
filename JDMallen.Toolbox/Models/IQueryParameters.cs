using System;
using System.Collections.Generic;

namespace JDMallen.Toolbox.Models
{
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

	public interface IQueryParameters<TId> : IQueryParameters
	{
		TId Id { get; set; }

		IEnumerable<TId> Ids { get; set; }
	}

}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JDMallen.Toolbox.Models
{
	/// <summary>
	/// A set of parameters a user may set in order query a set of entities.
	/// </summary>
    public interface IQueryParameters
	{
		string Id { get; set; }

		IEnumerable<string> Ids { get; set; }

		bool IncludeNestedEntities { get; set; }

		bool TrackEntities { get; set; }

		DateTime? DateCreatedBefore { get; set; }

		DateTime? DateCreatedAfter { get; set; }

		DateTime? DateModifiedBefore { get; set; }

		DateTime? DateModifiedAfter { get; set; }

		int Skip { get; set; }

		int Take { get; set; }

		string SortBy { get; set; }

		ListSortDirection SortDirection { get; set; }
    }
}

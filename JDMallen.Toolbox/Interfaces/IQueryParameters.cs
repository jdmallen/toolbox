﻿using System;
using System.Collections.Generic;

namespace JDMallen.Toolbox.Interfaces
{
	/// <summary>
	/// A set of parameters a user may set in order query a set of entities.
	/// </summary>
	public interface IQueryParameters
	{
		DateTime? DateCreatedBefore { get; set; }

		DateTime? DateCreatedAfter { get; set; }

		DateTime? DateModifiedBefore { get; set; }

		DateTime? DateModifiedAfter { get; set; }

		int? Skip { get; set; }

		int? Take { get; set; }

		string SortBy { get; set; }

		bool SortAscending { get; set; }

		bool IsDeleted { get; set; }
	}

	/// <summary>
	/// A set of parameters a user may set in order query a set of entities.
	/// </summary>
	public interface IQueryParameters<TId> : IQueryParameters
		where TId : struct
	{
		TId? Id { get; set; }

		ICollection<TId> Ids { get; set; }
	}
}

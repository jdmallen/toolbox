using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JDMallen.Toolbox.Models
{
	/// <inheritdoc />
	/// <summary>
	/// By default, <see cref="AutomaticallyIncludeFirstLevelEntities"/> and <see cref="TrackEntities"/> are false.
	/// </summary>
	public abstract class QueryParameters : IQueryParameters
	{
		protected QueryParameters()
		{
			Ids = new List<string>();
		}

		public string Id { get; set; }

		public IEnumerable<string> Ids { get; set; }

		public bool AutomaticallyIncludeFirstLevelEntities { get; set; } = false;

		public bool TrackEntities { get; set; } = false;

		public DateTime? DateCreatedBefore { get; set; }
		 
		public DateTime? DateCreatedAfter { get; set; }
		 
		public DateTime? DateModifiedBefore { get; set; }
		 
		public DateTime? DateModifiedAfter { get; set; }

		public int Skip { get; set; } = -1;

		public int Take { get; set; } = -1;

		public string SortBy { get; set; }

		public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;
	}
}
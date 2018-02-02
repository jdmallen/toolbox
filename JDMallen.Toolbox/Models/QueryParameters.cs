using System;
using System.Collections.Generic;

namespace JDMallen.Toolbox.Models
{
	public abstract class QueryParameters<TId> : IQueryParameters<TId>
	{
		protected QueryParameters()
		{
			Ids = new List<TId>();
		}

		public bool IncludeNestedEntities { get; set; }

		public bool TrackEntities { get; set; }

		public DateTime? DateCreated { get; set; }

		public DateTime? DateModified { get; set; }

		public int? Skip { get; set; }

		public int? Take { get; set; }

		public string SortBy { get; set; }

		public string SortDirection { get; set; }

		public TId Id { get; set; }

		public IEnumerable<TId> Ids { get; set; }
	}
}
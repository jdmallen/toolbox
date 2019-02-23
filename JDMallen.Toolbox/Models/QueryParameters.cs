using System;
using System.Collections.Generic;
using JDMallen.Toolbox.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JDMallen.Toolbox.Models
{
	/// <inheritdoc />
	/// <summary>
	/// By default, <see cref="AutomaticallyIncludeFirstLevelEntities"/> and <see cref="TrackEntities"/> are false.
	/// </summary>
	public abstract class QueryParameters : IQueryParameters
	{
		public bool AutomaticallyIncludeFirstLevelEntities { get; set; } =
			false;

		public bool TrackEntities { get; set; } = false;

		public DateTime? DateCreatedBefore { get; set; }

		public DateTime? DateCreatedAfter { get; set; }

		public DateTime? DateModifiedBefore { get; set; }

		public DateTime? DateModifiedAfter { get; set; }

		public int Skip { get; set; } = -1;

		public int Take { get; set; } = -1;

		public string SortBy { get; set; }

		public bool SortAscending { get; set; } = true;

		public bool IsDeleted { get; set; } = false;

		[JsonConverter(typeof(StringEnumConverter))]
		public SearchStyle SearchStyle { get; set; } = SearchStyle.Exact;
	}

	public abstract class QueryParameters<TId>
		: QueryParameters, IQueryParameters<TId>
		where TId : struct
	{
		protected QueryParameters()
		{
			Ids = new List<TId>();
		}

		public TId? Id { get; set; }

		public ICollection<TId> Ids { get; set; }
	}
}

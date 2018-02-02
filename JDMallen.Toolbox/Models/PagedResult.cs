using System;
using System.Collections.Generic;

namespace JDMallen.Toolbox.Models
{
	public class PagedResult<TModel> : IPagedResult<TModel>
		where TModel : class, IModel
	{
		public int Skipped { get; set; }

		public int Taken { get; set; }

		public long TotalItemCount { get; set; }

		public int PageIndex
		{
			get
			{
				if (Taken == 0 || Taken > TotalItemCount) return 1;
				return Skipped / Taken + 1;
			}
		}

		public int TotalPageCount
		{
			get
			{
				if (Taken == 0 || Taken > TotalItemCount) return 1;
				return (int) Math.Ceiling((double) TotalItemCount / Taken);
			}
		}

		public IEnumerable<TModel> Items { get; set; }
	}
}

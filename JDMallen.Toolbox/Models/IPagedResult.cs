using System.Collections.Generic;

namespace JDMallen.Toolbox.Models
{
	public interface IPagedResult<TModel> where TModel : IModel
	{
		int Skipped { get; set; }

		int Taken { get; set; }

		long TotalItemCount { get; set; }

		int PageIndex { get; }

		int TotalPageCount { get; }

		IEnumerable<TModel> Items { get; set; }
	}
}

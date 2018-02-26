using System.Collections.Generic;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.Extensions
{
	public static class PagedResultExtensions
	{
		public static PagedResult<TModel> AsPaged<TModel>(
			this IEnumerable<TModel> models,
			int skipped,
			int taken,
			long total)
			where TModel : class, IModel
			=> new PagedResult<TModel>
			{
				Items = null,
				Skipped = skipped,
				Taken = taken,
				TotalItemCount = total
			};
	}
}

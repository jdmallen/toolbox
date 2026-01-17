using System.Collections.Generic;
using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.Extensions;

public static class PagedResultExtensions
{
	public static PagedResult<TModel> AsPaged<TModel>(
		this IEnumerable<TModel> models,
		int skipped,
		int taken,
		long total)
		where TModel : class, IModel
	{
		return new PagedResult<TModel>
		{
			Items = models,
			Skipped = skipped < 0 ? 0 : skipped,
			Taken = (int)(taken < 0 ? total : taken),
			TotalItemCount = total
		};
	}
}
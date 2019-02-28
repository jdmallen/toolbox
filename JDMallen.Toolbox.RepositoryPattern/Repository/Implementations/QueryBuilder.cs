using System.Linq;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;
using JDMallen.Toolbox.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Implementations
{
	public class QueryBuilder<TEntityModel>
		where TEntityModel : class, IEntityModel
	{
		/// <summary>
		/// Credit:
		/// https://github.com/dotnet-architecture/eShopOnWeb/blob/master/src/Infrastructure/Data/SpecificationEvaluator.cs
		/// </summary>
		/// <param name="inputQuery"></param>
		/// <param name="specification"></param>
		/// <returns></returns>
		public static IQueryable<TEntityModel> Build(
			IQueryable<TEntityModel> inputQuery,
			ISpecification<TEntityModel> specification)
		{
			var query = inputQuery;

			if (specification.Criteria != null)
			{
				// soooo much better than a sea of if (... HasValue) { } blocks
				query = query.Where(specification.Criteria);
			}

			query = specification.Includes.Aggregate(
				query,
				(entities, expression) => entities.Include(expression));

			query = specification.IncludeStrings.Aggregate(
				query,
				(entities, includeString) => entities.Include(includeString));

			if (specification.OrderBy != null)
			{
				query = query.OrderBy(specification.OrderBy);
			}
			else if (specification.OrderByDescending != null)
			{
				query = query.OrderByDescending(specification.OrderByDescending);
			}

			if (specification.IsPagingEnabled)
			{
				query = query.Skip(specification.Skip).Take(specification.Take);
			}

			return query;
		}
	}
}

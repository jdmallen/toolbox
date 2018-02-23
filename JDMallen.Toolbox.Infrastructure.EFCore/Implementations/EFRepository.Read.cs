using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JDMallen.Toolbox.Extensions;
using JDMallen.Toolbox.Infrastructure.EFCore.Models;
using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract partial class EFRepository<TContext, TEntityModel, TQueryParameters, TId>
		where TContext : class, IEFContext
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		public override Task<bool> Any(TQueryParameters parameters)
			=> BuildQuery(parameters).AnyAsync();

		public override Task<long> Count(TQueryParameters parameters)
			=> BuildQuery(parameters).LongCountAsync();

		public override Task<TEntityModel> Get(TId id)
			=> BuildQuery(null).SingleOrDefaultAsync(x => id.Equals(x.Id));

		public override Task<TEntityModel> Find(TQueryParameters parameters)
			=> BuildQuery(parameters).FirstOrDefaultAsync();

		public override Task<List<TEntityModel>> FindAll(TQueryParameters parameters)
			=> BuildQuery(parameters).ToListAsync();

		public override async Task<IPagedResult<TEntityModel>> FindAllPaged(TQueryParameters parameters)
		{
			var count = await Count(parameters);

			var query = BuildQuery(parameters);

			if (parameters.Skip > -1)
				query = query.Skip(parameters.Skip);

			if (parameters.Take > -1)
				query = query.Take(parameters.Take);

			var results = await query.ToListAsync();

			return results.AsPaged(parameters.Skip, parameters.Take, count);
		}
	}
}

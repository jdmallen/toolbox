﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JDMallen.Toolbox.Extensions;
using JDMallen.Toolbox.Infrastructure.EFCore.Models;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract partial class EFRepositoryBase<TContext, TEntityModel, TQueryParameters, TId>
		where TContext : class, IEFContext
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		public Task<bool> Any(TQueryParameters parameters)
			=> BuildQuery(parameters).AnyAsync();

		public Task<long> Count(TQueryParameters parameters)
			=> BuildQuery(parameters).LongCountAsync();

		public Task<TEntityModel> Get(TId id)
			=> BuildQuery(null).SingleOrDefaultAsync(x => id.Equals(x.Id));

		public Task<List<TEntityModel>> Find(TQueryParameters parameters)
			=> BuildQuery(parameters).ToListAsync();

		public async Task<IPagedResult<TEntityModel>> FindPaged(TQueryParameters parameters)
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

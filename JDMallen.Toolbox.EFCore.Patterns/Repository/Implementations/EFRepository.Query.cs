using System;
using System.Linq;
using JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;
using JDMallen.Toolbox.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Implementations
{
	public abstract partial class EFRepositoryBase<
		TContext,
		TEntityModel,
		TId> : IRepository<TEntityModel, TId>
		where TContext : DbContext, IContext
		where TEntityModel : class, IEntityModel<TId>
		where TId : struct
	{
		protected EFRepositoryBase(TContext context)
		{
			Context = context;
		}

		protected TContext Context { get; }

		private IQueryable<TEntityModel> ApplySpecification(
			ISpecification<TEntityModel> specification)
			=> QueryBuilder<TEntityModel>.Build(
				Context.Set<TEntityModel>().AsQueryable(),
				specification);

		#region IDisposable

		private bool _disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Context.Dispose();
				}
			}

			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}

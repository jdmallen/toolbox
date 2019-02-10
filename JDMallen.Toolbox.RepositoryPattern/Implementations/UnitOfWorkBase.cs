using System;
using System.Data;
using System.Linq;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public abstract class UnitOfWorkBase<TContext>
		: IUnitOfWork
		where TContext : IContext
	{
		protected bool Disposed;

		protected UnitOfWorkBase(TContext context)
		{
			Context = context;
			Connection = context.GetConnection();
			Id = Guid.NewGuid();
			Connection.Open();
			Transaction = Connection.BeginTransaction();
			State = UnitOfWorkState.Open;
		}

		protected TContext Context { get; }

		public Guid Id { get; }

		public IDbConnection Connection { get; protected set; }

		public IDbTransaction Transaction { get; protected set; }

		public UnitOfWorkState State { get; protected set; }
		
		public virtual void Commit()
		{
			try
			{
				Transaction.Commit();
				State = UnitOfWorkState.Committed;
			}
			catch
			{
				Rollback();
				throw;
			}
		}

		public IRepository GetRepository(string name)
		{
			var repo = GetType()
				.GetProperties(
				)
				.Where(
					prop => Enumerable.Contains(
						prop.PropertyType.GetInterfaces(),
						typeof(IRepository)))
				.FirstOrDefault(
					prop => prop.PropertyType.Name.IndexOf(
						        name,
						        StringComparison.InvariantCultureIgnoreCase)
					        != -1);
			return (IRepository) repo?.GetValue(this);
		}

		protected abstract void ResetRepositories();

		public void ResetUnitOfWork()
		{
			Transaction = Connection.BeginTransaction();
			State = UnitOfWorkState.Open;
			ResetRepositories();
		}

		public virtual void Rollback()
		{
			Transaction.Rollback();
			State = UnitOfWorkState.RolledBack;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (Disposed)
				return;
			if (disposing)
			{
				Transaction?.Dispose();
				Connection.Close();
				Connection.Dispose();
				ResetUnitOfWork();
			}

			Connection = null;
			Transaction = default(IDbTransaction);
			Disposed = true;
			State = UnitOfWorkState.Disposed;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~UnitOfWorkBase()
		{
			Dispose(false);
		}
	}
}

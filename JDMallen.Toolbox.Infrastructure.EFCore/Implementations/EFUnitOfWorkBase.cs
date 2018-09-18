using System;
using System.Data;
using JDMallen.Toolbox.Infrastructure.EFCore.Models;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract class EFUnitOfWorkBase<TContext>
		: IUnitOfWork<IDbContextTransaction>
		where TContext : DbContext, IEFContext
	{
		private bool _disposed;

		protected TContext Context;

		protected EFUnitOfWorkBase(TContext context)
		{
			Context = context;
			Id = Guid.NewGuid();
			State = UnitOfWorkState.New;
		}

		public Guid Id { get; }

		public IDbConnection Connection => Context.Database.GetDbConnection();

		public IDbContextTransaction Transaction { get; private set; }

		public UnitOfWorkState State { get; private set; }

		public void Begin()
		{
			Transaction = Context.Database.BeginTransaction();
			State = UnitOfWorkState.Open;
		}

		public void Commit()
		{
			try
			{
				Transaction.Commit();
			}
			catch
			{
				Rollback();
				throw;
			}
			finally
			{
				Dispose();
				State = UnitOfWorkState.Committed;
			}
		}

		public void Rollback()
		{
			try
			{
				Transaction.Rollback();
			}
			finally
			{
				Dispose();
				State = UnitOfWorkState.RolledBack;
			}
		}

		public abstract void NullRepositories();

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				Transaction?.Dispose();
				Connection.Close();
				Connection.Dispose();
				NullRepositories();
			}

			Context = null;
			Transaction = null;
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~EFUnitOfWorkBase()
		{
			Dispose(false);
		}
	}
}

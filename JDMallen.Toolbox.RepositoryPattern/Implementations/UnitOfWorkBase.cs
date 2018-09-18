using System;
using System.Data;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public abstract class UnitOfWorkBase<TContext, TTransaction>
		: IUnitOfWork<TTransaction>
		where TContext : IContext
		where TTransaction : IDbTransaction
	{
		private bool _disposed;

		protected UnitOfWorkBase(TContext connectionFactory)
		{
			Connection = connectionFactory.GetConnection();
			Id = Guid.NewGuid();
			State = UnitOfWorkState.New;
		}

		public Guid Id { get; }

		public IDbConnection Connection { get; private set; }

		public TTransaction Transaction { get; private set; }

		public UnitOfWorkState State { get; private set; }

		public void Begin()
		{
			Connection.Open();
			Transaction = (TTransaction) Connection.BeginTransaction();
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

			Connection = null;
			Transaction = default(TTransaction);
			_disposed = true;
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

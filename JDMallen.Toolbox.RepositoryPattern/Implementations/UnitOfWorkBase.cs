using System;
using System.Data;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public abstract class UnitOfWorkBase<TContext> : IUnitofWork
		where TContext : IContext
	{
		private bool _disposed = false;
		private IDbConnection _connection;
		private UnitOfWorkState _state;

		protected UnitOfWorkBase(TContext connectionFactory)
		{
			_connection = connectionFactory.GetConnection();
			Id = Guid.NewGuid();
			_state = UnitOfWorkState.New;
		}

		public Guid Id { get; }

		public IDbConnection Connection => _connection;

		public IDbTransaction Transaction { get; private set; }

		public UnitOfWorkState State => _state;

		public void Begin()
		{
			Connection.Open();
			Transaction = Connection.BeginTransaction();
			_state = UnitOfWorkState.Open;
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
				_state = UnitOfWorkState.Committed;
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
				_state = UnitOfWorkState.RolledBack;
			}
		}

		public abstract void NullRepositories();

		protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
							Transaction?.Dispose();
							Connection.Close();
							Connection.Dispose();
							NullRepositories();
            }
						_connection = null;
						Transaction = null;
            _disposed = true;
        }
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

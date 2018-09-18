using System;
using System.Data;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IUnitOfWork<out TTransaction> : IDisposable
	{
		Guid Id { get; }

		IDbConnection Connection { get; }

		TTransaction Transaction { get; }

		UnitOfWorkState State { get; }

		void Begin();

		void Commit();

		void NullRepositories();

		void Rollback();
	}
}

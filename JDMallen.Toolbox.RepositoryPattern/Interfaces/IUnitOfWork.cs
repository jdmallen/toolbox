using System;
using System.Data;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IUnitOfWork
	{
		Guid Id { get; }

		IDbConnection Connection { get; }

		IDbTransaction Transaction { get; }

		UnitOfWorkState State { get; }

		void Commit();

		IRepository GetRepository(string name);

		void ResetUnitOfWork();

		void Rollback();
	}
}

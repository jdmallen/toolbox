using System;
using System.Data;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IUnitofWork : IDisposable
	{
		Guid Id { get; }
		IDbConnection Connection { get; }
		IDbTransaction Transaction { get; }
		UnitOfWorkState State { get; }
		void Begin();
		void Commit();
		void NullRepositories();
		void Rollback();
	}
}

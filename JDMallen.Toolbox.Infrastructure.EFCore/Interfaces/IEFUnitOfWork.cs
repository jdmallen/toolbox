using System;
using System.Data;
using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Interfaces
{
	public interface IEFUnitOfWork : IDisposable
	{
		Guid Id { get; }

		IDbConnection Connection { get; }

		IDbContextTransaction Transaction { get; }

		UnitOfWorkState State { get; }

		void Commit();

		void NullRepositories();

		void Rollback();
	}
}

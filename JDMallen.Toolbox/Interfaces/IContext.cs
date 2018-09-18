﻿using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace JDMallen.Toolbox.Interfaces
{
	/// <summary>
	/// Represents a context from and to which the repository or service can 
	/// create, read, update, and delete data.
	/// </summary>
  public interface IContext
  {
		IDbConnection GetConnection();

		Task<int> SaveChangesAsync(
			CancellationToken cancellationToken = default(CancellationToken));
	}
}

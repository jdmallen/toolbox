using System;
using System.Data;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IRepository : IDisposable
	{
		void SetTransaction(IDbTransaction transaction);
	}
}
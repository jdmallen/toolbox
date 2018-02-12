using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IRepository
	{
	}

	public interface IRepository<out TContext> : IRepository
	    where TContext : IContext
	{
		TContext Context { get; }
	}
}

using JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces;
using JDMallen.Toolbox.EFCore.Services.Interfaces;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.EFCore.Services.Implementations
{
	/// <summary>
	/// An abstract service that can only read data from a data source
	/// </summary>
	/// <typeparam name="TRepository">The repository used to perform the data actions</typeparam>
	/// <typeparam name="TModel">The database entity model type</typeparam>
	/// <typeparam name="TId">The primary key type</typeparam>
	public abstract class ServiceBase<TRepository, TModel, TId>
		: IReadService<TRepository>
		where TRepository : IRepository<TModel, TId>
		where TModel : class, IModel
		where TId : struct
	{
		/// <summary>
		/// Base constructor with DI repository
		/// </summary>
		/// <param name="repository">The repository to use</param>
		protected ServiceBase(TRepository repository)
		{
			Repository = repository;
		}

		/// <summary>
		/// The <see cref="TRepository"/> used to perform all the CRUD actions
		/// </summary>
		public TRepository Repository { get; }
	}
}

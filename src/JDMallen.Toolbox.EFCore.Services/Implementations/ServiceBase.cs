using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces;
using JDMallen.Toolbox.EFCore.Services.Interfaces;

namespace JDMallen.Toolbox.EFCore.Services.Implementations;

/// <summary>
/// An abstract service that can only read data from a data source.
/// </summary>
/// <typeparam name="TRepository">The repository used to perform the data actions.</typeparam>
/// <typeparam name="TModel">The database entity model type.</typeparam>
/// <typeparam name="TId">The primary key type.</typeparam>
public abstract class ServiceBase<TRepository, TModel, TId>
	: IReadService<TRepository>
	where TRepository : IRepository<TModel, TId>
	where TModel : class, IModel
	where TId : struct
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ServiceBase{TRepository, TModel, TId}"/> class.
	/// </summary>
	/// <remarks>
	/// This constructor is protected to ensure the base class is only instantiated
	/// through derived classes that provide additional service functionality.
	/// </remarks>
	/// <param name="repository">The repository to use for data access.</param>
	protected ServiceBase(TRepository repository)
	{
		Repository = repository;
	}

	/// <summary>
	/// The <see cref="TRepository" /> used to perform all the CRUD actions
	/// </summary>
	public TRepository Repository { get; }
}

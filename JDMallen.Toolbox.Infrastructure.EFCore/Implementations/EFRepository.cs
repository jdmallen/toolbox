using JDMallen.Toolbox.Infrastructure.EFCore.Config;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Implementations;
using System.Linq;
using System.Reflection;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract partial class EFRepository<TDomainModel, TEntityModel, TQueryParameters, TId>
		: Repository<EFContextBase, TDomainModel, TEntityModel, TQueryParameters, TId>
		where TDomainModel : class, IDomainModel<TId>
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		protected EFRepository(EFContextBase context) : base(context)
		{
		}

		/// <summary>
		/// https://stackoverflow.com/a/8181736/3986790
		/// </summary>
		/// <param name="oldEntity"></param>
		/// <param name="newEntity"></param>
		private static void CopyProps(ref TEntityModel oldEntity, ref TEntityModel newEntity)
		{
			TEntityModel old = newEntity;
			TEntityModel newish = oldEntity;
			typeof(TEntityModel)
				.GetFields(BindingFlags.Public
				           | BindingFlags.Instance)
				.ToList()
				.ForEach(field => field.SetValue(old, field.GetValue(newish)));
			oldEntity = old;
			newEntity = newish;
		}
	}
}

using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Config
{
	public class EntityContext : DbContext, IContext
	{
		public EntityContext(DbContextOptions options) : base(options)
		{
		}
	}
}

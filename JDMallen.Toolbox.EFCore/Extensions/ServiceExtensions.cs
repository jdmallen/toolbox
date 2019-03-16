using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JDMallen.Toolbox.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace JDMallen.Toolbox.EFCore.Extensions
{
	public static class ServiceExtensions
	{
		public static IdentityBuilder AddCustomIdentity<
			TDbContext,
			TUser,
			TRole,
			TId,
			TUserStore,
			TValidator,
			TErrorDescriber,
			TUserClaim,
			TUserRole,
			TUserLogin,
			TUserToken,
			TRoleClaim>(
			this IServiceCollection services,
			Action<IdentityOptions> identityOptions = null,
			Action<PasswordHasherOptions> hasherOptions = null)
			where TDbContext : DbContext
			where TUser : IdentityUser<TId>
			where TRole : IdentityRole<TId>
			where TId : struct, IEquatable<TId>
			where TUserStore : UserStore<
				TUser,
				TRole,
				TDbContext,
				TId,
				TUserClaim,
				TUserRole,
				TUserLogin,
				TUserToken,
				TRoleClaim>
			where TValidator : class, IPasswordValidator<TUser>
			where TErrorDescriber : IdentityErrorDescriber
			where TUserClaim : IdentityUserClaim<TId>, new()
			where TUserRole : IdentityUserRole<TId>, new()
			where TUserLogin : IdentityUserLogin<TId>, new()
			where TUserToken : IdentityUserToken<TId>, new()
			where TRoleClaim : IdentityRoleClaim<TId>, new()
		{
			if (identityOptions != null)
			{
				services.Configure(identityOptions);
			}

			if (hasherOptions != null)
			{
				services.Configure(hasherOptions);
			}

			return services.AddIdentity<TUser, TRole>()
				.AddEntityFrameworkStores<TDbContext>()
				.AddDefaultTokenProviders()
				.AddUserStore<TUserStore>()
				.AddPasswordValidator<TValidator>()
				.AddErrorDescriber<TErrorDescriber>();
		}

		/// <summary>
		/// CAREFUL with this! It does exactly as it describes with no warning:
		/// drops all the tables and then recreates them. Useful for designing
		/// the structure of your entities/DB, but disastrous for production.
		/// </summary>
		public static void DropTablesAndEnsureCreated(
			this DbContext dbContext,
			bool dropTables = true,
			IEnumerable<string> orderedTablesToDrop = null)
		{
			if (dropTables)
			{
				List<string> orderOfDroppage;
				if (orderedTablesToDrop != null)
				{
					orderOfDroppage = orderedTablesToDrop.ToList();
				}
				else
				{
					orderOfDroppage = dbContext.Model.GetEntityTypes()
						.Select(
							et => et.GetAnnotations()
								      .FirstOrDefault(
									      x
										      => x.Name
										         == "Relational:TableName")
								      ?.Value.ToString()
								      .ToLowerInvariant()
							      ?? et.ClrType.Name
								      .ToLowerInvariant())
						.ToList();
				}

				var conn = dbContext.Database.GetDbConnection();
				conn.Open();
				orderOfDroppage
					.ForEach(
						tableName =>
						{
							bool tableExists;
							using (var cmd = conn.CreateCommand())
							{
								cmd.CommandText =
									"SELECT COUNT(*) FROM information_schema.TABLES "
									+ "WHERE TABLE_CATALOG = "
									+ $"\'{dbContext.Database.GetDbConnection().Database}\' "
									+ $"AND TABLE_NAME = \'{tableName.ToLowerInvariant()}\';";
								cmd.CommandType = CommandType.Text;
								using (var reader =
									cmd.ExecuteReader())
								{
									reader.Read();
									tableExists =
										reader.GetInt32(0) > 0;
									reader.Close();
								}
							}

							if (!tableExists)
								return;

							var dropTable =
								$"DROP TABLE {tableName.ToLowerInvariant()};";
							dbContext.Database.ExecuteSqlCommand(dropTable);
						});
				conn.Close();
			}

			dbContext.Database.EnsureCreated();
		}

		public static IdentityBuilder AddCustomIdentity<
			TDbContext, TUser, TRole, TId, TUserStore, TValidator>(
			this IServiceCollection services,
			Action<IdentityOptions> identityOptions = null,
			Action<PasswordHasherOptions> hasherOptions = null)
			where TDbContext : DbContext
			where TUser : IdentityUser<TId>
			where TRole : IdentityRole<TId>
			where TValidator : class, IPasswordValidator<TUser>
			where TId : struct, IEquatable<TId>
			where TUserStore : UserStore<
				TUser,
				TRole,
				TDbContext,
				TId,
				IdentityUserClaim<TId>,
				IdentityUserRole<TId>,
				IdentityUserLogin<TId>,
				IdentityUserToken<TId>,
				IdentityRoleClaim<TId>>
		{
			return AddCustomIdentity<TDbContext, TUser, TRole, TId, TUserStore,
				TValidator, IdentityErrorDescriber, IdentityUserClaim<TId>,
				IdentityUserRole<TId>, IdentityUserLogin<TId>,
				IdentityUserToken<TId>, IdentityRoleClaim<TId>>(
				services,
				identityOptions,
				hasherOptions);
		}

		public static IdentityBuilder AddCustomIdentity<
			TDbContext, TUser, TRole>(
			this IServiceCollection services,
			Action<IdentityOptions> identityOptions = null,
			Action<PasswordHasherOptions> hasherOptions = null)
			where TDbContext : DbContext
			where TUser : IdentityUser<Guid>
			where TRole : IdentityRole<Guid>
		{
			return AddCustomIdentity<
				TDbContext,
				TUser,
				TRole,
				Guid,
				UserStore<
					TUser,
					TRole,
					TDbContext,
					Guid,
					IdentityUserClaim<Guid>,
					IdentityUserRole<Guid>,
					IdentityUserLogin<Guid>,
					IdentityUserToken<Guid>,
					IdentityRoleClaim<Guid>>,
				CustomPasswordValidator<TUser>>(
				services,
				identityOptions,
				hasherOptions);
		}
	}
}

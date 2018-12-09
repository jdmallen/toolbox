using System;
using System.Data;
using System.Linq;
using JDMallen.Toolbox.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
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
			bool dropTables = true)
		{
			if (dropTables)
			{
				var conn = dbContext.Database.GetDbConnection();
				conn.Open();
				dbContext.Model.GetEntityTypes()
					.ToList()
					.ForEach(
						et =>
						{
							bool tableExists;
							var tableName =
								et.GetAnnotations()
									.FirstOrDefault(
										x
											=> x.Name
											   == "Relational:TableName")
									?.Value.ToString()
									.ToLowerInvariant()
								?? et.ClrType.Name
									.ToLowerInvariant();
							using (var cmd = conn.CreateCommand())
							{
								cmd.CommandText =
									"SELECT COUNT(*) FROM information_schema.TABLES "
									+ "WHERE TABLE_SCHEMA = "
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
							var dropCommand =
								"SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, "
								+ "FOREIGN_KEY_CHECKS=0;"
								+ $"DROP TABLE `{tableName.ToLowerInvariant()}`;"
								+ "SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;";
							dbContext.Database.ExecuteSqlCommand(
								dropCommand);
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

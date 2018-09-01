using System;
using JDMallen.Toolbox.Models;
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

		// public static IdentityBuilder AddCustomIdentity<TDbContext, TUser, TRole, TValidator, TId>(
		// 	this IServiceCollection services,
		// 	Action<IdentityOptions> identityOptions = null,
		// 	Action<PasswordHasherOptions> hasherOptions = null)
		// 	where TDbContext : DbContext
		// 	where TUser : IdentityUser<TId>
		// 	where TRole : IdentityRole<TId>
		// 	where TValidator : class, IPasswordValidator<TUser>
		// 	where TId : struct, IEquatable<TId>
		// {
		// 	return AddCustomIdentity<TDbContext, TUser, TRole, TValidator, CustomIdentityErrorDescriber, TId>(
		// 		services,
		// 		identityOptions,
		// 		hasherOptions);
		// }

		// public static IdentityBuilder AddCustomIdentity<TDbContext, TUser, TRole, TValidator>(
		// 	this IServiceCollection services,
		// 	Action<IdentityOptions> identityOptions = null,
		// 	Action<PasswordHasherOptions> hasherOptions = null)
		// 	where TDbContext : DbContext
		// 	where TUser : IdentityUser<Guid>
		// 	where TRole : IdentityRole<Guid>
		// 	where TValidator : class, IPasswordValidator<TUser>
		// {
		// 	return AddCustomIdentity<TDbContext, TUser, TRole, TValidator, Guid>(
		// 		services,
		// 		identityOptions,
		// 		hasherOptions);
		// }



		// public static IdentityBuilder AddCustomIdentity<TDbContext, TUser, TRole>(
		// 	this IServiceCollection services,
		// 	Action<IdentityOptions> identityOptions = null,
		// 	Action<PasswordHasherOptions> hasherOptions = null)
		// 	where TDbContext : DbContext
		// 	where TUser : IdUser
		// 	where TRole : IdentityRole<Guid>
		// {
		// 	return AddCustomIdentity<TDbContext, TUser, TRole, CustomPasswordValidator<TUser>>(
		// 		services,
		// 		identityOptions,
		// 		hasherOptions);
		// }
	}
}

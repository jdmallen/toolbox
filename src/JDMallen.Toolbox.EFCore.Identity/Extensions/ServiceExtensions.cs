using JDMallen.Toolbox.AspNetCore.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JDMallen.Toolbox.EFCore.Identity.Extensions;

/// <summary>
/// Extension methods for configuring ASP.NET Core Identity with Entity Framework
/// Core.
/// </summary>
public static class ServiceExtensions
{
	/// <summary>
	/// Adds custom Identity services with full configuration options for all Identity
	/// entities.
	/// </summary>
	/// <typeparam name="TDbContext">The Entity Framework DbContext type.</typeparam>
	/// <typeparam name="TUser">The user type.</typeparam>
	/// <typeparam name="TRole">The role type.</typeparam>
	/// <typeparam name="TId">The primary key type.</typeparam>
	/// <typeparam name="TUserStore">The user store implementation.</typeparam>
	/// <typeparam name="TValidator">The password validator implementation.</typeparam>
	/// <typeparam name="TErrorDescriber">The error describer implementation.</typeparam>
	/// <typeparam name="TUserClaim">The user claim type.</typeparam>
	/// <typeparam name="TUserRole">The user role type.</typeparam>
	/// <typeparam name="TUserLogin">The user login type.</typeparam>
	/// <typeparam name="TUserToken">The user token type.</typeparam>
	/// <typeparam name="TRoleClaim">The role claim type.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <param name="identityOptions">Optional Identity configuration.</param>
	/// <param name="hasherOptions">Optional password hasher configuration.</param>
	/// <returns>An <see cref="IdentityBuilder" /> for further configuration.</returns>
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
		Action<IdentityOptions>? identityOptions = null,
		Action<PasswordHasherOptions>? hasherOptions = null)
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
	/// Adds custom Identity services with default Identity entity types.
	/// </summary>
	/// <typeparam name="TDbContext">The Entity Framework DbContext type.</typeparam>
	/// <typeparam name="TUser">The user type.</typeparam>
	/// <typeparam name="TRole">The role type.</typeparam>
	/// <typeparam name="TId">The primary key type.</typeparam>
	/// <typeparam name="TUserStore">The user store implementation.</typeparam>
	/// <typeparam name="TValidator">The password validator implementation.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <param name="identityOptions">Optional Identity configuration.</param>
	/// <param name="hasherOptions">Optional password hasher configuration.</param>
	/// <returns>An <see cref="IdentityBuilder" /> for further configuration.</returns>
	public static IdentityBuilder AddCustomIdentity<
		TDbContext, TUser, TRole, TId, TUserStore, TValidator>(
		this IServiceCollection services,
		Action<IdentityOptions>? identityOptions = null,
		Action<PasswordHasherOptions>? hasherOptions = null)
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
		return services.AddCustomIdentity<TDbContext, TUser, TRole, TId, TUserStore,
			TValidator, IdentityErrorDescriber, IdentityUserClaim<TId>,
			IdentityUserRole<TId>, IdentityUserLogin<TId>,
			IdentityUserToken<TId>, IdentityRoleClaim<TId>>(
			identityOptions,
			hasherOptions);
	}

	/// <summary>
	/// Adds custom Identity services with GUID primary keys and default
	/// implementations.
	/// This is the simplest overload using CustomPasswordValidator and all default
	/// Identity entity types.
	/// </summary>
	/// <typeparam name="TDbContext">The Entity Framework DbContext type.</typeparam>
	/// <typeparam name="TUser">The user type.</typeparam>
	/// <typeparam name="TRole">The role type.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <param name="identityOptions">Optional Identity configuration.</param>
	/// <param name="hasherOptions">Optional password hasher configuration.</param>
	/// <returns>An <see cref="IdentityBuilder" /> for further configuration.</returns>
	public static IdentityBuilder AddCustomIdentity<
		TDbContext, TUser, TRole>(
		this IServiceCollection services,
		Action<IdentityOptions>? identityOptions = null,
		Action<PasswordHasherOptions>? hasherOptions = null)
		where TDbContext : DbContext
		where TUser : IdentityUser<Guid>
		where TRole : IdentityRole<Guid>
	{
		return services.AddCustomIdentity<
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
			identityOptions,
			hasherOptions);
	}
}

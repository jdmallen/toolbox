using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JDMallen.Toolbox.Utilities;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.Infrastructure.EFCore.Config; 

namespace JDMallen.Toolbox.Infrastructure.EFCore.Utilities 
{
	public class CustomUserStore<
		TUser, 
		TRole, 
		TContext, 
		TKey, 
		TUserClaim, 
		TUserRole, 
		TUserLogin, 
		TUserToken, 
		TRoleClaim> 
		: UserStore<
			TUser, 
			TRole, 
			TContext, 
			TKey, 
			TUserClaim, 
			TUserRole, 
			TUserLogin, 
			TUserToken, 
			TRoleClaim>
		where TUser : IdUser
		where TRole : IdRole
		where TContext : EFContextBase
		where TKey : IEquatable<TKey>
		where TUserClaim : IdentityUserClaim<TKey>, new()
		where TUserRole : IdentityUserRole<TKey>, new()
		where TUserLogin : IdentityUserLogin<TKey>, new()
		where TUserToken : IdentityUserToken<TKey>, new()
		where TRoleClaim : IdentityRoleClaim<TKey>, new()
	{
		public CustomUserStore(
			TContext context, 
			IdentityErrorDescriber describer = null) 
			: base(context, describer)
		{
		}
	}

	public class CustomUserStore<TUser, TRole, TContext, TKey>
		: CustomUserStore<
			TUser,
			TRole,
			TContext,
			TKey,
			IdentityUserClaim<TKey>,
			IdentityUserRole<TKey>,
			IdentityUserLogin<TKey>,
			IdentityUserToken<TKey>,
			IdentityRoleClaim<TKey>>
		where TUser : IdUser<TKey>
		where TRole : IdentityRole<TKey>
		where TContext : DbContext
		where TKey : IEquatable<TKey>
	{
		public CustomUserStore(TContext context, IdentityErrorDescriber describer = null)
		: base(context, describer)
		{
		}
	}
}

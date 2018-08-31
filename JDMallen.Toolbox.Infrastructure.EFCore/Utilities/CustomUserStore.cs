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

namespace JDMallen.Toolbox.Utilities 
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
		where TUser : IdentityUser<TKey>
		where TRole : IdentityRole<TKey>
		where TContext : DbContext
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
}

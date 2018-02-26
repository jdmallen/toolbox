using System;
using System.Text;
using JDMallen.Toolbox.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;

namespace JDMallen.Toolbox.Utilities
{
	public class Pbkdf2PasswordHasher<TUser> : IPasswordHasher<TUser>
	where TUser: IdUser
    {
		/// <summary>
		/// Returns a hashed representation of the supplied <paramref name="password" /> for the specified <paramref name="user" />.
		/// </summary>
		/// <param name="user">The user whose password is to be hashed.</param>
		/// <param name="password">The password to hash.</param>
		/// <returns>A hashed representation of the supplied <paramref name="password" /> for the specified <paramref name="user" />.</returns>
		public string HashPassword(TUser user, string password)
		{
			var keyBytes = KeyDerivation.Pbkdf2(password,
												Encoding.UTF8.GetBytes(user.PasswordSalt),
												KeyDerivationPrf.HMACSHA256,
												10000,
												256 / 8);
			return Convert.ToBase64String(keyBytes);
		}

		/// <summary>
		/// Returns a <see cref="T:Microsoft.AspNetCore.Identity.PasswordVerificationResult" /> indicating the result of a password hash comparison.
		/// </summary>
		/// <param name="user">The user whose password should be verified.</param>
		/// <param name="hashedPassword">The hash value for a user's stored password.</param>
		/// <param name="providedPassword">The password supplied for comparison.</param>
		/// <returns>A <see cref="T:Microsoft.AspNetCore.Identity.PasswordVerificationResult" /> indicating the result of a password hash comparison.</returns>
		/// <remarks>Implementations of this method should be time consistent.</remarks>
		public PasswordVerificationResult VerifyHashedPassword(
			TUser user,
			string hashedPassword,
			string providedPassword)
		{
			var isMatch = HashPassword(user, providedPassword) == hashedPassword;
			return isMatch ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
		}
	}
}

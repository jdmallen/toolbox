using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JDMallen.Toolbox.Constants;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.Options;
using JDMallen.Toolbox.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace JDMallen.Toolbox.Utilities
{
	public class CustomPasswordValidator<TUser> : PasswordValidator<TUser>
		where TUser : IdUser
	{
		private readonly IOptions<PasswordComplexityOptions> _options;
		private readonly CustomIdentityErrorDescriber _errors;

		public CustomPasswordValidator(
			IOptions<PasswordComplexityOptions> options = null,
			CustomIdentityErrorDescriber errors = null)
		{
			_options = options
			           ?? new OptionsWrapper<PasswordComplexityOptions>(new PasswordComplexityOptions());
			_errors = errors ?? new CustomIdentityErrorDescriber(_options);
		}

		/// <summary>Validates a password as an asynchronous operation.</summary>
		/// <param name="manager">The <see cref="T:Microsoft.AspNetCore.Identity.UserManager`1" /> to retrieve the <paramref name="user" /> properties from.</param>
		/// <param name="user">The user whose password should be validated.</param>
		/// <param name="password">The password supplied for validation</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public override async Task<IdentityResult> ValidateAsync(
			UserManager<TUser> manager, 
			TUser user, 
			string password)
		{
			var errors = new List<IdentityError>();

			var result = await base.ValidateAsync(manager, user, password);
			if (!result.Succeeded)
			{
				errors.AddRange(result.Errors);
			}

			var checkPwdResult = CheckPassword(password);
			if (checkPwdResult.IsError)
			{
				if (checkPwdResult.Error.HasFlag(PasswordError.TooCommon))
				{
					errors.Add(_errors.PasswordTooCommon());
				}

				if (checkPwdResult.Error.HasFlag(PasswordError.NotComplexEnough))
				{
					errors.Add(_errors.PasswordNotComplexEnough(checkPwdResult));
				}

				if (checkPwdResult.Error.HasFlag(PasswordError.TooShort))
				{
					errors.Add(_errors.PasswordTooShort(checkPwdResult.Length));
				}

				return IdentityResult.Failed(errors.ToArray());
			}
			else
			{
				return result;
			}
		}

		public const string LowerSet = @"abcdefghijklmnopqrstuvwxyz";

		public const string UpperSet = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public const string NumberSet = @"0123456789";

		public const string SymbolSet1 = @"!@#$%^&*()";

		public const string SymbolSet2 = @"`~-_=+[{]}\|;:'"",<.>/?";

		/// <summary>
		/// http://rumkin.com/tools/password/passchk.php
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		public PasswordResult CheckPassword(string password)
		{
			var result = new PasswordResult { Length = password.Length };
			if (string.IsNullOrWhiteSpace(password) || password.Length < 2)
			{
				result.BitsOfEntropy = 0;
				result.Strength = PasswordStrength.Poor;
				result.Error = PasswordError.TooShort;
				return result;
			}

			var bits = 0F;
			var isCommon = IsCommonPassword(password);
			var len = password.Length;
			var plower = password.ToLowerInvariant();

			var aidx = GetIndex(plower[0]);
			for (var b = 1; b < len; b++)
			{
				var bidx = GetIndex(plower[b]);
				var parseOk =
					float.TryParse(ResourceLoader.FrequencyTable.ToArray()[aidx * 27 + bidx],
						out var freq);
				var c = 1.0F - (parseOk ? freq : 0);
				bits += (float) Math.Log(GetCharacterSet(password), 2) * c * c;
				aidx = bidx;
			}

			result.BitsOfEntropy = bits;
			result.Strength = EvaluateStrength(bits);
			if (isCommon) result.Error = PasswordError.TooCommon;
			if (len < _options.Value.MinimumLength) result.Error = result.Error | PasswordError.TooShort;
			if (bits < _options.Value.BitsThreshold)
				result.Error = result.Error | PasswordError.NotComplexEnough;
			return result;
		}

		private static bool IsCommonPassword(string password)
		{
			return ResourceLoader.CommonPasswords.Contains(password.ToLowerInvariant());
		}

		private static int GetIndex(char c)
		{
			if (c < 'a' || c > 'z') return 0;
			return c - 'a' + 1;
		}

		private static int GetCharacterSet(string password)
		{
			bool containsLower = false,
				containsUpper = false,
				containsNumber = false,
				containsSym1 = false,
				containsSym2 = false,
				containsSpace = false,
				containsOther = false;
			var characters = 0;

			foreach (var c in password)
			{
				var ch = c.ToString();
				if (!containsLower && LowerSet.Contains(ch))
				{
					characters += LowerSet.Length;
					containsLower = true;
				}

				if (!containsUpper && UpperSet.Contains(ch))
				{
					characters += UpperSet.Length;
					containsUpper = true;
				}

				if (!containsNumber && NumberSet.Contains(ch))
				{
					characters += NumberSet.Length;
					containsNumber = true;
				}

				if (!containsSym1 && SymbolSet1.Contains(ch))
				{
					characters += SymbolSet1.Length;
					containsSym1 = true;
				}

				if (!containsSym2 && SymbolSet2.Contains(ch))
				{
					characters += SymbolSet2.Length;
					containsSym2 = true;
				}

				if (!containsSpace && c == ' ')
				{
					characters += 1;
					containsSpace = true;
				}

				if (!containsOther && (c < ' ' || c > '~'))
				{
					characters += 32 + 128;
					containsOther = true;
				}
			}

			return characters;
		}

		private static PasswordStrength EvaluateStrength(float bits)
		{
			if (bits > 120) return PasswordStrength.Excellent;
			if (bits > 90) return PasswordStrength.Great;
			if (bits > 60) return PasswordStrength.Good;
			return bits > 30 ? PasswordStrength.Fair : PasswordStrength.Poor;
		}
	}
}

using JDMallen.Toolbox.AspNetCore.Constants;
using JDMallen.Toolbox.AspNetCore.Models;
using JDMallen.Toolbox.AspNetCore.Options;
using JDMallen.Toolbox.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace JDMallen.Toolbox.AspNetCore.Utilities;

/// <summary>
/// Custom password validator that evaluates password strength using entropy
/// calculation
/// and checks against common password lists.
/// </summary>
/// <typeparam name="TUser">The user type.</typeparam>

// ReSharper disable once ClassNeverInstantiated.Global
public class CustomPasswordValidator<TUser> : PasswordValidator<TUser>
	where TUser : IdentityUser<Guid>
{
	/// <summary>
	/// Character set containing lowercase letters.
	/// </summary>
	public const string LowerSet = "abcdefghijklmnopqrstuvwxyz";

	/// <summary>
	/// Character set containing uppercase letters.
	/// </summary>
	public const string UpperSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	/// <summary>
	/// Character set containing digits.
	/// </summary>
	public const string NumberSet = "0123456789";

	/// <summary>
	/// Character set containing common symbols.
	/// </summary>
	public const string SymbolSet1 = "!@#$%^&*()";

	/// <summary>
	/// Character set containing additional symbols.
	/// </summary>
	public const string SymbolSet2 = @"`~-_=+[{]}\|;:'"",<.>/?";

	private readonly CustomIdentityErrorDescriber _errors;
	private readonly PasswordComplexityOptions _options;

	/// <summary>
	/// Initializes a new instance of the <see cref="CustomPasswordValidator{TUser}" />
	/// class.
	/// </summary>
	/// <param name="options">The password complexity options.</param>
	/// <param name="errors">The custom error describer.</param>
	public CustomPasswordValidator(
		IOptions<PasswordComplexityOptions>? options = null,
		CustomIdentityErrorDescriber? errors = null)
	{
		_options = options?.Value ?? new PasswordComplexityOptions();
		_errors = errors ?? new CustomIdentityErrorDescriber(options!);
	}

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
		bool isCommon = IsCommonPassword(password);
		int len = password.Length;

		// Lowercase is load-bearing here: GetIndex maps 'a'-'z' to frequency-table
		// indices, so the value must be lowercased — uppercasing (CA1308's
		// suggestion) would break the adjacency lookup.
#pragma warning disable CA1308 // Normalize strings to uppercase
		string plower = password.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

		int aidx = GetIndex(plower[0]);
		for (var b = 1; b < len; b++)
		{
			int bidx = GetIndex(plower[b]);
			bool parseOk =
				float.TryParse(
					ResourceLoader.FrequencyTable.ToArray()[aidx * 27 + bidx],
					out float freq);
			float c = 1.0F - (parseOk ? freq : 0);
			bits += (float)Math.Log(GetCharacterSet(password), 2) * c * c;
			aidx = bidx;
		}

		result.BitsOfEntropy = bits;
		result.Strength = EvaluateStrength(bits);
		if (isCommon)
		{
			result.Error = PasswordError.TooCommon;
		}

		if (len < _options.MinimumLength)
		{
			result.Error |= PasswordError.TooShort;
		}

		if (bits < _options.BitsThreshold)
		{
			result.Error |= PasswordError.NotComplexEnough;
		}

		return result;
	}

	private static PasswordStrength EvaluateStrength(float bits)
	{
		return bits switch
		{
			> 120 => PasswordStrength.Excellent,
			> 90  => PasswordStrength.Great,
			> 60  => PasswordStrength.Good,
			_     => bits > 30 ? PasswordStrength.Fair : PasswordStrength.Poor,
		};
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

		foreach (char c in password)
		{
			if (!containsLower && LowerSet.Contains(c, StringComparison.Ordinal))
			{
				characters += LowerSet.Length;
				containsLower = true;
			}

			if (!containsUpper && UpperSet.Contains(c, StringComparison.Ordinal))
			{
				characters += UpperSet.Length;
				containsUpper = true;
			}

			if (!containsNumber && NumberSet.Contains(c, StringComparison.Ordinal))
			{
				characters += NumberSet.Length;
				containsNumber = true;
			}

			if (!containsSym1 && SymbolSet1.Contains(c, StringComparison.Ordinal))
			{
				characters += SymbolSet1.Length;
				containsSym1 = true;
			}

			if (!containsSym2 && SymbolSet2.Contains(c, StringComparison.Ordinal))
			{
				characters += SymbolSet2.Length;
				containsSym2 = true;
			}

			if (!containsSpace && c == ' ')
			{
				characters += 1;
				containsSpace = true;
			}

			// ReSharper disable once InvertIf
			if (!containsOther && c is < ' ' or > '~')
			{
				characters += 32 + 128;
				containsOther = true;
			}
		}

		return characters;
	}

	private static int GetIndex(char c)
	{
		if (c is < 'a' or > 'z')
		{
			return 0;
		}

		return c - 'a' + 1;
	}

	private static bool IsCommonPassword(string password)

		// The common-password resource list is stored lowercase, so the input must
		// be lowercased to match it; ToUpperInvariant (CA1308) would never match.
#pragma warning disable CA1308 // Normalize strings to uppercase
		=> ResourceLoader.CommonPasswords.Contains(password.ToLowerInvariant());
#pragma warning restore CA1308 // Normalize strings to uppercase

	/// <summary>Validates a password as an asynchronous operation.</summary>
	/// <param name="manager">
	/// The <see cref="UserManager{TUser}" /> to retrieve
	/// the
	/// <paramref name="user" /> properties from.
	/// </param>
	/// <param name="user">The user whose password should be validated.</param>
	/// <param name="password">The password supplied for validation</param>
	/// <returns>The task object representing the asynchronous operation.</returns>
	public override async Task<IdentityResult> ValidateAsync(
		UserManager<TUser> manager,
		TUser user,
		string? password)
	{
		var errors = new List<IdentityError>();

		IdentityResult result = await base.ValidateAsync(manager, user, password);
		if (!result.Succeeded)
		{
			errors.AddRange(result.Errors);
		}

		if (password == null)
		{
			return result;
		}

		PasswordResult checkPwdResult = CheckPassword(password);
		if (!checkPwdResult.IsError)
		{
			return result;
		}

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
			errors.Add(_errors.PasswordTooShort(_options.MinimumLength));
		}

		return IdentityResult.Failed(errors.ToArray());
	}
}

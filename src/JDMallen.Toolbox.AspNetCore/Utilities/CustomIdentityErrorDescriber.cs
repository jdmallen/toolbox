using JDMallen.Toolbox.AspNetCore.Models;
using JDMallen.Toolbox.AspNetCore.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace JDMallen.Toolbox.AspNetCore.Utilities;

/// <summary>
/// Custom implementation of Identity error messages with enhanced password
/// validation errors.
/// </summary>
public class CustomIdentityErrorDescriber : IdentityErrorDescriber
{
	private readonly IOptions<PasswordComplexityOptions> _options;

	/// <summary>
	/// Initializes a new instance of the <see cref="CustomIdentityErrorDescriber" />
	/// class.
	/// </summary>
	/// <param name="options">The password complexity options.</param>
	public CustomIdentityErrorDescriber(IOptions<PasswordComplexityOptions> options)
	{
		_options = options;
	}

	/// <inheritdoc />
	public override IdentityError ConcurrencyFailure() => new()
	{
		Code = nameof(ConcurrencyFailure),
		Description = "Optimistic concurrency failure, object has been modified.",
	};

	/// <inheritdoc />
	public override IdentityError DefaultError() => new()
	{
		Code = nameof(DefaultError),
		Description = "An unknown failure has occurred.",
	};

	/// <inheritdoc />
	public override IdentityError DuplicateEmail(string email) => new()
	{
		Code = nameof(DuplicateEmail),
		Description = $"Email '{email}' is already taken.",
	};

	/// <inheritdoc />
	public override IdentityError DuplicateRoleName(string role) => new()
	{
		Code = nameof(DuplicateRoleName),
		Description = $"Role name '{role}' is already taken.",
	};

	/// <inheritdoc />
	public override IdentityError DuplicateUserName(string userName) => new()
	{
		Code = nameof(DuplicateUserName),
		Description = $"User Name '{userName}' is already taken.",
	};

	/// <inheritdoc />
	public override IdentityError InvalidEmail(string? email) => new()
	{
		Code = nameof(InvalidEmail),
		Description = $"Email '{email}' is invalid.",
	};

	/// <inheritdoc />
	public override IdentityError InvalidRoleName(string? role) => new()
	{
		Code = nameof(InvalidRoleName),
		Description = $"Role name '{role}' is invalid.",
	};

	/// <inheritdoc />
	public override IdentityError InvalidToken() => new()
	{
		Code = nameof(InvalidToken),
		Description = "Invalid token.",
	};

	/// <inheritdoc />
	public override IdentityError InvalidUserName(string? userName) => new()
	{
		Code = nameof(InvalidUserName),
		Description =
			$"User name '{userName}' is invalid, can only contain letters or digits.",
	};

	/// <inheritdoc />
	public override IdentityError LoginAlreadyAssociated() => new()
	{
		Code = nameof(LoginAlreadyAssociated),
		Description = "A user with this login already exists.",
	};

	/// <inheritdoc />
	public override IdentityError PasswordMismatch() => new()
	{
		Code = nameof(PasswordMismatch),
		Description = "Incorrect password.",
	};

	/// <summary>
	/// Returns an error indicating that the password does not meet complexity
	/// requirements.
	/// </summary>
	/// <param name="passwordResult">The password analysis result.</param>
	/// <returns>An <see cref="IdentityError" /> describing the complexity issue.</returns>
	public IdentityError PasswordNotComplexEnough(PasswordResult passwordResult) => new()
	{
		Code = nameof(PasswordNotComplexEnough),
		Description =
			$"Password complexity of {passwordResult.BitsOfEntropy} bits of entropy ({passwordResult.Strength:G}) does not meet goal of {_options.Value.BitsThreshold} bits. Try making it longer, or using multiple character sets like upper-case, lower-case, numbers, or symbols.",
	};

	/// <inheritdoc />
	public override IdentityError PasswordRequiresDigit() => new()
	{
		Code = nameof(PasswordRequiresDigit),
		Description = "Passwords must have at least one digit ('0'-'9').",
	};

	/// <inheritdoc />
	public override IdentityError PasswordRequiresLower() => new()
	{
		Code = nameof(PasswordRequiresLower),
		Description = "Passwords must have at least one lowercase ('a'-'z').",
	};

	/// <inheritdoc />
	public override IdentityError PasswordRequiresNonAlphanumeric() => new()
	{
		Code = nameof(PasswordRequiresNonAlphanumeric),
		Description =
			"Passwords must have at least one non alphanumeric character.",
	};

	/// <inheritdoc />
	public override IdentityError PasswordRequiresUpper() => new()
	{
		Code = nameof(PasswordRequiresUpper),
		Description = "Passwords must have at least one uppercase ('A'-'Z').",
	};

	/// <summary>
	/// Returns an error indicating that the password is too common.
	/// </summary>
	/// <returns>An <see cref="IdentityError" /> for common passwords.</returns>
	public IdentityError PasswordTooCommon() => new()
	{
		Code = nameof(PasswordTooCommon),
		Description = "Password is too common. Please choose a unique password.",
	};

	/// <inheritdoc />
	public override IdentityError PasswordTooShort(int length) => new()
	{
		Code = nameof(PasswordTooShort),
		Description = $"Passwords must be at least {length} characters.",
	};

	/// <inheritdoc />
	public override IdentityError UserAlreadyHasPassword() => new()
	{
		Code = nameof(UserAlreadyHasPassword),
		Description = "User already has a password set.",
	};

	/// <inheritdoc />
	public override IdentityError UserAlreadyInRole(string role) => new()
	{
		Code = nameof(UserAlreadyInRole),
		Description = $"User already in role '{role}'.",
	};

	/// <inheritdoc />
	public override IdentityError UserLockoutNotEnabled() => new()
	{
		Code = nameof(UserLockoutNotEnabled),
		Description = "Lockout is not enabled for this user.",
	};

	/// <inheritdoc />
	public override IdentityError UserNotInRole(string role) => new()
	{
		Code = nameof(UserNotInRole),
		Description = $"User is not in role '{role}'.",
	};
}

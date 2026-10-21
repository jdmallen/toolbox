using Microsoft.IdentityModel.Tokens;

namespace JDMallen.Toolbox.AspNetCore.Options;

/// <summary>
/// The one source of JWT signing credentials, registered as a singleton by
/// <c>AddJwtAuthentication</c>. Bearer token validation and
/// <see cref="Factories.JwtTokenFactory" /> both read it, so tokens are always
/// signed with the key they are validated against.
/// </summary>
/// <remarks>
/// Credentials live here rather than on <see cref="JwtOptions" /> because a
/// <see cref="SecurityKey" /> cannot be bound from configuration.
/// </remarks>
public sealed class JwtSigningCredentials
{
	/// <summary>
	/// Initializes a new instance of the <see cref="JwtSigningCredentials" />
	/// class.
	/// </summary>
	/// <param name="credentials">The credentials to sign tokens with.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="credentials" /> is null.
	/// </exception>
	public JwtSigningCredentials(SigningCredentials credentials)
	{
		ArgumentNullException.ThrowIfNull(credentials);
		Credentials = credentials;
	}

	/// <summary>
	/// Gets the credentials used to sign tokens. Their
	/// <see cref="SigningCredentials.Key" /> also validates incoming tokens.
	/// </summary>
	public SigningCredentials Credentials { get; }
}

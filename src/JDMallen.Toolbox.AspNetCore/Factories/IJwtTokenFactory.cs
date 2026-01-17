using System.Security.Claims;

namespace JDMallen.Toolbox.AspNetCore.Factories;

/// <summary>
/// Factory interface for creating JWT tokens and claims identities.
/// </summary>
public interface IJwtTokenFactory
{
	/// <summary>
	/// Generates a claims identity for the specified user.
	/// </summary>
	/// <param name="email">The user's email address.</param>
	/// <param name="id">The user's ID as a string.</param>
	/// <returns>A <see cref="ClaimsIdentity"/> containing the user's claims.</returns>
	ClaimsIdentity GenerateClaimsIdentity(string email, string id);

	/// <summary>
	/// Generates a claims identity for the specified user.
	/// </summary>
	/// <param name="email">The user's email address.</param>
	/// <param name="id">The user's ID as a GUID.</param>
	/// <returns>A <see cref="ClaimsIdentity"/> containing the user's claims.</returns>
	ClaimsIdentity GenerateClaimsIdentity(string email, Guid id);

	/// <summary>
	/// Generates a JWT token from the specified claims identity.
	/// </summary>
	/// <param name="identity">The claims identity to encode in the token.</param>
	/// <returns>A JWT token string.</returns>
	string GenerateToken(ClaimsIdentity identity);
}

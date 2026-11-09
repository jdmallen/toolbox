using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace JDMallen.Toolbox.AspNetCore.Tests.Infrastructure;

/// <summary>
/// Authentication handler for the in-process test server. A request is treated
/// as authenticated only when it carries the <see cref="USER_HEADER" /> header;
/// its value becomes the <see cref="ClaimTypes.NameIdentifier" /> claim. This
/// lets a single test host produce both authenticated and anonymous requests so
/// the authorization-related endpoint helpers can be exercised end to end.
/// </summary>
internal sealed class TestAuthHandler(
	IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder)
	: AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
	/// <summary>The name of the test authentication scheme.</summary>
	public const string SCHEME_NAME = "Test";

	/// <summary>
	/// Request header whose presence authenticates the caller; its value is
	/// used as the user identifier claim.
	/// </summary>
	public const string USER_HEADER = "X-Test-User";

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.TryGetValue(USER_HEADER, out StringValues userId)
		    || string.IsNullOrWhiteSpace(userId))
		{
			return Task.FromResult(AuthenticateResult.NoResult());
		}

		var identity = new ClaimsIdentity(
			[new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
			SCHEME_NAME);
		var ticket = new AuthenticationTicket(
			new ClaimsPrincipal(identity),
			SCHEME_NAME);

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}

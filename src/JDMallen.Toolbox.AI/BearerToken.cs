namespace JDMallen.Toolbox.AI;

/// <summary>
/// An OAuth bearer token and the moment it stops being valid.
/// </summary>
/// <remarks>
/// Deliberately not named <c>AccessToken</c>, so it never collides with
/// <c>Azure.Core.AccessToken</c> in code that uses both.
/// <see cref="ToString" /> is overridden so a token is never written to a log or
/// exception message by accident.
/// </remarks>
/// <param name="Value">The raw token, sent as <c>Authorization: Bearer</c>.</param>
/// <param name="ExpiresOn">When the token expires.</param>
public readonly record struct BearerToken(string Value, DateTimeOffset ExpiresOn)
{
	/// <summary>Describes the token without revealing its value.</summary>
	/// <returns>The expiry only.</returns>
	public override string ToString() => $"BearerToken (expires {ExpiresOn:O})";
}

/// <summary>
/// Supplies bearer tokens to a chat client. The seam between the clients, which
/// only send the header, and whatever obtains the token: the Azure CLI
/// (<see cref="AzureCliTokenProvider" />), an Azure.Identity credential, or a
/// test double.
/// </summary>
public interface IBearerTokenProvider
{
	/// <summary>Gets a token that is valid now.</summary>
	/// <param name="cancellationToken">Cancels the wait for a token.</param>
	/// <returns>The token and its expiry.</returns>
	ValueTask<BearerToken> GetTokenAsync(CancellationToken cancellationToken = default);
}

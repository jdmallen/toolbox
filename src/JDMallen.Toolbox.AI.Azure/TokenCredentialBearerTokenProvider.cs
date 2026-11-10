using Azure.Core;

namespace JDMallen.Toolbox.AI.Azure;

/// <summary>
/// Adapts any Azure.Identity <see cref="TokenCredential" /> to the toolbox's
/// <see cref="IBearerTokenProvider" />, so
/// <see cref="AzureOpenAIClientOptions.ForEntraId" /> can use it.
/// </summary>
/// <remarks>
/// Each call asks the credential for a token. Wrap the provider in
/// <see cref="CachingBearerTokenProvider" /> to reuse a token until it nears
/// expiry; <see cref="AzureCredentialFactory.CreateTokenProvider" /> does.
/// </remarks>
public sealed class TokenCredentialBearerTokenProvider : IBearerTokenProvider
{
	private readonly TokenCredential _credential;
	private readonly string _scope;
	private readonly string? _tenantId;

	/// <summary>Creates a provider for one scope and tenant.</summary>
	/// <param name="credential">The credential to get tokens from.</param>
	/// <param name="scope">
	/// The token scope; <see cref="AzureOpenAIScopes.DEFAULT" /> unless set.
	/// </param>
	/// <param name="tenantId">
	/// The tenant to request the token from, or null for the credential's own.
	/// </param>
	/// <exception cref="ArgumentException">The scope is blank.</exception>
	public TokenCredentialBearerTokenProvider(
		TokenCredential credential,
		string scope = AzureOpenAIScopes.DEFAULT,
		string? tenantId = null)
	{
		ArgumentNullException.ThrowIfNull(credential);
		ArgumentException.ThrowIfNullOrWhiteSpace(scope);

		_credential = credential;
		_scope = scope.Trim();
		_tenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim();
	}

	/// <inheritdoc />
	/// <exception cref="global::Azure.Identity.AuthenticationFailedException">
	/// The credential could not get a token.
	/// </exception>
	/// <exception cref="global::Azure.Identity.CredentialUnavailableException">
	/// The credential cannot be used here (e.g. its tool is not installed).
	/// </exception>
	public async ValueTask<BearerToken> GetTokenAsync(CancellationToken cancellationToken = default)
	{
		AccessToken token = await _credential
			.GetTokenAsync(RequestContext(), cancellationToken)
			.ConfigureAwait(false);

		return new BearerToken(token.Token, token.ExpiresOn);
	}

	/// <summary>The request for this provider's scope and tenant.</summary>
	internal TokenRequestContext RequestContext() =>
		new([_scope], tenantId: _tenantId);
}

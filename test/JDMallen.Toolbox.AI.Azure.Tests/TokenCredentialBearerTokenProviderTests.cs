using Azure.Core;
using NSubstitute;

namespace JDMallen.Toolbox.AI.Azure.Tests;

public class TokenCredentialBearerTokenProviderTests
{
	private static readonly DateTimeOffset ExpiresOn =
		new(
			2026,
			11,
			9,
			12,
			0,
			0,
			TimeSpan.Zero);

	private readonly TokenCredential _credential =
		Substitute.For<TokenCredential>();

	private TokenRequestContext? _lastContext;
	private CancellationToken _lastToken;

	public TokenCredentialBearerTokenProviderTests()
	{
		// CA2012: this "call" only tells NSubstitute which member to stub;
		// the ValueTask it returns is a placeholder, never awaited.
#pragma warning disable CA2012
		_credential
			.GetTokenAsync(
				Arg.Any<TokenRequestContext>(),
				Arg.Any<CancellationToken>())
			.Returns(call =>
			{
				_lastContext = call.Arg<TokenRequestContext>();
				_lastToken = call.Arg<CancellationToken>();

				return new ValueTask<AccessToken>(
					new AccessToken("entra-token", ExpiresOn));
			});
#pragma warning restore CA2012
	}

	[Fact]
	public async Task GetTokenAsync_MapsTheAccessToken()
	{
		var provider = new TokenCredentialBearerTokenProvider(_credential);

		BearerToken token = await provider.GetTokenAsync(_lastToken);

		Assert.Equal("entra-token", token.Value);
		Assert.Equal(ExpiresOn, token.ExpiresOn);
	}

	[Fact]
	public async Task GetTokenAsync_Defaults_AskForTheDefaultScopeAndNoTenant()
	{
		var provider = new TokenCredentialBearerTokenProvider(_credential);

		await provider.GetTokenAsync(_lastToken);

		Assert.Equal([AzureOpenAIScopes.DEFAULT], _lastContext!.Value.Scopes);
		Assert.Null(_lastContext.Value.TenantId);
	}

	[Fact]
	public async Task GetTokenAsync_PassesScopeAndTenantTrimmed()
	{
		var provider = new TokenCredentialBearerTokenProvider(
			_credential,
			$" {AzureOpenAIScopes.COGNITIVE_SERVICES} ",
			" contoso.onmicrosoft.com ");

		await provider.GetTokenAsync(_lastToken);

		Assert.Equal(
			[AzureOpenAIScopes.COGNITIVE_SERVICES],
			_lastContext!.Value.Scopes);
		Assert.Equal("contoso.onmicrosoft.com", _lastContext.Value.TenantId);
	}

	[Fact]
	public async Task GetTokenAsync_BlankTenant_IsNoTenant()
	{
		var provider = new TokenCredentialBearerTokenProvider(
			_credential,
			tenantId: "  ");

		await provider.GetTokenAsync(_lastToken);

		Assert.Null(_lastContext!.Value.TenantId);
	}

	[Fact]
	public async Task GetTokenAsync_PassesTheCancellationTokenThrough()
	{
		using var cancellation = new CancellationTokenSource();
		var provider = new TokenCredentialBearerTokenProvider(_credential);

		await provider.GetTokenAsync(cancellation.Token);

		Assert.Equal(cancellation.Token, _lastToken);
	}

	[Theory]
	[InlineData("")]
	[InlineData("  ")]
	public void Constructor_BlankScope_Throws(string scope) =>
		Assert.Throws<ArgumentException>(() =>
			new TokenCredentialBearerTokenProvider(_credential, scope));

	[Fact]
	public void Constructor_NullCredential_Throws() =>
		Assert.Throws<ArgumentNullException>(() =>
			new TokenCredentialBearerTokenProvider(null!));
}

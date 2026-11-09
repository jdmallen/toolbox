namespace JDMallen.Toolbox.AI;

/// <summary>
/// Wraps an <see cref="IBearerTokenProvider" /> and reuses its token until
/// shortly before it expires, so a long-lived client does not fetch a token per
/// request (for <see cref="AzureCliTokenProvider" />, that is one <c>az</c>
/// process per request).
/// </summary>
/// <remarks>
/// <para>
/// Concurrent callers share a single in-flight fetch. That fetch is not
/// cancelled when one caller's token is: each caller stops waiting on its own
/// token while the fetch completes for the others. The inner provider is
/// expected to bound its own run time (<see cref="AzureCliTokenProvider" /> has a
/// timeout).
/// </para>
/// <para>A failed fetch is not cached; the next call tries again.</para>
/// </remarks>
public sealed class CachingBearerTokenProvider : IBearerTokenProvider
{
	/// <summary>
	/// How long before expiry a cached token is replaced when no margin is given.
	/// </summary>
	public static readonly TimeSpan DefaultRefreshMargin = TimeSpan.FromMinutes(5);

	private readonly Lock _gate = new();
	private readonly IBearerTokenProvider _inner;
	private readonly TimeSpan _refreshMargin;
	private readonly TimeProvider _timeProvider;
	private BearerToken? _cachedToken;
	private Task<BearerToken>? _pendingFetch;

	/// <summary>Creates a cache in front of <paramref name="inner" />.</summary>
	/// <param name="inner">The provider that actually obtains tokens.</param>
	/// <param name="timeProvider">
	/// The clock used to judge expiry; <see cref="TimeProvider.System" /> when null.
	/// </param>
	/// <param name="refreshMargin">
	/// How long before expiry to fetch a new token;
	/// <see cref="DefaultRefreshMargin" /> when null.
	/// </param>
	public CachingBearerTokenProvider(
		IBearerTokenProvider inner,
		TimeProvider? timeProvider = null,
		TimeSpan? refreshMargin = null)
	{
		ArgumentNullException.ThrowIfNull(inner);
		ArgumentOutOfRangeException.ThrowIfLessThan(
			refreshMargin ?? DefaultRefreshMargin,
			TimeSpan.Zero,
			nameof(refreshMargin));

		_inner = inner;
		_timeProvider = timeProvider ?? TimeProvider.System;
		_refreshMargin = refreshMargin ?? DefaultRefreshMargin;
	}

	/// <inheritdoc />
	public async ValueTask<BearerToken> GetTokenAsync(CancellationToken cancellationToken = default)
	{
		Task<BearerToken> fetch;
		lock (_gate)
		{
			if (_cachedToken is { } cachedToken && IsFresh(cachedToken))
			{
				return cachedToken;
			}

			// A completed fetch is stale here: it either failed or produced the
			// token just judged not fresh. Start a new one.
			if (_pendingFetch is null || _pendingFetch.IsCompleted)
			{
				_pendingFetch = FetchAsync();
			}

			fetch = _pendingFetch;
		}

		return await fetch.WaitAsync(cancellationToken).ConfigureAwait(false);
	}

	private bool IsFresh(BearerToken token) =>
		_timeProvider.GetUtcNow() < token.ExpiresOn - _refreshMargin;

	private async Task<BearerToken> FetchAsync()
	{
		BearerToken token = await _inner
			.GetTokenAsync(CancellationToken.None)
			.ConfigureAwait(false);

		lock (_gate)
		{
			_cachedToken = token;
		}

		return token;
	}
}

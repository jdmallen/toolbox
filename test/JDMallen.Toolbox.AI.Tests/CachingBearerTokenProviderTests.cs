using Microsoft.Extensions.Time.Testing;

// ReSharper disable PropertyCanBeMadeInitOnly.Local

namespace JDMallen.Toolbox.AI.Tests;

public class CachingBearerTokenProviderTests
{
	private static readonly DateTimeOffset Start =
		new(
			2026,
			11,
			9,
			20,
			0,
			0,
			TimeSpan.Zero);

	/// <summary>
	/// Hands out "token-1", "token-2", ... each valid for an hour from the fake
	/// clock's current time, optionally waiting on a gate first.
	/// </summary>
	private sealed class CountingProvider(TimeProvider clock) : IBearerTokenProvider
	{
		private int _calls;

		public int Calls => _calls;

		public TaskCompletionSource? Gate { get; set; }

		public Exception? FailWith { get; set; }

		public async ValueTask<BearerToken> GetTokenAsync(CancellationToken cancellationToken = default)
		{
			int call = Interlocked.Increment(ref _calls);

			if (Gate is { } gate)
			{
				await gate.Task;
			}

			if (FailWith is { } failure)
			{
				throw failure;
			}

			return new BearerToken($"token-{call}", clock.GetUtcNow().AddHours(1));
		}
	}

	[Fact]
	public async Task GetTokenAsync_FreshToken_IsReused()
	{
		var clock = new FakeTimeProvider(Start);
		var inner = new CountingProvider(clock);
		var cache = new CachingBearerTokenProvider(inner, clock);

		BearerToken first = await cache.GetTokenAsync();
		clock.Advance(TimeSpan.FromMinutes(54));
		BearerToken second = await cache.GetTokenAsync();

		Assert.Equal("token-1", first.Value);
		Assert.Equal(first, second);
		Assert.Equal(1, inner.Calls);
	}

	[Fact]
	public async Task GetTokenAsync_WithinRefreshMargin_FetchesNewToken()
	{
		var clock = new FakeTimeProvider(Start);
		var inner = new CountingProvider(clock);
		var cache = new CachingBearerTokenProvider(inner, clock);

		await cache.GetTokenAsync();
		clock.Advance(TimeSpan.FromMinutes(55));
		BearerToken refreshed = await cache.GetTokenAsync();

		Assert.Equal("token-2", refreshed.Value);
		Assert.Equal(2, inner.Calls);
	}

	[Fact]
	public async Task GetTokenAsync_CustomMargin_IsHonoured()
	{
		var clock = new FakeTimeProvider(Start);
		var inner = new CountingProvider(clock);
		var cache = new CachingBearerTokenProvider(inner, clock, TimeSpan.FromMinutes(30));

		await cache.GetTokenAsync();
		clock.Advance(TimeSpan.FromMinutes(31));
		await cache.GetTokenAsync();

		Assert.Equal(2, inner.Calls);
	}

	[Fact]
	public async Task GetTokenAsync_ConcurrentCallers_ShareOneFetch()
	{
		var clock = new FakeTimeProvider(Start);
		var inner = new CountingProvider(clock) { Gate = new TaskCompletionSource() };
		var cache = new CachingBearerTokenProvider(inner, clock);

		Task<BearerToken>[] callers =
			[.. Enumerable.Range(0, 10).Select(_ => cache.GetTokenAsync().AsTask())];
		inner.Gate.SetResult();
		BearerToken[] tokens = await Task.WhenAll(callers);

		Assert.Equal(1, inner.Calls);
		Assert.All(tokens, token => Assert.Equal("token-1", token.Value));
	}

	[Fact]
	public async Task GetTokenAsync_Failure_IsNotCached()
	{
		var clock = new FakeTimeProvider(Start);
		var inner = new CountingProvider(clock)
		{
			FailWith = new AzureCliCredentialException("not signed in"),
		};
		var cache = new CachingBearerTokenProvider(inner, clock);

		await Assert.ThrowsAsync<AzureCliCredentialException>(() => cache.GetTokenAsync().AsTask());
		inner.FailWith = null;
		BearerToken token = await cache.GetTokenAsync();

		Assert.Equal("token-2", token.Value);
	}

	[Fact]
	public async Task GetTokenAsync_OneCallerCancels_OthersStillGetToken()
	{
		var clock = new FakeTimeProvider(Start);
		var inner = new CountingProvider(clock) { Gate = new TaskCompletionSource() };
		var cache = new CachingBearerTokenProvider(inner, clock);
		using var cancellation = new CancellationTokenSource();

		Task<BearerToken> cancelled = cache.GetTokenAsync(cancellation.Token).AsTask();
		// Deliberately not cancellation.Token: this caller must outlive the
		// cancelled one, so it can't share its token.
		Task<BearerToken> patient = cache.GetTokenAsync(CancellationToken.None).AsTask();
		await cancellation.CancelAsync();
		inner.Gate.SetResult();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelled);
		Assert.Equal("token-1", (await patient).Value);
		Assert.Equal(1, inner.Calls);
	}

	[Fact]
	public void Constructor_NegativeMargin_Throws()
	{
		var inner = new CountingProvider(TimeProvider.System);

		Assert.Throws<ArgumentOutOfRangeException>(()
			=> new CachingBearerTokenProvider(inner, refreshMargin: TimeSpan.FromSeconds(-1)));
	}
}

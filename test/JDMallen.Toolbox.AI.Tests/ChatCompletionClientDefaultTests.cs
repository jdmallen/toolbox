namespace JDMallen.Toolbox.AI.Tests;

public class ChatCompletionClientDefaultTests
{
	[Fact]
	public async Task GetCompletionAsync_LegacyImplementation_FallsBackToCompleteAsync()
	{
		IChatCompletionClient client = new LegacyClient();

		CompletionResult result =
			await client.GetCompletionAsync(new CompletionRequest("system", "user", 10));

		Assert.Equal("legacy text", result.Text);
		Assert.Null(result.Usage);
		Assert.Null(result.Model);
	}

	// An implementation written against 4.2.x, before GetCompletionAsync existed.
	private sealed class LegacyClient : IChatCompletionClient
	{
		public Task<string> CompleteAsync(
			CompletionRequest request,
			CancellationToken cancellationToken = default)
			=> Task.FromResult("legacy text");
	}
}

namespace JDMallen.Toolbox.AI.Tests;

/// <summary>
/// Test double for <see cref="HttpClient" />: returns a canned response (or task)
/// and captures the outgoing request for assertions. The request body and headers
/// are read eagerly because the message is disposed by the client after sending.
/// </summary>
internal sealed class StubHttpMessageHandler(
	Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
	: HttpMessageHandler
{
	public Uri? RequestUri { get; private set; }

	public string? RequestBody { get; private set; }

	public Dictionary<string, string> RequestHeaders { get; } = [];

	public StubHttpMessageHandler(HttpResponseMessage response)
		: this(_ => Task.FromResult(response))
	{
	}

	public HttpClient CreateClient() => new(this);

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		RequestUri = request.RequestUri;

		foreach ((string name, IEnumerable<string> values) in request.Headers)
		{
			RequestHeaders[name] = string.Join(",", values);
		}

		RequestBody = request.Content is null
			? null
			: await request.Content.ReadAsStringAsync(cancellationToken);

		return await responder(request);
	}
}

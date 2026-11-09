namespace JDMallen.Toolbox.AI.Tests;

/// <summary>
/// Test double for <see cref="IProcessRunner" />: records the command it was
/// asked to run and returns a canned result or throws a canned exception.
/// </summary>
internal sealed class FakeProcessRunner : IProcessRunner
{
	private readonly Func<ProcessResult> _respond;

	private FakeProcessRunner(Func<ProcessResult> respond)
	{
		_respond = respond;
	}

	public string? FileName { get; private set; }

	public IReadOnlyList<string> Arguments { get; private set; } = [];

	public TimeSpan? Timeout { get; private set; }

	public int Calls { get; private set; }

	public static FakeProcessRunner Returning(
		int exitCode,
		string standardOutput = "",
		string standardError = "")
		=> new(() => new ProcessResult(exitCode, standardOutput, standardError));

	public static FakeProcessRunner Throwing(Exception exception)
		=> new(() => throw exception);

	public Task<ProcessResult> RunAsync(
		string fileName,
		IReadOnlyList<string> arguments,
		TimeSpan timeout,
		CancellationToken cancellationToken)
	{
		Calls++;
		FileName = fileName;
		Arguments = arguments;
		Timeout = timeout;

		return Task.FromResult(_respond());
	}
}

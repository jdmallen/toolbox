using System.ComponentModel;
using System.Diagnostics;

namespace JDMallen.Toolbox.AI;

/// <summary>The exit code and captured output of a finished process.</summary>
internal readonly record struct ProcessResult(
	int ExitCode,
	string StandardOutput,
	string StandardError);

/// <summary>
/// Runs an external process to completion. The seam that lets
/// <see cref="AzureCliTokenProvider" /> be tested without a real <c>az</c>.
/// </summary>
internal interface IProcessRunner
{
	/// <summary>Runs the process and captures its output.</summary>
	/// <exception cref="Win32Exception">The executable could not be started.</exception>
	/// <exception cref="TimeoutException">
	/// The process ran longer than <paramref name="timeout" /> and was killed.
	/// </exception>
	/// <exception cref="OperationCanceledException">
	/// <paramref name="cancellationToken" /> was cancelled; the process was killed.
	/// </exception>
	Task<ProcessResult> RunAsync(
		string fileName,
		IReadOnlyList<string> arguments,
		TimeSpan timeout,
		CancellationToken cancellationToken);
}

/// <summary>
/// <see cref="IProcessRunner" /> over <see cref="Process" />. Arguments go
/// through <see cref="ProcessStartInfo.ArgumentList" />, never a shell string.
/// </summary>
internal sealed class ProcessRunner : IProcessRunner
{
	public static readonly ProcessRunner Instance = new();

	public async Task<ProcessResult> RunAsync(
		string fileName,
		IReadOnlyList<string> arguments,
		TimeSpan timeout,
		CancellationToken cancellationToken)
	{
		var startInfo = new ProcessStartInfo(fileName)
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,

			// The caller may be an interactive app reading the console; the child
			// must never compete with it for keyboard input.
			RedirectStandardInput = true,
		};

		foreach (string argument in arguments)
		{
			startInfo.ArgumentList.Add(argument);
		}

		using var process = new Process();
		process.StartInfo = startInfo;
		process.Start();
		process.StandardInput.Close();

		Task<string> standardOutput =
			process.StandardOutput.ReadToEndAsync(CancellationToken.None);
		Task<string> standardError =
			process.StandardError.ReadToEndAsync(CancellationToken.None);

		using var timeoutSource =
			CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeoutSource.CancelAfter(timeout);

		try
		{
			await process.WaitForExitAsync(timeoutSource.Token).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			KillQuietly(process);
			cancellationToken.ThrowIfCancellationRequested();

			throw new TimeoutException(
				$"'{fileName}' did not exit within {timeout.TotalSeconds:0.#} seconds.");
		}

		return new ProcessResult(
			process.ExitCode,
			await standardOutput.ConfigureAwait(false),
			await standardError.ConfigureAwait(false));
	}

	private static void KillQuietly(Process process)
	{
		try
		{
			process.Kill(true);
		}
		catch (InvalidOperationException)
		{
			// Already exited between the timeout and the kill.
		}
		catch (Win32Exception)
		{
			// Could not be killed (e.g. exiting); nothing more to do.
		}
	}
}

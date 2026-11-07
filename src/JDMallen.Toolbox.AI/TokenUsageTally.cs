namespace JDMallen.Toolbox.AI;

/// <summary>
/// Running token totals for one run of an application: every completion
/// request it made, added up. Record it with
/// <see cref="TokenUsageLog.TryRecord" /> when the run ends.
/// </summary>
public sealed class TokenUsageTally
{
	/// <summary>Requests whose provider reported token usage.</summary>
	public int Requests { get; private set; }

	/// <summary>Sum of <see cref="TokenUsage.InputTokens" />.</summary>
	public long InputTokens { get; private set; }

	/// <summary>Sum of <see cref="TokenUsage.OutputTokens" />.</summary>
	public long OutputTokens { get; private set; }

	/// <summary><see cref="InputTokens" /> plus <see cref="OutputTokens" />.</summary>
	public long TotalTokens => InputTokens + OutputTokens;

	/// <summary>
	/// Whether any request reported usage. False for a run that made no
	/// requests, or only made them to a provider that doesn't report usage.
	/// </summary>
	public bool HasUsage => Requests > 0;

	/// <summary>Adds one request's usage; a null usage is ignored.</summary>
	public void Add(TokenUsage? usage)
	{
		if (usage is null)
		{
			return;
		}

		Requests++;
		InputTokens += usage.InputTokens;
		OutputTokens += usage.OutputTokens;
	}
}

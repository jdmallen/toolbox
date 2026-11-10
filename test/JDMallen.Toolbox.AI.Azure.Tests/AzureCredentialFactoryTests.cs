using System.Text;
using Azure.Identity;

namespace JDMallen.Toolbox.AI.Azure.Tests;

public sealed class AzureCredentialFactoryTests : IDisposable
{
	// The documented AuthenticationRecord JSON: account and tenant ids only.
	private const string RECORD_JSON =
		"""
		{
		  "username": "me@contoso.com",
		  "authority": "login.microsoftonline.com",
		  "homeAccountId": "00000000-0000-0000-0000-000000000001.00000000-0000-0000-0000-000000000002",
		  "tenantId": "00000000-0000-0000-0000-000000000002",
		  "clientId": "04b07795-8ddb-461a-bbee-02f9e1bf7b46",
		  "version": "1.0"
		}
		""";

	private readonly string _directory = Directory.CreateTempSubdirectory(
			"toolbox-ai-azure-tests-")
		.FullName;

	public void Dispose() => Directory.Delete(_directory, true);

	private string WriteRecordFile(string contents = RECORD_JSON)
	{
		string path = Path.Combine(_directory, "auth-record.json");
		File.WriteAllText(path, contents);

		return path;
	}

	[Theory]
	[InlineData(AzureCredentialKind.Default, typeof(DefaultAzureCredential))]
	[InlineData(AzureCredentialKind.AzureCli, typeof(AzureCliCredential))]
	[InlineData(
		AzureCredentialKind.AzureDeveloperCli,
		typeof(AzureDeveloperCliCredential))]
	[InlineData(
		AzureCredentialKind.InteractiveBrowser,
		typeof(InteractiveBrowserCredential))]
	[InlineData(AzureCredentialKind.DeviceCode, typeof(DeviceCodeCredential))]
	public void Create_BuildsTheCredentialForEachKind(
		AzureCredentialKind kind,
		Type expected) => Assert.IsType(
		expected,
		AzureCredentialFactory.Create(new AzureCredentialOptions { Kind = kind }));

	[Fact]
	public void Create_UnknownKind_Throws() =>
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			AzureCredentialFactory.Create(
				new AzureCredentialOptions { Kind = (AzureCredentialKind)99 }));

	[Theory]
	[InlineData(AzureCredentialKind.InteractiveBrowser, true)]
	[InlineData(AzureCredentialKind.DeviceCode, true)]
	[InlineData(AzureCredentialKind.Default, false)]
	[InlineData(AzureCredentialKind.AzureCli, false)]
	[InlineData(AzureCredentialKind.AzureDeveloperCli, false)]
	public void IsInteractive_OnlyBrowserAndDeviceCode(
		AzureCredentialKind kind,
		bool expected) =>
		Assert.Equal(expected, AzureCredentialFactory.IsInteractive(kind));

	[Fact]
	public void InteractiveBrowserOptions_UseANamedEncryptedCache()
	{
		InteractiveBrowserCredentialOptions options =
			AzureCredentialFactory.InteractiveBrowserOptions(
				new AzureCredentialOptions
				{
					Kind = AzureCredentialKind.InteractiveBrowser,
					CacheName = " committy ",
					TenantId = " contoso.com ",
				});

		Assert.Equal("committy", options.TokenCachePersistenceOptions!.Name);
		Assert.False(
			options.TokenCachePersistenceOptions.UnsafeAllowUnencryptedStorage);
		Assert.Equal("contoso.com", options.TenantId);
		Assert.False(options.DisableAutomaticAuthentication);
		Assert.Null(options.AuthenticationRecord);
	}

	[Fact]
	public void InteractiveBrowserOptions_BlankCacheName_UsesTheDefault()
		=> Assert.Equal(
			AzureCredentialOptions.DEFAULT_CACHE_NAME,
			AzureCredentialFactory.InteractiveBrowserOptions(
					new AzureCredentialOptions { CacheName = " " })
				.TokenCachePersistenceOptions!.Name);

	[Fact]
	public void DeviceCodeOptions_NoPrompting_DisablesAutomaticAuthentication()
	{
		DeviceCodeCredentialOptions options =
			AzureCredentialFactory.DeviceCodeOptions(
				new AzureCredentialOptions
				{
					Kind = AzureCredentialKind.DeviceCode,
					AllowInteractiveAuthentication = false,
				});

		Assert.True(options.DisableAutomaticAuthentication);
		Assert.False(
			options.TokenCachePersistenceOptions!.UnsafeAllowUnencryptedStorage);
	}

	[Fact]
	public void InteractiveOptions_LoadTheSavedRecord()
	{
		string path = WriteRecordFile();

		AuthenticationRecord? record =
			AzureCredentialFactory.InteractiveBrowserOptions(
					new AzureCredentialOptions { AuthenticationRecordPath = path })
				.AuthenticationRecord;

		Assert.NotNull(record);
		Assert.Equal("me@contoso.com", record.Username);
		Assert.Equal("00000000-0000-0000-0000-000000000002", record.TenantId);
	}

	[Theory]
	[InlineData("not json")]
	[InlineData("")]
	public void LoadRecord_Unreadable_IsNull(string contents) =>
		Assert.Null(
			AzureCredentialFactory.LoadRecord(
				new AzureCredentialOptions
				{
					AuthenticationRecordPath = WriteRecordFile(contents),
				}));

	[Fact]
	public void LoadRecord_MissingOrUnset_IsNull()
	{
		Assert.Null(
			AzureCredentialFactory.LoadRecord(
				new AzureCredentialOptions
				{
					AuthenticationRecordPath = Path.Combine(_directory, "missing.json"),
				}));
		Assert.Null(
			AzureCredentialFactory.LoadRecord(new AzureCredentialOptions()));
	}

	[Fact]
	public async Task SaveRecordAsync_RoundTripsAndCreatesTheDirectory()
	{
		AuthenticationRecord record = AzureCredentialFactory.LoadRecord(
			new AzureCredentialOptions
			{
				AuthenticationRecordPath = WriteRecordFile(),
			})!;
		string path = Path.Combine(
			_directory,
			"nested",
			"committy",
			"record.json");
		var options = new AzureCredentialOptions { AuthenticationRecordPath = path };

		await AzureCredentialFactory.SaveRecordAsync(
			record,
			options,
			CancellationToken.None);

		AuthenticationRecord? reloaded = AzureCredentialFactory.LoadRecord(options);
		Assert.NotNull(reloaded);
		Assert.Equal(record.Username, reloaded.Username);
		Assert.Equal(record.HomeAccountId, reloaded.HomeAccountId);
		Assert.False(File.Exists($"{path}.tmp"));
		Assert.DoesNotContain(
			"token",
			(await File.ReadAllTextAsync(path, Encoding.UTF8)).Replace(
				"\"tenantId\"",
				string.Empty,
				StringComparison.Ordinal),
			StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void WithKeyringHint_KeepsTheMessageAndPointsAtTheAzureCli()
	{
		var original = new CredentialUnavailableException("No keyring.");

		CredentialUnavailableException wrapped =
			AzureCredentialFactory.WithKeyringHint(original);

		Assert.StartsWith("No keyring. ", wrapped.Message);
		Assert.Contains("Azure CLI", wrapped.Message);
		Assert.Same(original, wrapped.InnerException);
	}

	[Fact]
	public void NeedsKeyringHint_NotForAuthenticationRequired()
	{
		// Prompting disabled with no saved sign-in: not a keyring problem, and
		// callers (git hooks) must still be able to catch this type.
		var required = new AuthenticationRequiredException(
			"Interactive authentication is needed.",
			new global::Azure.Core.TokenRequestContext([AzureOpenAIScopes.DEFAULT]));

		Assert.False(AzureCredentialFactory.NeedsKeyringHint(required));
	}

	[Fact]
	public void NeedsKeyringHint_OnlyOnce()
	{
		var original = new CredentialUnavailableException("No keyring.");

		Assert.True(AzureCredentialFactory.NeedsKeyringHint(original));
		Assert.False(
			AzureCredentialFactory.NeedsKeyringHint(
				AzureCredentialFactory.WithKeyringHint(original)));
	}

	[Theory]
	[InlineData(AzureCredentialKind.AzureCli)]
	[InlineData(AzureCredentialKind.InteractiveBrowser)]
	public void CreateTokenProvider_IsCached(AzureCredentialKind kind) =>
		Assert.IsType<CachingBearerTokenProvider>(
			AzureCredentialFactory.CreateTokenProvider(
				new AzureCredentialOptions { Kind = kind }));
}

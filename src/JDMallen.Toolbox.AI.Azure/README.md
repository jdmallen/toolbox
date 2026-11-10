# JDMallen.Toolbox.AI.Azure

Microsoft Entra ID sign-in for the Azure OpenAI client in
[JDMallen.Toolbox.AI](https://www.nuget.org/packages/JDMallen.Toolbox.AI),
through [Azure.Identity](https://www.nuget.org/packages/Azure.Identity):

- `TokenCredentialBearerTokenProvider` adapts any Azure.Identity
  `TokenCredential` to the toolbox's `IBearerTokenProvider`.
- `AzureCredentialFactory` builds a credential from a few settings, with a
  persistent token cache for the interactive ones, so a CLI that runs once per
  command (such as a git hook) signs in once rather than every time.

Only need the Azure CLI's sign-in? `AzureCliTokenProvider` in
JDMallen.Toolbox.AI does that with no Azure SDK dependency, and stays trim- and
AOT-safe. Use this package for browser or device code sign-in, managed
identity, or `DefaultAzureCredential`.

## Installation

```shell
dotnet add package JDMallen.Toolbox.AI.Azure
```

## Quick start

```csharp
IBearerTokenProvider tokens = AzureCredentialFactory.CreateTokenProvider(
	new AzureCredentialOptions
	{
		Kind = AzureCredentialKind.InteractiveBrowser,
		TenantId = "contoso.onmicrosoft.com", // optional
		CacheName = "myapp",
		AuthenticationRecordPath = Path.Combine(configDirectory, "auth-record.json"),
	});

var client = new AzureOpenAIChatClient(
	httpClient,
	AzureOpenAIClientOptions.ForEntraId(
		"https://myresource.openai.azure.com",
		"gpt-6-luna",
		tokens));
```

`CreateTokenProvider` wraps the credential in `CachingBearerTokenProvider`, so a
token is reused until shortly before it expires. The scope defaults to
`AzureOpenAIScopes.DEFAULT` (`https://ai.azure.com/.default`); pass another as
the second argument.

## Credential kinds

| `AzureCredentialKind` | Credential | Notes |
|---|---|---|
| `Default` | `DefaultAzureCredential` | Environment, workload and managed identity, then developer tools. Narrow it with the `AZURE_TOKEN_CREDENTIALS` environment variable: `dev`, `prod`, or one credential name such as `AzureCliCredential`. |
| `AzureCli` | `AzureCliCredential` | Whoever ran `az login`. |
| `AzureDeveloperCli` | `AzureDeveloperCliCredential` | Whoever ran `azd auth login`. |
| `InteractiveBrowser` | `InteractiveBrowserCredential` | Opens a browser; remembered in the persistent cache. |
| `DeviceCode` | `DeviceCodeCredential` | Prints a code to enter at a sign-in page on any device; remembered like the browser kind. |

## Signing in once (interactive kinds)

The interactive kinds keep tokens in a persistent cache named by `CacheName`,
and the sign-in's `AuthenticationRecord` (account and tenant ids, no tokens) in
`AuthenticationRecordPath`. A later process loads the record and gets its token
from the cache silently.

- **Without a saved record**, the first token request from
  `CreateTokenProvider` signs in and saves one.
- **To sign in ahead of time**, for example from a setup command, call
  `SignInAsync`. It always allows a prompt, saves the record, and returns the
  account:

  ```csharp
  AzureSignInResult signedIn = await AzureCredentialFactory.SignInAsync(options);
  Console.WriteLine($"Signed in as {signedIn.Username}");
  ```

- **Where a prompt would be wrong**, such as inside a git hook, set
  `AllowInteractiveAuthentication = false`. With no usable cached sign-in, the
  token request then throws `AuthenticationRequiredException` instead of
  opening a browser. Catch it and tell the user to run your setup command.

**Set `TenantId` for the interactive kinds** when the account is a personal
Microsoft account (outlook.com, or a gmail address added to a tenant), or
belongs to more than one tenant. Without it, sign-in goes through the generic
"organizations" endpoint and can fail with "Selected user account does not
exist in tenant 'Microsoft Services'". `az account show --query tenantId`
prints the right value.

The cache is encrypted by the operating system: the keyring (libsecret) on
Linux, the Keychain on macOS, DPAPI on Windows. It is never written
unencrypted. Without a keyring (for example over SSH, or in a container), the
interactive kinds throw `CredentialUnavailableException`, with a message
suggesting the Azure CLI instead.

## Azure setup

1. The signed-in identity needs a data-plane role on the resource, such as
   "Cognitive Services OpenAI User". Owner and Contributor are not enough. New
   assignments take up to 5 minutes to apply.
2. The endpoint must be the resource's own custom subdomain (not a regional
   endpoint).

## Target Framework

- `net10.0`. Not marked trimmable or AOT-compatible, because Azure.Identity
  (and MSAL beneath it) make no such claim.

## Dependencies

- `JDMallen.Toolbox.AI`
- `Azure.Identity`

## License

MIT. See the repository root for the full license text.

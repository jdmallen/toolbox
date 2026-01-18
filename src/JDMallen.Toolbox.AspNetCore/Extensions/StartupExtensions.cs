using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using JDMallen.Toolbox.AspNetCore.Constants;
using JDMallen.Toolbox.AspNetCore.Options;
using JDMallen.Toolbox.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JDMallen.Toolbox.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring ASP.NET Core startup and authentication.
/// </summary>
public static class StartupExtensions
{
	/// <summary>
	/// Configures Kestrel server endpoints based on configuration settings.
	/// Supports HTTPS endpoint configuration with certificate loading from store or file.
	/// See https://blogs.msdn.microsoft.com/webdev/2017/11/29/configuring-https-in-asp-net-core-across-different-platforms/
	/// </summary>
	/// <param name="options">The Kestrel server options to configure.</param>
	public static void ConfigureEndpoints(this KestrelServerOptions options)
	{
		var configuration =
			options.ApplicationServices.GetRequiredService<IConfiguration>();
		var environment =
			options.ApplicationServices.GetRequiredService<IHostEnvironment>();
		var endpoints = configuration.GetSection("Endpoints")
			.GetChildren()
			.ToDictionary(
				section => section.Key,
				section =>
				{
					var endpoint = new EndpointConfiguration();
					section.Bind(endpoint);
					return endpoint;
				});
		foreach (var endpoint in endpoints)
		{
			var config = endpoint.Value;
			bool.TryParse(
				configuration["Settings:ReverseProxy"],
				out var reverseProxy);
			if (config.Scheme == "https" && reverseProxy)
			{
				continue;
			}

			var port = config.Port ?? (config.Scheme == "https" ? 443 : 80);
			var ipAddresses = new List<IPAddress>();
			if (config.Host == "localhost")
			{
				ipAddresses.Add(IPAddress.IPv6Loopback);
				ipAddresses.Add(IPAddress.Loopback);
			}
			else if (IPAddress.TryParse(config.Host, out var address))
			{
				ipAddresses.Add(address);
			}
			else
			{
				ipAddresses.Add(IPAddress.IPv6Any);
			}

			foreach (var address in ipAddresses)
			{
				Debug.WriteLine(
					$"Adding option:{Environment.NewLine}"
					+ $"  Address: {address}{Environment.NewLine}"
					+ $"  Port: {port}{Environment.NewLine}"
					+ $"  Scheme: {config.Scheme}");
				options.Listen(
					address,
					port,
					listenOptions =>
					{
						if (config.Scheme != "https" || reverseProxy)
						{
							return;
						}

						var certificate = LoadCertificate(config, environment);
						Debug.WriteLine($"  Certificate: {certificate.FriendlyName}");
						listenOptions.UseHttps(certificate);
					});
			}
		}
	}

	/// <summary>
	/// Loads an X.509 certificate based on the provided endpoint configuration.
	/// Attempts to load from certificate store (if store and location are specified) or from file path.
	/// See https://blogs.msdn.microsoft.com/webdev/2017/11/29/configuring-https-in-asp-net-core-across-different-platforms/
	/// </summary>
	/// <param name="config">The endpoint configuration containing certificate details.</param>
	/// <param name="environment">The hosting environment.</param>
	/// <returns>The loaded X.509 certificate.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the certificate cannot be found or no valid configuration is provided.</exception>
	private static X509Certificate2 LoadCertificate(
		EndpointConfiguration config,
		IHostEnvironment environment)
	{
		switch (config)
		{
			case { StoreName: not null, StoreLocation: not null, Thumbprint: not null }:
			{
				var parseSuccess = Enum.TryParse<StoreLocation>(
					config.StoreLocation,
					out var storeLocation);
				var store = parseSuccess
					? new X509Store(config.StoreName, storeLocation)
					: new X509Store(config.StoreName);
				using (store)
				{
					store.Open(OpenFlags.ReadOnly);
					var certificate = store.Certificates.Find(
						X509FindType.FindByThumbprint,
						config.Thumbprint,
						!environment.IsDevelopment());
					if (certificate.Count == 0)
					{
						throw new InvalidOperationException(
							$"Certificate not found for {config.Host}.");
					}

					return certificate[0];
				}
			}
			case { FilePath: not null, CertificatePassword: not null }:
				return X509CertificateLoader.LoadPkcs12FromFile(
					config.FilePath,
					config.CertificatePassword);
			default:
				throw new InvalidOperationException(
					"No valid certificate configuration found for the current endpoint.");
		}
	}

	/// <summary>
	/// Adds GitHub OAuth authentication to the application.
	/// </summary>
	/// <param name="builder">The authentication builder.</param>
	/// <param name="config">
	/// The OAuth configuration containing GitHub client
	/// credentials.
	/// </param>
	/// <returns>The authentication builder for method chaining.</returns>
	public static AuthenticationBuilder AddOAuthGitHub(
		this AuthenticationBuilder builder,
		OAuthConfiguration config)
	{
		if (string.IsNullOrWhiteSpace(config.GitHubClientId) || string.IsNullOrWhiteSpace(config.GitHubClientSecret))
		{
			throw new InvalidOperationException(
				"GitHub OAuth client ID and secret must be provided in configuration.");
		}
		
		return builder.AddOAuth(
			OAuthSchemes.GitHub,
			options =>
			{
				options.ClientId = config.GitHubClientId;
				options.ClientSecret = config.GitHubClientSecret;
				options.CallbackPath = new PathString("/signin-github");
				options.AuthorizationEndpoint =
					"https://github.com/login/oauth/authorize";
				options.TokenEndpoint = "https://github.com/login/oauth/access_token";
				options.UserInformationEndpoint = "https://api.github.com/user";
				config.GitHubScopes.ToList().ForEach(s => options.Scope.Add(s));
				options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
				options.ClaimActions.MapJsonKey(ClaimTypes.Name, "email");
				options.ClaimActions.MapJsonKey("urn:github:login", "login");
				options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
				options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");
				options.ClaimActions.MapJsonKey("urn:github:displayName", "name");
				options.Events = new OAuthEvents
				{
					OnCreatingTicket = async context =>
					{
						var request = new HttpRequestMessage(
							HttpMethod.Get,
							context.Options.UserInformationEndpoint);
						request.Headers.Accept.Add(
							new MediaTypeWithQualityHeaderValue("application/json"));
						request.Headers.Authorization = new AuthenticationHeaderValue(
							"Bearer",
							context.AccessToken);
						var response = await context.Backchannel.SendAsync(
							request,
							HttpCompletionOption.ResponseHeadersRead,
							context.HttpContext.RequestAborted);
						response.EnsureSuccessStatusCode();
						var responseStr = await response.Content.ReadAsStringAsync();
						var user = JsonDocument.Parse(responseStr);
						Debug.WriteLine(user);
						context.RunClaimActions(user.RootElement);
					}
				};
			});
	}

	/// <summary>
	/// Adds Google OAuth authentication to the application.
	/// </summary>
	/// <param name="builder">The authentication builder.</param>
	/// <param name="config">
	/// The OAuth configuration containing Google client
	/// credentials.
	/// </param>
	/// <returns>The authentication builder for method chaining.</returns>
	public static AuthenticationBuilder AddOAuthGoogle(
		this AuthenticationBuilder builder,
		OAuthConfiguration config)
	{
		if(string.IsNullOrWhiteSpace(config.GoogleClientId) || string.IsNullOrWhiteSpace(config.GoogleClientSecret))
		{
			throw new InvalidOperationException(
				"Google OAuth client ID and secret must be provided in configuration.");
		}
		
		return builder.AddGoogle(
			OAuthSchemes.Google,
			options =>
			{
				options.ClientId = config.GoogleClientId;
				options.ClientSecret = config.GoogleClientSecret;
				options.TokenEndpoint = "https://accounts.google.com/o/oauth2/token";
				config.GoogleScopes.ToList().ForEach(s => options.Scope.Add(s));
				options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
				options.ClaimActions.MapJsonKey(ClaimTypes.Name, "emails[0].value");
				options.ClaimActions.MapJsonKey("urn:google:login", "login");
				options.ClaimActions.MapJsonKey("urn:google:url", "url");
				options.ClaimActions.MapJsonKey("urn:google:avatar", "image.url");
				options.ClaimActions.MapJsonKey(
					"urn:google:displayName",
					"displayName");
				options.Events = new OAuthEvents
				{
					OnCreatingTicket = async context =>
					{
						var request = new HttpRequestMessage(
							HttpMethod.Get,
							context.Options.UserInformationEndpoint);
						request.Headers.Accept.Add(
							new MediaTypeWithQualityHeaderValue("application/json"));
						request.Headers.Authorization = new AuthenticationHeaderValue(
							"Bearer",
							context.AccessToken);
						var response = await context.Backchannel.SendAsync(
							request,
							HttpCompletionOption.ResponseHeadersRead,
							context.HttpContext.RequestAborted);
						response.EnsureSuccessStatusCode();
						var responseStr = await response.Content.ReadAsStringAsync();
						var user = JsonDocument.Parse(responseStr);
						Debug.WriteLine(user);
						context.RunClaimActions(user.RootElement);
					}
				};
			});
	}
}

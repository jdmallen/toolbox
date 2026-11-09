using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using JDMallen.Toolbox.AspNetCore.Constants;
using JDMallen.Toolbox.AspNetCore.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JDMallen.Toolbox.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring ASP.NET Core startup and authentication.
/// </summary>
public static class StartupExtensions
{
	/// <param name="builder">The authentication builder.</param>
	extension(AuthenticationBuilder builder)
	{
		/// <summary>
		/// Adds GitHub OAuth authentication to the application.
		/// </summary>
		/// <param name="config">
		/// The OAuth configuration containing GitHub client
		/// credentials.
		/// </param>
		/// <returns>The authentication builder for method chaining.</returns>
		public AuthenticationBuilder AddOAuthGitHub(OAuthConfiguration config)
		{
			if (string.IsNullOrWhiteSpace(config.GitHubClientId)
			    || string.IsNullOrWhiteSpace(config.GitHubClientSecret))
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
							HttpResponseMessage response = await context.Backchannel.SendAsync(
								request,
								HttpCompletionOption.ResponseHeadersRead,
								context.HttpContext.RequestAborted);
							response.EnsureSuccessStatusCode();
							string responseStr = await response.Content.ReadAsStringAsync();
							JsonDocument user = JsonDocument.Parse(responseStr);
							Debug.WriteLine(user);
							context.RunClaimActions(user.RootElement);
						},
					};
				});
		}

		/// <summary>
		/// Adds Google OAuth authentication to the application.
		/// </summary>
		/// <param name="config">
		/// The OAuth configuration containing Google client
		/// credentials.
		/// </param>
		/// <returns>The authentication builder for method chaining.</returns>
		public AuthenticationBuilder AddOAuthGoogle(OAuthConfiguration config)
		{
			if (string.IsNullOrWhiteSpace(config.GoogleClientId)
			    || string.IsNullOrWhiteSpace(config.GoogleClientSecret))
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
							HttpResponseMessage response = await context.Backchannel.SendAsync(
								request,
								HttpCompletionOption.ResponseHeadersRead,
								context.HttpContext.RequestAborted);
							response.EnsureSuccessStatusCode();
							string responseStr = await response.Content.ReadAsStringAsync();
							JsonDocument user = JsonDocument.Parse(responseStr);
							Debug.WriteLine(user);
							context.RunClaimActions(user.RootElement);
						},
					};
				});
		}
	}
}

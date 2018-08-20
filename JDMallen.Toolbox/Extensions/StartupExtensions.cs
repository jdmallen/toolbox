using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using JDMallen.Toolbox.Constants;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace JDMallen.Toolbox.Extensions
{
	public static class StartupExtensions
	{
		/// <summary>
		/// https://blogs.msdn.microsoft.com/webdev/2017/11/29/configuring-https-in-asp-net-core-across-different-platforms/
		/// </summary>
		/// <param name="options"></param>
		public static void ConfigureEndpoints(this KestrelServerOptions options)
		{
			var configuration = options.ApplicationServices.GetRequiredService<IConfiguration>();
			var environment = options.ApplicationServices.GetRequiredService<IHostingEnvironment>();

			var endpoints = configuration.GetSection("Endpoints")
										.GetChildren()
										.ToDictionary(section => section.Key,
													section =>
													{
														var endpoint = new EndpointConfiguration();
														section.Bind(endpoint);
														return endpoint;
													});

			foreach (var endpoint in endpoints)
			{
				var config = endpoint.Value;
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
					Debug.WriteLine($"Adding option:{Environment.NewLine}"
									+ $"  Address: {address}{Environment.NewLine}"
									+ $"  Port: {port}{Environment.NewLine}"
									+ $"  Scheme: {config.Scheme}");
					options.Listen(address,
									port,
									listenOptions =>
									{
										if (config.Scheme != "https") return;
										var certificate = LoadCertificate(config, environment);
										Debug.WriteLine($"  Certificate: {certificate.FriendlyName}");
										listenOptions.UseHttps(certificate);
									});
				}
			}
		}

		/// <summary>
		/// https://blogs.msdn.microsoft.com/webdev/2017/11/29/configuring-https-in-asp-net-core-across-different-platforms/
		/// </summary>
		/// <param name="config"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		private static X509Certificate2 LoadCertificate(
			EndpointConfiguration config,
			IHostingEnvironment environment)
		{
			if (config.StoreName != null && config.StoreLocation != null)
			{
				var parseSuccess = Enum.TryParse<StoreLocation>(config.StoreLocation, out var storeLocation);
				var store = parseSuccess
								? new X509Store(config.StoreName, storeLocation)
								: new X509Store(config.StoreName);
				using (store)
				{
					store.Open(OpenFlags.ReadOnly);
					var certificate = store.Certificates.Find(
						X509FindType.FindByThumbprint,
						config.Thumbprint,
						validOnly: !environment.IsDevelopment());

					if (certificate.Count == 0)
					{
						throw new InvalidOperationException($"Certificate not found for {config.Host}.");
					}

					return certificate[0];
				}
			}

			if (config.FilePath != null && config.CertificatePassword != null)
			{
				return new X509Certificate2(config.FilePath, config.CertificatePassword);
			}

			throw new InvalidOperationException(
				"No valid certificate configuration found for the current endpoint.");
		}

		public static AuthenticationBuilder AddOAuthGitHub(
			this AuthenticationBuilder builder,
			OAuthConfiguration config)
		{
			return builder.AddOAuth(OAuthSchemes.GitHub, options =>
			{
				options.ClientId = config.GitHubClientId;
				options.ClientSecret = config.GitHubClientSecret;
				options.CallbackPath = new PathString("/signin-github");
				options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
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
						var request =
							new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
						request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						request.Headers.Authorization =
							new AuthenticationHeaderValue("Bearer", context.AccessToken);

						var response = await context.Backchannel.SendAsync(request,
																			HttpCompletionOption.ResponseHeadersRead,
																			context.HttpContext.RequestAborted);
						response.EnsureSuccessStatusCode();

						var user = JObject.Parse(await response.Content.ReadAsStringAsync());
						Debug.WriteLine(user);
						context.RunClaimActions(user);
					}
				};
			});
		}

		public static AuthenticationBuilder AddOAuthGoogle(
			this AuthenticationBuilder builder,
			OAuthConfiguration config)
		{
			return builder.AddGoogle(OAuthSchemes.Google, options =>
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
				options.ClaimActions.MapJsonKey("urn:google:displayName", "displayName");

				options.Events = new OAuthEvents
				{
					OnCreatingTicket = async context =>
					{
						var request =
							new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
						request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						request.Headers.Authorization =
							new AuthenticationHeaderValue("Bearer", context.AccessToken);

						var response = await context.Backchannel.SendAsync(request,
																			HttpCompletionOption.ResponseHeadersRead,
																			context.HttpContext.RequestAborted);
						response.EnsureSuccessStatusCode();

						var user = JObject.Parse(await response.Content.ReadAsStringAsync());
						Debug.WriteLine(user);
						context.RunClaimActions(user);
					}
				};
			});
		}
	}
}

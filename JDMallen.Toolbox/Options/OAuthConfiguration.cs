using System.Collections.Generic;

namespace JDMallen.Toolbox.Options
{
	public class OAuthConfiguration
	{
		public string FacebookClientId { get; set; }

		public string FacebookClientSecret { get; set; }

		public string GitHubClientId { get; set; }

		public string GitHubClientSecret { get; set; }

		public IEnumerable<string> GitHubScopes { get; set; }

		public string GoogleClientId { get; set; }

		public string GoogleClientSecret { get; set; }

		public IEnumerable<string> GoogleScopes { get; set; }

		public string TwitterClientId { get; set; }

		public string TwitterClientSecret { get; set; }
	}
}
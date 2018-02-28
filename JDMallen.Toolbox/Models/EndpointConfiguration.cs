namespace JDMallen.Toolbox.Models
{
	/// <summary>
	/// https://blogs.msdn.microsoft.com/webdev/2017/11/29/configuring-https-in-asp-net-core-across-different-platforms/
	/// </summary>
    public class EndpointConfiguration
    {
		public string Host { get; set; }

		public int? Port { get; set; }

		public string Scheme { get; set; }

		public string StoreName { get; set; }

		public string StoreLocation { get; set; }

		public string FilePath { get; set; }

		public string CertificatePassword { get; set; }
    }
}

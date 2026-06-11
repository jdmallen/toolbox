namespace JDMallen.Toolbox.Models;

/// <summary>
/// https://blogs.msdn.microsoft.com/webdev/2017/11/29/configuring-https-in-asp-net-core-across-different-platforms/
/// </summary>
public class EndpointConfiguration
{
	/// <summary>
	/// The password for the PKCS#12 certificate.
	/// </summary>
	public string? CertificatePassword { get; set; }

	/// <summary>
	/// The file path to the PKCS#12 certificate to use.
	/// </summary>
	public string? FilePath { get; set; }

	/// <summary>
	/// The host name to bind to (e.g. "localhost"). Use "*" to bind to all hosts.
	/// </summary>
	public string? Host { get; set; }

	/// <summary>
	/// The port number to bind to (e.g. 5001).
	/// </summary>
	public int? Port { get; set; }

	/// <summary>
	/// The scheme to use (e.g. "https").
	/// </summary>
	public string? Scheme { get; set; }

	/// <summary>
	/// The location of the certificate store to use (e.g. "CurrentUser").
	/// </summary>
	public string? StoreLocation { get; set; }

	/// <summary>
	/// The name of the certificate store to use (e.g. "My").
	/// </summary>
	public string? StoreName { get; set; }

	/// <summary>
	/// The thumbprint of the certificate to use.
	/// </summary>
	public string? Thumbprint { get; set; }
}

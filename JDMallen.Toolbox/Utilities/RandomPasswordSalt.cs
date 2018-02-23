using System;
using System.Security.Cryptography;

namespace JDMallen.Toolbox.Utilities
{
	/// <summary>
	/// https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing
	/// </summary>
	public static class RandomPasswordSalt
    {
		public static string Generate()
		{
			using (var numberGenerator = RandomNumberGenerator.Create())
			{
				var bytes = new byte[128 / 8];
				numberGenerator.GetBytes(bytes);
				return Convert.ToBase64String(bytes);
			}
		}
    }
}

using JDMallen.Toolbox.AspNetCore.Constants;
using JDMallen.Toolbox.AspNetCore.Models;
using JDMallen.Toolbox.AspNetCore.Options;
using JDMallen.Toolbox.AspNetCore.Utilities;
using JDMallen.Toolbox.Resources;
using Microsoft.AspNetCore.Identity;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Tests for <see cref="CustomPasswordValidator{TUser}.CheckPassword" /> and the
/// embedded resources behind it. Before 3.2.2, <see cref="ResourceLoader" />'s
/// iterators never ended, so any password of two or more characters hung.
/// </summary>
public class CustomPasswordValidatorTests
{
	private static PasswordResult Check(string password) =>
		new CustomPasswordValidator<IdentityUser<Guid>>().CheckPassword(password);

	[Fact]
	public void Resources_HaveTheExpectedEntryCounts()
	{
		Assert.Equal(7186, ResourceLoader.CommonPasswords.Count());
		Assert.Equal(27 * 27, ResourceLoader.FrequencyTable.Count());
	}

	[Fact(Timeout = 10_000)]
	public async Task CheckPassword_ReturnsForAMultiCharacterPassword()
	{
		// Run off the test thread so the timeout can fail the test instead of
		// hanging the whole run if the regression comes back.
		PasswordResult result = await Task.Run(() => Check("Tr0ub4dor&3"));

		Assert.Equal(11, result.Length);
	}

	[Theory]
	[InlineData("password", 28.693077, PasswordStrength.Poor,
		PasswordError.TooCommon | PasswordError.NotComplexEnough)]
	[InlineData("PassWord", 34.797417, PasswordStrength.Fair,
		PasswordError.TooCommon | PasswordError.NotComplexEnough)]
	[InlineData("abcdefgh", 29.891434, PasswordStrength.Poor,
		PasswordError.NotComplexEnough)]
	[InlineData("qwertyuiop", 37.33481, PasswordStrength.Fair,
		PasswordError.NotComplexEnough)]
	[InlineData("short1!", 26.549723, PasswordStrength.Poor,
		PasswordError.TooShort | PasswordError.NotComplexEnough)]
	[InlineData("ab", 4.4744873, PasswordStrength.Poor,
		PasswordError.TooShort | PasswordError.NotComplexEnough)]
	[InlineData("Tr0ub4dor&3", 51.7914, PasswordStrength.Fair,
		PasswordError.None)]
	[InlineData("zZ9!kq#Lm2@wXp", 69.02942, PasswordStrength.Good,
		PasswordError.None)]
	[InlineData("café au lait 2026", 94.79811, PasswordStrength.Great,
		PasswordError.None)]
	[InlineData("correct horse battery staple", 104.19175,
		PasswordStrength.Great, PasswordError.None)]
	public void CheckPassword_PinsKnownResults(
		string password,
		double bits,
		PasswordStrength strength,
		PasswordError error)
	{
		PasswordResult result = Check(password);

		Assert.Equal(bits, result.BitsOfEntropy, 3);
		Assert.Equal(strength, result.Strength);
		Assert.Equal(error, result.Error);
		Assert.Equal(error != PasswordError.None, result.IsError);
		Assert.Equal(password.Length, result.Length);
	}

	[Theory]
	[InlineData("")]
	[InlineData("a")]
	[InlineData("   ")]
	public void CheckPassword_ShortCircuitsBlankOrSingleCharacterInput(
		string password)
	{
		PasswordResult result = Check(password);

		Assert.Equal(0, result.BitsOfEntropy);
		Assert.Equal(PasswordStrength.Poor, result.Strength);
		Assert.Equal(PasswordError.TooShort, result.Error);
		Assert.Equal(password.Length, result.Length);
	}

	[Fact]
	public void CommonPasswordCheck_IgnoresCase()
	{
		Assert.True(Check("PASSWORD").Error.HasFlag(PasswordError.TooCommon));
	}

	[Fact]
	public void Options_ChangeTheThresholds()
	{
		var validator = new CustomPasswordValidator<IdentityUser<Guid>>(
			Microsoft.Extensions.Options.Options.Create(
				new PasswordComplexityOptions
				{
					MinimumLength = 20,
					BitsThreshold = 100,
				}));

		PasswordResult result = validator.CheckPassword("zZ9!kq#Lm2@wXp");

		Assert.Equal(
			PasswordError.TooShort | PasswordError.NotComplexEnough,
			result.Error);
	}
}

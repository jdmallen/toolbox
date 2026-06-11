using JDMallen.Toolbox.AspNetCore.Dtos;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Confirms that the auth DTOs are records with value semantics and
/// non-destructive mutation, the payoff of the Phase 3 modernization.
/// </summary>
public class DtoTests
{
	[Fact]
	public void GitHubCallbackDto_StructurallyEqualInstancesAreEqual()
	{
		var first = new GitHubCallbackDto
		{
			Code = "abc",
			State = "xyz",
		};
		var second = new GitHubCallbackDto
		{
			Code = "abc",
			State = "xyz",
		};

		Assert.Equal(first, second);
	}

	[Fact]
	public void LoginDto_StructurallyEqualInstancesAreEqual()
	{
		var first = new LoginDto
		{
			Email = "user@test",
			Password = "secret",
		};
		var second = new LoginDto
		{
			Email = "user@test",
			Password = "secret",
		};

		Assert.Equal(first, second);
		Assert.True(first == second);
		Assert.Equal(first.GetHashCode(), second.GetHashCode());
	}

	[Fact]
	public void LoginDto_WithExpressionProducesModifiedCopy()
	{
		var original = new LoginDto
		{
			Email = "user@test",
			Password = "secret",
		};

		LoginDto rotated = original with { Password = "rotated" };

		Assert.NotEqual(original, rotated);
		Assert.Equal("user@test", rotated.Email);
		Assert.Equal("rotated", rotated.Password);
		Assert.Equal("secret", original.Password);
	}

	[Fact]
	public void RegisterDto_DiffersWhenAnyMemberDiffers()
	{
		var first = new RegisterDto
		{
			Email = "user@test",
			Password = "secret",
			DisplayName = "User",
		};
		RegisterDto second = first with { DisplayName = "Someone Else" };

		Assert.NotEqual(first, second);
	}
}

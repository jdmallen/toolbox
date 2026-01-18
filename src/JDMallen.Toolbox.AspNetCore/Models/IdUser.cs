using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JDMallen.Toolbox.Structs;
using Microsoft.AspNetCore.Identity;

namespace JDMallen.Toolbox.AspNetCore.Models;

/// <summary>
/// Custom Identity user class with GUID primary key and short ID support.
/// </summary>
public class IdUser : IdentityUser<Guid>
{
	private Guid _id;

	/// <summary>
	/// Initializes a new instance of the <see cref="IdUser" /> class.
	/// </summary>
	public IdUser()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="IdUser" /> class with the
	/// specified username.
	/// </summary>
	/// <param name="userName">The username for the user.</param>
	public IdUser(string userName) : base(userName)
	{
	}

	/// <summary>
	/// Gets or sets the primary key for this user.
	/// </summary>
	[Key]
	public override Guid Id
	{
		get => _id;
		set => _id = value;
	}

	/// <summary>
	/// Gets or sets the shortened base64-encoded version of the user's GUID.
	/// This property is not mapped to the database.
	/// </summary>
	[NotMapped]
	public MiniGuid ShortId
	{
		get => MiniGuid.Encode(_id);
		set => _id = MiniGuid.Decode(value);
	}
}

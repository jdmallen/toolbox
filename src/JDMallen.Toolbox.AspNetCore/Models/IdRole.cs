using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JDMallen.Toolbox.Structs;
using Microsoft.AspNetCore.Identity;

namespace JDMallen.Toolbox.AspNetCore.Models;

/// <summary>
/// Custom Identity role class with GUID primary key and short ID support.
/// </summary>
public class IdRole : IdentityRole<Guid>
{
	private Guid _id;

	/// <summary>
	/// Initializes a new instance of the <see cref="IdRole" /> class.
	/// </summary>
	public IdRole()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="IdRole" /> class with the
	/// specified role name.
	/// </summary>
	/// <param name="roleName">The name of the role.</param>
	public IdRole(string roleName) : base(roleName)
	{
	}

	/// <summary>
	/// Gets or sets the primary key for this role.
	/// </summary>
	[Key]
	public override Guid Id
	{
		get => _id;
		set => _id = value;
	}

	/// <summary>
	/// Gets or sets the shortened base64-encoded version of the role's GUID.
	/// This property is not mapped to the database.
	/// </summary>
	[NotMapped]
	public MiniGuid ShortId
	{
		get => MiniGuid.Encode(_id);
		set => _id = MiniGuid.Decode(value);
	}
}

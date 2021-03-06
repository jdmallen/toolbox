﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JDMallen.Toolbox.Structs;
using Microsoft.AspNetCore.Identity;

namespace JDMallen.Toolbox.AspNetCore.Models
{
    public class IdUser : IdentityUser<Guid>
    {
		private Guid _id;

		public IdUser()
		{
		}

		public IdUser(string userName) : base(userName)
		{
		}

		[Key]
		public override Guid Id
		{
			get => _id;
			set => _id = value;
		}

		[NotMapped]
		public MiniGuid ShortId
		{
			get => MiniGuid.Encode(_id);
			set => _id = MiniGuid.Decode(value);
		}
	}
}

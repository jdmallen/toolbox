﻿using System;

namespace JDMallen.Toolbox.AspNetCore.Constants
{
	[Flags]
	public enum PasswordError
	{
		None = 0,
		TooShort = 1,
		TooCommon = 2,
		NotComplexEnough = 4
	}
}

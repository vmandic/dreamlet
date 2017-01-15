﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.Models
{
	[Flags]
	public enum ActiveState : int
	{
		Active = 1,
		Inactive = 2,
		Deleted = 4
	}
	
	public enum DreamletRole : int
	{
		Admin = 1,
		User = 2
	}

	[Flags]
	public enum AccessFilter : int
	{
		Public = 1,
		User = 2,
		General = 4
	}
}

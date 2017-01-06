using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.Utilities
{
	[Flags]
	public enum ActiveState : int
	{
		Deleted = 0,
		Active = 1,
		Inactive = 2
	}

	[Flags]
	public enum DreamletRole : int
	{
		Admin = 0,
		User = 1
	}
}

using dreamlet.Utilities;
using System;

namespace dreamlet.DbEntities.Base
{
	public interface IBaseEntity
	{
		Guid Uid { get; set; }
		int Id { get; set; }
		DateTime CreatedAtUtc { get; set; }
		ActiveState ActiveState { get; set; }
		byte[] RowVersion { get; set; }
	}
}

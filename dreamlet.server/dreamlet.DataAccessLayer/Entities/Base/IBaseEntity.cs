using dreamlet.Models;
using System;

namespace dreamlet.DataAccessLayer.Entities.Base
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

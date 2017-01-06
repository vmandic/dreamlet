using dreamlet.Utilities;
using System;

namespace dreamlet.DataAccessLayer.Entities.Base
{
	public interface IBaseEntity
	{
		Guid Id { get; set; }
		int SequenceId { get; set; }
		DateTime CreatedAtUtc { get; set; }
		ActiveState ActiveState { get; set; }
		byte[] RowVersion { get; set; }
	}
}

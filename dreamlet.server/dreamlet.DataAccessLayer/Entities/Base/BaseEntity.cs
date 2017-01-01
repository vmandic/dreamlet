using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace dreamlet.DataAccessLayer.Entities.Base
{
	public abstract class BaseEntity : IBaseEntity
	{
		public ActiveState ActiveState { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public DateTime CreatedAtUtc { get; set; }
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]

		public Guid Id { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int SequenceId { get; set; }

		[Timestamp]
		public byte[] RowVersion { get; set; }
	}

	public class BaseEntityMapping<TEntity> : EntityTypeConfiguration<TEntity> where TEntity : class, IBaseEntity
	{
		public BaseEntityMapping()
		{
	
		}
	}
}

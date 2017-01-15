using dreamlet.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace dreamlet.DataAccessLayer.Entities.Base
{
	public abstract class BaseEntity : IBaseEntity
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public ActiveState ActiveState { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public DateTime CreatedAtUtc { get; set; }
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]

		public Guid Id { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int SequenceId { get; set; }

		[Timestamp]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public byte[] RowVersion { get; set; }
	}

	public class BaseEntityMapping<TEntity> : EntityTypeConfiguration<TEntity> where TEntity : class, IBaseEntity
	{
		public BaseEntityMapping()
		{
			this.Property(x => x.Id).HasColumnOrder(1);
			this.Property(x => x.SequenceId).HasColumnOrder(2);
			this.Property(x => x.ActiveState).HasColumnOrder(3);
			this.Property(x => x.CreatedAtUtc).HasColumnOrder(4);
			this.Property(x => x.RowVersion).HasColumnOrder(5);
		}
	}
}

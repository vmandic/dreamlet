using dreamlet.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

namespace dreamlet.DbEntities.Base
{
	public abstract class BaseEntity : IBaseEntity
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public ActiveState ActiveState { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public DateTime CreatedAtUtc { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Timestamp]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public byte[] RowVersion { get; set; }
	}

	public abstract class BaseEntityMapping<TEntity> : IModelMapping where TEntity : class, IBaseEntity
	{
    public virtual void Define(DbModelBuilder builder) => DefineBaseAndGetConfig(builder);

    protected EntityTypeConfiguration<TEntity> DefineBaseAndGetConfig(DbModelBuilder builder)
    {
      var e = builder.Entity<TEntity>();

      e.Property(x => x.Id).HasColumnOrder(1);
      e.Property(x => x.Uid).HasColumnOrder(2);
      e.Property(x => x.ActiveState).HasColumnOrder(3);
      e.Property(x => x.CreatedAtUtc).HasColumnOrder(4);
      e.Property(x => x.RowVersion).HasColumnOrder(5);

      return e;
    }
  }
}

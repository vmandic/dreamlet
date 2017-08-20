using dreamlet.DbEntities.Base;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;

namespace dreamlet.DbEntities.Models
{
	public class DreamTerm : BaseEntity
	{
		public DreamTerm()
		{
			this.DreamExplanations = new List<DreamExplanation>();
			this.DreamTermTags = new List<DreamTermTag>();
		}
		public int LanguageId { get; set; }
		public string Term { get; set; }

		public virtual Language Language { get; set; }
		public virtual DreamTermStatistic DreamTermStatistic { get; set; }
		public virtual ICollection<DreamExplanation> DreamExplanations { get; set; }
		public virtual ICollection<DreamTermTag> DreamTermTags { get; set; }

    [Export(typeof(IModelMapping))]
    class Map : BaseEntityMapping<DreamTerm>, IModelMapping
    {
      public override void Define(DbModelBuilder builder)
      {
        var e = DefineBaseAndGetConfig(builder);
        e.HasOptional(x => x.DreamTermStatistic).WithRequired(dts => dts.DreamTerm);
      }
    }
  }
}

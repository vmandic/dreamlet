using dreamlet.DbEntities.Base;
using System;
using System.Collections.Generic;

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
	}

  public class DreamTermMapping : BaseEntityMapping<DreamTerm>
	{
		public DreamTermMapping() : base()
		{
			this.HasOptional(x => x.DreamTermStatistic).WithRequired(dts => dts.DreamTerm);
		}
	}
}

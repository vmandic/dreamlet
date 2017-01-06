using dreamlet.DataAccessLayer.Entities.Base;
using System;
using System.Collections.Generic;

namespace dreamlet.DataAccessLayer.Entities.Models
{
	public class DreamTerm : BaseEntity
	{
		public DreamTerm()
		{
			this.DreamExplanations = new List<DreamExplanation>();
			this.DreamTermTags = new List<DreamTermTag>();
		}
		public Guid LanguageId { get; set; }
		public string Term { get; set; }

		public virtual Language Language { get; set; }
		public virtual ICollection<DreamExplanation> DreamExplanations { get; set; }
		public virtual ICollection<DreamTermTag> DreamTermTags { get; set; }
	}

	internal class DreamTermMapping : BaseEntityMapping<DreamTerm>
	{
		public DreamTermMapping() : base()
		{

		}
	}
}

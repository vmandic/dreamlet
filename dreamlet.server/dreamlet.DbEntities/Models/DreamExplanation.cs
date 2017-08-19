using dreamlet.DbEntities.Base;
using System;

namespace dreamlet.DbEntities.Models
{
	public class DreamExplanation : BaseEntity
	{
		public string Explanation { get; set; }
		public int DreamTermId { get; set; }

		public virtual DreamTerm DreamTerm { get; set; }
	}

	public class DreamExplanationMapping : BaseEntityMapping<DreamExplanation>
	{
		public DreamExplanationMapping() : base()
		{
			
		}
	}
}

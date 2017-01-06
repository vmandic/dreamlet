using dreamlet.DataAccessLayer.Entities.Base;
using System;

namespace dreamlet.DataAccessLayer.Entities.Models
{
    public class DreamExplanation : BaseEntity
    {
		public string Explanation { get; set; }
		public Guid DreamTermId { get; set; }

		public virtual DreamTerm DreamTerm { get; set; }
	}

	internal class DreamExplanationMapping : BaseEntityMapping<DreamExplanation>
	{
		public DreamExplanationMapping() : base()
		{
			
		}
	}
}

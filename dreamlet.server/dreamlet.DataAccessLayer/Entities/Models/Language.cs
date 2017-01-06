using dreamlet.DataAccessLayer.Entities.Base;
using System.Collections.Generic;

namespace dreamlet.DataAccessLayer.Entities.Models
{
	public class Language : BaseEntity
	{
		public Language()
		{
			this.DreamTerms = new List<DreamTerm>();
		}

		public string InternationalCode { get; set; }

		public virtual ICollection<DreamTerm> DreamTerms { get; set; }
	}

	internal class LanguageMapping : BaseEntityMapping<Language>
	{
		public LanguageMapping() : base()
		{

		}
	}
}

using dreamlet.DbEntities.Base;
using System.Collections.Generic;

namespace dreamlet.DbEntities.Models
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

  public class LanguageMapping : BaseEntityMapping<Language>
	{
		public LanguageMapping() : base()
		{

		}
	}
}

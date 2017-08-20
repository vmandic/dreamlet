using dreamlet.DbEntities.Base;
using System.Collections.Generic;
using System.ComponentModel.Composition;

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

    [Export(typeof(IModelMapping))]
    class Map : BaseEntityMapping<Language> { }
  }
}

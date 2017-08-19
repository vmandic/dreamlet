using dreamlet.DbEntities.Base;
using System.Collections.Generic;

namespace dreamlet.DbEntities.Models
{
	public class DreamTag : BaseEntity
	{
		public DreamTag()
		{
			this.DreamTermTags = new List<DreamTermTag>();
		}

		public string Tag { get; set; }
		public virtual ICollection<DreamTermTag> DreamTermTags { get; set; }
	}

  public class DreamTagMapping : BaseEntityMapping<DreamTag>
	{
		public DreamTagMapping() : base()
		{

		}
	}
}

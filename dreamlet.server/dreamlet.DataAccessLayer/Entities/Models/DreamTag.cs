using dreamlet.DataAccessLayer.Entities.Base;
using System.Collections.Generic;

namespace dreamlet.DataAccessLayer.Entities.Models
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

	internal class DreamTagMapping : BaseEntityMapping<DreamTag>
	{
		public DreamTagMapping()
		{

		}
	}
}

using dreamlet.DbEntities.Base;
using System.Collections.Generic;
using System.ComponentModel.Composition;

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

    [Export(typeof(IModelMapping))]
    class Map : BaseEntityMapping<DreamTag> { }
  }
}

using dreamlet.DbEntities.Base;
using System.ComponentModel.Composition;

namespace dreamlet.DbEntities.Models
{
	public class DreamTermTag : BaseEntity
	{
		public int DreamTagId { get; set; }
		public int DreamTermId { get; set; }

		public virtual DreamTag DreamTag { get; set; }
		public virtual DreamTerm DreamTerm { get; set; }

    [Export(typeof(IModelMapping))]
    class Map : BaseEntityMapping<DreamTermTag> { }
  }

}

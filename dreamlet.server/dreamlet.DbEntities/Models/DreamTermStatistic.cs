using dreamlet.DbEntities.Base;
using System.ComponentModel.Composition;

namespace dreamlet.DbEntities.Models
{
	public class DreamTermStatistic : BaseEntity
	{
		public int DreamTermId { get; set; }
		public long VisitCount { get; set; }
		public long LikeCount { get; set; }
		public virtual DreamTerm DreamTerm { get; set; }

    [Export(typeof(IModelMapping))]
    class Map : BaseEntityMapping<DreamTermStatistic> { }
  }
}

using dreamlet.DbEntities.Base;

namespace dreamlet.DbEntities.Models
{
	public class DreamTermStatistic : BaseEntity
	{
		public DreamTermStatistic()
		{

		}

		public int DreamTermId { get; set; }
		public long VisitCount { get; set; }
		public long LikeCount { get; set; }
		public virtual DreamTerm DreamTerm { get; set; }
	}

  public class DreamTermStatisticMapping : BaseEntityMapping<DreamTermStatistic>
	{
		public DreamTermStatisticMapping() : base()
		{

		}
	}
}

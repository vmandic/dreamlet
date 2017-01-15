using dreamlet.DataAccessLayer.Entities.Base;
using System;

namespace dreamlet.DataAccessLayer.Entities.Models
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

	internal class DreamTermStatisticMapping : BaseEntityMapping<DreamTermStatistic>
	{
		public DreamTermStatisticMapping() : base()
		{
			this.HasKey(x => x.DreamTermId);
		}
	}
}

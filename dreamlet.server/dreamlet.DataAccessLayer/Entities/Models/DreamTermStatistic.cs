using dreamlet.DataAccessLayer.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.Entities.Models
{
	public class DreamTermStatistic : BaseEntity
	{
		public DreamTermStatistic()
		{
				
		}

		public Guid DreamTermId { get; set; }
		public long VisitCount { get; set; }
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

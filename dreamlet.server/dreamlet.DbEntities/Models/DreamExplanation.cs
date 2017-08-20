using dreamlet.DbEntities.Base;
using System.ComponentModel.Composition;

namespace dreamlet.DbEntities.Models
{
	public class DreamExplanation : BaseEntity
	{
		public string Explanation { get; set; }
		public int DreamTermId { get; set; }

		public virtual DreamTerm DreamTerm { get; set; }

    [Export(typeof(IModelMapping))]
    class Map : BaseEntityMapping<DreamExplanation> { }
  }
}

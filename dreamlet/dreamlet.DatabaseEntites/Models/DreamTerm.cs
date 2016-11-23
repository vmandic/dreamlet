using dreamlet.DatabaseEntites.Base;
using System.Collections.Generic;

namespace dreamlet.DatabaseEntites.Models
{
    public class DreamTerm : BaseMongoEntity
    {
        public DreamTerm()
        {
            this.Explanations = new List<DreamExplanation>();
        }

        public string Term { get; set; }
        public IEnumerable<DreamExplanation> Explanations { get; set; }
    }
}

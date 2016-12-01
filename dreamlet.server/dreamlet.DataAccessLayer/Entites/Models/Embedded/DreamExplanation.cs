using dreamlet.DataAccessLayer.Entities.Base;
using MongoDB.Bson;

namespace dreamlet.DataAccessLayer.Entities.Models
{
    public class DreamExplanation : BaseMongoEntity
    {
        public DreamExplanation()
        {
            Id = ObjectId.GenerateNewId().ToString();
        }

        public string Explanation { get; set; }
    }
}

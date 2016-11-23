using dreamlet.DatabaseEntites.Base;
using MongoDB.Bson;

namespace dreamlet.DatabaseEntites.Models
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

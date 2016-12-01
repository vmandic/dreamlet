using dreamlet.DataAccessLayer.Entities.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace dreamlet.DataAccessLayer.Entities.Models
{
    public class DreamTermTag : BaseMongoMetaEntity
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string DreamTagId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string DreamTermId { get; set; }
        public T DreamTag<T>(IMongoCollection<T> collection) where T : IBaseMongoEntity 
            => collection.Find(x => x.Id == DreamTagId).FirstOrDefault();
        
        public T DreamTerm<T>(IMongoCollection<T> collection) where T : IBaseMongoEntity
            => collection.Find(x => x.Id == DreamTermId).FirstOrDefault();
    }
}

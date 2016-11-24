using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.Serialization;

namespace dreamlet.DataAccessLayer.Entities.Base
{
    [DataContract]
    [Serializable]
    [BsonIgnoreExtraElements(Inherited = true)]
    public class BaseMongoEntity : IBaseMongoEntity
    {
        public BaseMongoEntity()
        {
            SetObjectId(ObjectId.GenerateNewId(DateTime.UtcNow));
        }

        public ObjectId GetObjectId() 
            => !String.IsNullOrWhiteSpace(Id) ? new ObjectId(Id) : MongoDB.Bson.ObjectId.Empty;

        public void SetObjectId(ObjectId id)
        {
            Id = id.ToString();
        }

        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; }
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.Serialization;

namespace dreamlet.DatabaseEntites.Base
{
    [DataContract]
    [Serializable]
    [BsonIgnoreExtraElements(Inherited = true)]
    public class BaseMongoEntity : IBaseMongoEntity<string>
    {
        public ObjectId GetObjectId() 
            => !String.IsNullOrWhiteSpace(Id) ? new ObjectId(Id) : MongoDB.Bson.ObjectId.Empty;
            

        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; }
    }
}

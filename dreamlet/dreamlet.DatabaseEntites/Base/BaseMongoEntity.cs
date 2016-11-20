using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.Serialization;

namespace dreamlet.DatabaseEntites.Base
{
    [DataContract]
    [Serializable]
    [BsonIgnoreExtraElements(Inherited = true)]
    public class BaseMongoEntity : BsonDocument, IBaseMongoEntity<string>
    {
        public ObjectId ObjectId
        {
            get
            {
                return new ObjectId(Id);
            }
        }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}

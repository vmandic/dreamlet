using dreamlet.DataAccessLayer.Entities.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace dreamlet.DataAccessLayer.Entities.Models
{
    public class Meta : BaseMongoEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedByUserId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UpdatedByUserId { get; set; }
    }
}

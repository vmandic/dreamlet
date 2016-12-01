using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.Serialization;

namespace dreamlet.DataAccessLayer.Entities.Base
{
    public interface IBaseMongoEntity : IBaseMongoEntity<string>
    {
        void SetObjectId(ObjectId id);
    }

    /// <summary>
    /// Generic Entity interface.
    /// </summary>
    public interface IBaseMongoEntity<TKey>
    {
        /// <summary>
        /// Gets or sets the Id of the Entity.
        /// </summary>
        /// <value>Id of the Entity.</value>
        [BsonId]
        [DataMember]
        TKey Id { get; set; }

        ObjectId GetObjectId();
    }
}

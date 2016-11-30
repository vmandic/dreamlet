using dreamlet.DataAccessLayer.MongoDbContext;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DataAccessLayer.Entities.Base;

namespace dreamlet.BusinessLogicLayer.Services.Base
{
    public interface IBaseService
    {
        IMongoContext MongoDatabaseContext { get; }

        /// <summary>
        /// Resolves a string Id based cached repository instance which exposes the MongoDatabase and MongoCollection.
        /// </summary>
        IRepository<TDocument> Repository<TDocument>() where TDocument : IBaseMongoEntity;
    }
}

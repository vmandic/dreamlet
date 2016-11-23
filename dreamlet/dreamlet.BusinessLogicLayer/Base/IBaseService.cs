using dreamlet.DataAccessLayer.MongoDbContext;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DatabaseEntites.Base;

namespace dreamlet.BusinessLogicLayer.Base
{
    public interface IBaseService
    {
        IMongoContext MongoDatabaseContext { get; }

        IRepository<TDocument, string> Repository<TDocument>() where TDocument : IBaseMongoEntity<string>;
    }
}

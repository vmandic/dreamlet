using dreamlet.DataAccessLayer.MongoDbContext;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DatabaseEntites.Base;

namespace dreamlet.DataAccessLayer.UnitOfWork
{
    public interface IUnitOfWork
    {
        IMongoContext Context { get; }
        void Dispose();
        void Dispose(bool disposing);
        IRepository<TDocument, TKey> Repository<TDocument, TKey>() where TDocument : IBaseMongoEntity<TKey>;
    }
}

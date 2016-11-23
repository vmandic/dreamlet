using MongoDB.Driver;

namespace dreamlet.DataAccessLayer.MongoDbContext
{
    public interface IMongoContext
    {
        IMongoCollection<TDocument> Collection<TDocument>();

        IMongoDatabase Database { get; }

        IMongoClient Client { get; }
    }
}

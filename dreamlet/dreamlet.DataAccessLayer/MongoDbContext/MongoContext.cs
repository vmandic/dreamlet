using MongoDB.Driver;
using System.Linq;

namespace dreamlet.DataAccessLayer.MongoDbContext
{
    public class MongoContext : IMongoContext
    {
        private readonly string _databaseName;
        private readonly MongoClientSettings _clientSettings;

        private static IMongoDatabase _database;
        private static IMongoClient _client;

        public MongoContext()
        {

        }

        public MongoContext(MongoClientSettings settings)
        {
            _clientSettings = settings;
            _databaseName = settings.Credentials.First().Source;
        }

        public IMongoDatabase Database
        {
            get
            {
                return _database ?? (_database = Client.GetDatabase(_databaseName));
            }
        }

        public IMongoClient Client
        {
            get
            {
                return _client ?? (_client = new MongoClient(_clientSettings));
            }
        }

        public IMongoCollection<TDocument> Collection<TDocument>()
        {
            string collectionName = typeof(TDocument).Name;
            return Database.GetCollection<TDocument>(collectionName);
        }
    }
}

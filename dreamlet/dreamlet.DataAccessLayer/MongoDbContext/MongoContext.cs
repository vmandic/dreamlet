using MongoDB.Driver;

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

        public MongoContext(MongoClientSettings settings, string databaseName)
        {
            _clientSettings = settings;
            _databaseName = databaseName;
        }

        public IMongoDatabase Database
        {
            get
            {
                return _database ?? (_database = _client.GetDatabase(_databaseName));
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
            return _database.GetCollection<TDocument>(collectionName);
        }
    }
}

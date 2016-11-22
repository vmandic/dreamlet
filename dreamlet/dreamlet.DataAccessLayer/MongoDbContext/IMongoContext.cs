using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.MongoDbContext
{
    public interface IMongoContext
    {
        IMongoCollection<TDocument> Collection<TDocument>();

        IMongoDatabase Database { get; }

        IMongoClient Client { get; }
    }
}

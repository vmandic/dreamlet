using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace dreamlet.DataAccessLayer.MongoDbContext
{
    public class MongoContext : IMongoContext
    {
        public MongoContext()
        {
            // TODO: initialize mongo server and database
        }

        public IMongoCollection<TDocument> Collection<TDocument>()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace dreamlet.DataAccessLayer.MongoDbContext
{
    public class DreamletMongoContext : MongoContext, IMongoContext
    {
        private static MongoClientSettings _settings = new MongoClientSettings()
        {
            GuidRepresentation = MongoDB.Bson.GuidRepresentation.Standard,
            ConnectTimeout = new TimeSpan(0, 1, 0),
            WriteConcern = WriteConcern.Acknowledged,
            Credentials = new List<MongoCredential> { MongoCredential.CreateCredential("dreamlet", "sa", "sa") },
            Server = new MongoServerAddress("localhost")
        };

        public DreamletMongoContext() : base(_settings, "dreamlet")
        {
            
        }
    }
}

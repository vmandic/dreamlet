using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace dreamlet.DataAccessLayer.MongoDbContext
{
    public class DreamletMongoContext : MongoContext, IMongoContext
    {
        private static readonly string DB_NAME = ConfigurationManager.AppSettings["DB_NAME"].ToString();
        private static readonly string DB_USR = ConfigurationManager.AppSettings["DB_USR"].ToString();
        private static readonly string DB_PWD = ConfigurationManager.AppSettings["DB_PWD"].ToString();

        private static MongoClientSettings _settings;

        public static MongoClientSettings Settings
        {
            get
            {
                return _settings ?? (_settings = new MongoClientSettings()
                {
                    GuidRepresentation = MongoDB.Bson.GuidRepresentation.Standard,
                    ConnectTimeout = new TimeSpan(0, 1, 0),
                    WriteConcern = WriteConcern.Acknowledged,
                    Credentials = new List<MongoCredential> { MongoCredential.CreateCredential(DB_NAME, DB_USR, DB_PWD) },
                    Server = new MongoServerAddress("localhost")
                });
            }
        }

        public DreamletMongoContext() : base(Settings)
        {

        }
    }
}

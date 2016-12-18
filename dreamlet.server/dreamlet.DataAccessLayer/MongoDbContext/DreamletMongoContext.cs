using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Reflection;

namespace dreamlet.DataAccessLayer.MongoDbContext
{
	[Export(typeof(IMongoContext))]
    public class DreamletMongoContext : MongoContext, IMongoContext
    {
		private static readonly string DB_NAME;
        private static readonly string DB_USR;
        private static readonly string DB_PWD;

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

		static DreamletMongoContext()
		{
			// override default if initializing
			if (!Assembly.GetEntryAssembly().FullName.Contains("dreamlet.DatabaseInit"))
			{
				DB_NAME = ConfigurationManager.AppSettings["DB_NAME"].ToString();
				DB_USR = ConfigurationManager.AppSettings["DB_USR"].ToString();
				DB_PWD = ConfigurationManager.AppSettings["DB_PWD"].ToString();
			}
		}

        public DreamletMongoContext(MongoClientSettings settings = null) : base(settings ?? Settings)
        {

        }
    }
}

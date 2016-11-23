using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Configuration;
using dreamlet.Generic.Tests.Services;
using dreamlet.DatabaseEntites.Models;
using MongoDB.Bson;

namespace dreamlet.Generic.Tests
{
    [TestClass]
    public class RepositoryTests
    {
        private static MongoClientSettings _settings;
        private static string DB_NAME, DB_USR, DB_PWD;

        [TestInitialize]
        public void Setup()
        {
            DB_NAME = ConfigurationManager.AppSettings["DB_NAME"].ToString();
            DB_USR = ConfigurationManager.AppSettings["DB_USR"].ToString();
            DB_PWD = ConfigurationManager.AppSettings["DB_PWD"].ToString();

            var cli = new MongoClient();
            var db = cli.GetDatabase(DB_NAME);

            Cleanup();
            _AddUser(db);

            _settings = new MongoClientSettings()
            {
                GuidRepresentation = MongoDB.Bson.GuidRepresentation.Standard,
                ConnectTimeout = new TimeSpan(0, 1, 0),
                WriteConcern = WriteConcern.Acknowledged,
                Credentials = new List<MongoCredential> { MongoCredential.CreateCredential(DB_NAME, DB_USR, DB_PWD) },
                Server = new MongoServerAddress("localhost")
            };
        }

        private static void _AddUser(IMongoDatabase db)
        {
            var readWriteRole = new BsonDocument { { "role", "readWrite" }, { "db", DB_NAME } };
            var dbAdminRole = new BsonDocument { { "role", "dbAdmin" }, { "db", DB_NAME } };

            var roles = new List<BsonDocument> { readWriteRole, dbAdminRole };
            var user = new BsonDocument { { "createUser", DB_USR }, { "pwd", DB_PWD }, { "roles", new BsonArray(roles) } };
            db.RunCommand<BsonDocument>(user);
        }

        [TestCleanup]
        public void Cleanup()
        {
            var cli = new MongoClient();
            var db = cli.GetDatabase(DB_NAME);

            if (db != null)
            {
                var dropAllUsersCommand = new BsonDocument { { "dropAllUsersFromDatabase", 1 } };
                db.RunCommand<BsonDocument>(dropAllUsersCommand);
                cli.DropDatabase(DB_NAME);
            }
        }

        [TestMethod]
        public void TestMongoRepo()
        {
            var service = new GenericService();
            var repo = service.Repository<DreamTerm>();

            var d = new DreamTerm()
            {
                Term = "Car",
                Explanations = new List<DreamExplanation>()
                {
                    new DreamExplanation() { Explanation = "You are awesome." },
                    new DreamExplanation() { Explanation = "You've been driving a car obviously!" }
                }
            };

            repo.Add(d);

            Assert.IsTrue(repo.Count() == 1);
        }
    }
}

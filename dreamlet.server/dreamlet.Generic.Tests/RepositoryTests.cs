using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Configuration;
using dreamlet.Generic.Tests.Services;
using dreamlet.DataAccessLayer.Entities.Models;
using MongoDB.Bson;

namespace dreamlet.Generic.Tests
{
    [TestClass]
    public class RepositoryTests
    {
        private static MongoClientSettings _settings;
        private static string DB_NAME, DB_USR, DB_PWD;
		private static MongoClient cli;

        [TestInitialize]
        public void Setup()
        {
            DB_NAME = ConfigurationManager.AppSettings["DB_NAME"].ToString();
            DB_USR = ConfigurationManager.AppSettings["DB_USR"].ToString();
            DB_PWD = ConfigurationManager.AppSettings["DB_PWD"].ToString();

			cli = new MongoClient();
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
            cli = cli ?? new MongoClient();
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
            // PREPARE:
            var service = new GenericService();
            var dreamTermRepo = service.Repository<DreamTerm>();
            var userRepo = service.Repository<User>();
            var langRepo = service.Repository<Language>();
            var utcnow = DateTime.UtcNow;
            var usrid = ObjectId.GenerateNewId(utcnow).ToString();

            // ACT:
            var usr1 = new User()
            {
                Id = usrid,
                Email = "test.test@test.com",
                PasswordHash = "123456",
                Role = "admin"
            };

            userRepo.Add(usr1);

            var lang1 = new Language()
            {
                Code = "HR-HR",
                Meta = new Meta()
                {
                    CreatedAt = utcnow,
                    UpdatedAt = utcnow,
                    CreatedByUserId = usrid,
                    UpdatedByUserId = usrid
                }
            };

            langRepo.Add(lang1);

            var d = new DreamTerm()
            {
                Term = "Car",
                LanguageId = lang1.Id,
                DreamExplanations = new List<DreamExplanation>()
                {
                    new DreamExplanation() { Explanation = "You are awesome." },
                    new DreamExplanation() { Explanation = "You've been driving a car obviously!" }
                }
            };

            var lang2 = d.Language(service.Repository<Language>);
            dreamTermRepo.Add(d);

            // ASSERT:
            Assert.AreEqual(lang1.Id, lang2.Id);
            Assert.IsTrue(dreamTermRepo.Count() == 1);
        }
    }
}

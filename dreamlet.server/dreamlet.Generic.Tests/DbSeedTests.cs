using dreamlet.BusinessLogicLayer.Ioc;
using dreamlet.DataAccessLayer.BootstarpDatabase;
using dreamlet.DataAccessLayer.Entities.Base;
using dreamlet.DataAccessLayer.Entities.Models;
using dreamlet.Generic.Tests.Services;
using DryIoc;
using DryIoc.MefAttributedModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace dreamlet.Generic.Tests
{
	[TestClass]
	public class DbSeedTests
	{
		class JsonDreamTerm : BaseMongoEntity
		{
			[JsonProperty("name")]
			public string Name { get; set; }
			[JsonProperty("explanations")]
			public IEnumerable<string> Explanations { get; set; }
		}

		private static MongoClientSettings _settings;
		private static string DB_NAME, DB_USR, DB_PWD;
		private static IContainer _container;

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

			// bootstrap IoC
			_container = new Container().WithMefAttributedModel();
			IocBootstrapper.RegisterDependencies(_container);
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

		private Meta _NewMeta (string usrid) => new Meta()
		{
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			CreatedByUserId = usrid,
			UpdatedByUserId = usrid
		};

		[TestMethod]
		public void Should_open_file_path_for_seed()
		{
			// PREPARE
			var b = new DatabaseBootstrapper();
			var obj = JsonConvert.DeserializeObject<IEnumerable<JsonDreamTerm>>(b.Text);


			var service = new GenericService();
			var dreamTermRepo = service.Repository<DreamTerm>();
			var userRepo = service.Repository<User>();
			var langRepo = service.Repository<Language>();
			var utcnow = DateTime.UtcNow;
			var usrid = ObjectId.GenerateNewId(utcnow).ToString();

			// ACT
			var lang1 = new Language()
			{
				Id = ObjectId.GenerateNewId(DateTime.UtcNow.AddDays(1)).ToString(),
				Code = "en-US",
				Meta = _NewMeta(usrid)
			};

			langRepo.Add(lang1);

			var dreamTerms = obj.Select(x => new DreamTerm
			{
				LanguageId = lang1.Id,
				Meta = _NewMeta(usrid),
				Term = x.Name,
				Explanations = x.Explanations.Select(de => new DreamExplanation { Explanation = de })
			});

			dreamTermRepo.Add(dreamTerms);

			// ASSERT
			Assert.IsNotNull(obj);
			Assert.IsInstanceOfType(obj, typeof(IEnumerable<JsonDreamTerm>));
		}
	}
}

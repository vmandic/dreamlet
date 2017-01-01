using dreamlet.BusinessLogicLayer.Ioc;
using dreamlet.DataAccessLayer.Entities.Models;
using dreamlet.DatabaseInit.ImportModels;
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

			// bootstrap IoC
			_container = IocBootstrapper.RegisterDependencies(new Container().WithMefAttributedModel());
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
			string json = System.IO.File.ReadAllText(@"Scrapes/dream-scrape1-take2-formatted.json");
			var obj = JsonConvert.DeserializeObject<IEnumerable<JsonDreamTerm>>(json);

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
				DreamExplanations = x.Explanations.Select(de => new DreamExplanation { Explanation = de })
			});

			dreamTermRepo.Add(dreamTerms);

			// ASSERT
			Assert.IsNotNull(obj);
			Assert.IsInstanceOfType(obj, typeof(IEnumerable<JsonDreamTerm>));
		}
	}
}

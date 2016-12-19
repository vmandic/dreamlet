using dreamlet.DataAccessLayer.Entities.Models;
using dreamlet.DataAccessLayer.MongoDbContext;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DatabaseInit.ImportModels;
using dreamlet.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace dreamlet.DatabaseInit
{
	class Program
	{
		static IRepository<Language> _LanguageRepository { get; set; }
		static IRepository<DreamTerm> _DreamTermRepository { get; set; }
		static IRepository<User> _UserRepository { get; set; }

		// Language cultures
		static string EN_US = "en-US";
		static string HR_HR = "hr-HR";

		// Admin
		static string ADMIN_EMAIL = "admin@dreamlet.me";

		static void Main(string[] args)
		{
			_SetupImporter();

			_TryInsertUserAdmin();
			_TryInsertLanguages();
			_TryInsertDreamTermsScrape1();
		}

		static void _SetupImporter()
		{
			var dbcs = ConfigurationManager.ConnectionStrings["cs"].ToString();
			var mongoClient = new MongoClient(dbcs);
			var settings = mongoClient.Settings;
			var dbContext = new DreamletMongoContext(settings);
			var repositoryFactory = new RepositoryFactory(dbContext);

			_DreamTermRepository = repositoryFactory.Get<DreamTerm>();
			_LanguageRepository = repositoryFactory.Get<Language>();
			_UserRepository = repositoryFactory.Get<User>();
		}

		static Meta _NewMeta(string id) => new Meta()
		{
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			CreatedByUserId = id,
			UpdatedByUserId = id
		};

		static void _TryInsertLanguages()
		{
			Debug.WriteLine("INSERTING LANGUAGES...");

			var adminUser = _UserRepository.Where(x => x.Email == ADMIN_EMAIL).FirstOrDefault();

			// English en-US
			if (!_LanguageRepository.Any(x => x.Code == EN_US))
			{
				_LanguageRepository.Add(new Language()
				{
					Code = "en-US",
					Meta = _NewMeta(adminUser.Id)
				});
				Debug.WriteLine($"Inserted \"{EN_US}\" language.");
			}
			else
				Debug.WriteLine($"\"{EN_US}\" language already exists, skipping.");

			// Croatian hr-HR
			if (!_LanguageRepository.Any(x => x.Code == HR_HR))
			{
				_LanguageRepository.Add(new Language()
				{
					Code = HR_HR,
					Meta = _NewMeta(adminUser.Id)
				});
				Debug.WriteLine($"Inserted \"{HR_HR}\" language.");
			}
			else
				Debug.WriteLine($"\"{HR_HR}\" language already exists, skipping.");

		}

		static void _TryInsertUserAdmin()
		{
			Debug.WriteLine("INSERTING USER ADMIN...");

			if (!_UserRepository.Any(x => x.Email == ADMIN_EMAIL))
			{
				var id = ObjectId.GenerateNewId(DateTime.UtcNow.AddDays(1)).ToString();

				_UserRepository.Add(new User()
				{
					Id = id,
					Email = ADMIN_EMAIL,
					Meta = _NewMeta(id),
					PasswordHash = "0",
					Role = DreamletRoles.Admin.ToString()
				});

				Debug.WriteLine($"Inserted admin user with id: \"{id}\"");
			}
			else
				Debug.WriteLine("Admin user already exists, skipping.");
		}

		static void _TryInsertDreamTermsScrape1()
		{
			Debug.WriteLine("INSERTING DREAM TERMS SCRAPE 1...");

			string json = System.IO.File.ReadAllText(@"Scrapes/dream-scrape1-take2-formatted.json");
			var obj = JsonConvert.DeserializeObject<IEnumerable<JsonDreamTerm>>(json);

			var engLang = _LanguageRepository.Where(x => x.Code == EN_US).FirstOrDefault();
			var adminUser = _UserRepository.Where(x => x.Email == ADMIN_EMAIL).FirstOrDefault();

			var dreamTerms = obj.Select(x => new DreamTerm
			{
				LanguageId = engLang.Id,
				Meta = _NewMeta(adminUser.Id),
				Term = x.Name.Replace("|", "\'"), // replace due to import formatting hack
				Explanations = x.Explanations.Select(explanation => new DreamExplanation { Explanation = explanation.Replace("|", "\'") }) // replace due to import formatting hack
			}).ToList();

			// NOTE: Term "Armpit" imported with error.
			dreamTerms.ForEach(dt => {
				if (!_DreamTermRepository.Any(x => x.Term == dt.Term))
				{
					Debug.WriteLine($"Inserting term \"{dt.Term}\" with {dt.Explanations.Count()} explanation(s).");
					_DreamTermRepository.Add(dt);
				}
				else
					Debug.WriteLine($"Dream term \"{dt.Term}\" already exists, skipping.");
			});
		}
	}
}

using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.DataAccessLayer.Entities.Models;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
		private static RepositoryFactory _repoFactory;

		static void Main(string[] args)
		{
			_SetupImporter();

			_TryInsertUserAdmin();
			_TryInsertLanguages();
			_TryInsertDreamTermsScrape1();

			Console.WriteLine("DONE!");
			Console.ReadKey();
		}

		static void _SetupImporter()
		{
			_repoFactory = new RepositoryFactory(new DreamletEfContext());

			_LanguageRepository = _repoFactory.Get<Language>();
			_DreamTermRepository = _repoFactory.Get<DreamTerm>();
			_UserRepository = _repoFactory.Get<User>();
		}

		static void _TryInsertLanguages()
		{
			Console.WriteLine("INSERTING LANGUAGES...");

			var adminUser = _UserRepository.Set.Where(x => x.Email == ADMIN_EMAIL).FirstOrDefault();

			// English en-US
			if (!_LanguageRepository.HasAny(x => x.InternationalCode == EN_US))
			{
				var savedEnUs = _LanguageRepository.CreateAndSaveAsync(new Language()
				{
					InternationalCode = EN_US
				}).Result;

				if (savedEnUs)
					Console.WriteLine($"Inserted \"{EN_US}\" language.");
				else
					throw new Exception("Did not insert en-us language. Saving returned false.");
			}
			else
				Console.WriteLine($"\"{EN_US}\" language already exists, skipping.");

			// Croatian hr-HR
			if (!_LanguageRepository.HasAny(x => x.InternationalCode == HR_HR))
			{
				var savedHrHr = _LanguageRepository.CreateAndSaveAsync(new Language()
				{
					InternationalCode = HR_HR
				}).Result;

				if (savedHrHr)
					Console.WriteLine($"Inserted \"{HR_HR}\" language.");
				else
					throw new Exception("Did not insert hr-hr language. Saving returned false.");
			}
			else
				Console.WriteLine($"\"{HR_HR}\" language already exists, skipping.");

		}

		static void _TryInsertUserAdmin()
		{
			Console.WriteLine("INSERTING USER ADMIN...");

			if (!_UserRepository.HasAny(x => x.Email == ADMIN_EMAIL))
			{
				var saved = _UserRepository.CreateAndSaveAsync(new User()
				{
					Email = ADMIN_EMAIL,
					PasswordHash = "0",
					Role = DreamletRole.Admin
				}).Result;

				if (saved)
					Console.WriteLine($"Inserted admin user with id: \"{_UserRepository.Set.First().Id}\"");
				else
					throw new Exception("Did not insert admin user. Saving returned false.");
			}
			else
				Console.WriteLine("Admin user already exists, skipping.");
		}

		static void _TryInsertDreamTermsScrape1()
		{
			Console.WriteLine("INSERTING DREAM TERMS SCRAPE 1...");

			string json = System.IO.File.ReadAllText(@"Scrapes/dream-scrape1-take2-formatted.json");
			var obj = JsonConvert.DeserializeObject<IEnumerable<JsonDreamTerm>>(json);

			var engLang = _LanguageRepository.Find(x => x.InternationalCode == EN_US);
			var adminUser = _UserRepository.Find(x => x.Email == ADMIN_EMAIL);

			var dreamTerms = obj.Select(x => new DreamTerm
			{
				LanguageId = engLang.Id,
				Term = x.Name.Replace("|", "\'"), // replace due to import formatting hack
				DreamExplanations = x.Explanations.Select(explanation => 
					new DreamExplanation {
						Explanation = explanation.Replace("|", "\'") // replace due to import formatting hack
					}).ToList() 
			}).ToList();

			// NOTE: Term "Armpit" imported with error.
			dreamTerms.AsParallel().ForAll(dt =>
			{
				//if (!_DreamTermRepository.HasAny(x => x.Term == dt.Term))
				//{
					Console.WriteLine($"Inserting term \"{dt.Term}\" with {dt.DreamExplanations.Count()} explanation(s).");
					_DreamTermRepository.Create(dt);
				//}
				//else
				//	Console.WriteLine($"Dream term \"{dt.Term}\" already exists, skipping.");
			});

			try
			{
				_repoFactory.DreamletContext.SaveChanges();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}

	internal class JsonDreamTerm
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("explanations")]
		public IEnumerable<string> Explanations { get; set; }
	}
}

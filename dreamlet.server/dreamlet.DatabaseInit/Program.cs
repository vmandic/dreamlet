using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.DataAccessLayer.Entities.Models;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DataAccessLayer.UnitOfWork;
using dreamlet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace dreamlet.DatabaseInit
{
	class Program
	{
		private static IRepository<Language> _LanguageRepository { get; set; }
		private static IRepository<DreamTerm> _DreamTermRepository { get; set; }
		private static IRepository<User> _UserRepository { get; set; }
		private static IRepositoryFactory _repoFactory;
		private static IUnitOfWork _uow;

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
			_WriteInsertionScriptToFile();

			Console.WriteLine("DONE!");
		}

		static void _SetupImporter()
		{
			var ctx = new DreamletEfContext();
			_repoFactory = new RepositoryFactory(ctx);
			_uow = new UnitOfWork();

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

		static void _WriteInsertionScriptToFile()
		{
			Console.WriteLine("INSERTING DREAM TERMS SCRAPE 1...");

			string json = System.IO.File.ReadAllText(@"Scrapes/dream-scrape1-take2-formatted.json");
			var obj = JsonConvert.DeserializeObject<IEnumerable<JsonDreamTerm>>(json);

			Console.WriteLine("Scrape loaded, fetching language and admin user...");

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

			Console.WriteLine("Saving to database, building insertion script...");
			_uow.DreamletContext.Database.Log = Console.WriteLine;

			StringBuilder sb = new StringBuilder();

			// NOTE: Term "Armpit" imported with error.
			int iter = 1;

			dreamTerms.ForEach(dt =>
			{
				var term = dt.Term.Replace("\'", "''");

				sb.AppendLine("GO");
				sb.AppendLine($"INSERT INTO DreamTerm (Term, LanguageId) VALUES ('{term}', {dt.LanguageId})");

				if (dt.DreamExplanations.Count > 0)
				{
					sb.AppendLine("DECLARE @dtid int = SCOPE_IDENTITY()");
					sb.AppendLine("INSERT INTO DreamExplanation (Explanation, DreamTermId) VALUES");

					dt.DreamExplanations.ToList().ForEach(de => sb.AppendLine($"('{de.Explanation.Replace("\'", "''")}', @dtid),"));

					// remove trailing commma
					var str = sb.ToString();
					sb = new StringBuilder(str.Remove(str.Length - 3, 1));
					sb.AppendLine($@"PRINT 'TERM: {term} -> NUM: {iter++}/{dreamTerms.Count}'");
					sb.AppendLine("");
				}				
			});

			sb.AppendLine
			(@"
				INSERT INTO DreamTermStatistic (DreamTermId, VisitCount, LikeCount)
					SELECT
						dt.Id,
						0,
						0
						FROM DreamTerm dt
			");

			string script = sb.ToString();
			File.WriteAllText("script.sql", script);
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

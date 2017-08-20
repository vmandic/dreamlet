using dreamlet.DataAccessLayer.Repository;
using dreamlet.DataAccessLayer.UnitOfWork;
using dreamlet.DbEntities.Models;
using dreamlet.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace dreamlet.DatabaseInit
{
  internal class Importer
  {
    // Language cultures
    const string EN_US = "en-US";
    const string HR_HR = "hr-HR";

    // Admin
    const string ADMIN_EMAIL = "admin@dreamlet.me";

    private readonly IRepository<Language> _languages;
    private readonly IRepository<User> _users;
    private readonly IRepository<DreamTerm> _dreamTerms;
    private readonly IUnitOfWork _uow;

    public Importer(IUnitOfWork uow, IRepository<Language> languages, IRepository<User> users, IRepository<DreamTerm> dreamTerms)
    {
      _uow = uow;
      _languages = languages;
      _users = users;
      _dreamTerms = dreamTerms;
    }

    public void TryInsertLanguages()
    {
      Console.WriteLine("INSERTING LANGUAGES...");

      var adminUser = _users.Set.Where(x => x.Email == ADMIN_EMAIL).FirstOrDefault();

      // English en-US
      if (!_languages.Any(x => x.InternationalCode == EN_US))
      {
        var savedEnUs = _languages.CreateAndSaveAsync(new Language()
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
      if (!_languages.Any(x => x.InternationalCode == HR_HR))
      {
        var savedHrHr = _languages.CreateAndSaveAsync(new Language()
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

    public void TryInsertUserAdmin()
    {
      Console.WriteLine("INSERTING USER ADMIN...");

      if (!_users.Any(x => x.Email == ADMIN_EMAIL))
      {
        var saved = _users.CreateAndSaveAsync(new User()
        {
          Email = ADMIN_EMAIL,
          PasswordHash = "0",
          Role = DreamletRole.Admin
        }).Result;

        if (saved)
          Console.WriteLine($"Inserted admin user with id: \"{_users.Set.First().Id}\"");
        else
          throw new Exception("Did not insert admin user. Saving returned false.");
      }
      else
        Console.WriteLine("Admin user already exists, skipping.");
    }

    public void WriteInsertionScriptToFile()
    {
      Console.WriteLine("INSERTING DREAM TERMS SCRAPE 1...");

      string json = File.ReadAllText(@"../Scrapes/scrape1.json");
      var obj = JsonConvert.DeserializeObject<IEnumerable<JsonDreamTerm>>(json);

      Console.WriteLine("Scrape loaded, fetching language and admin user...");

      var engLang = _languages.Find(x => x.InternationalCode == EN_US);
      var adminUser = _users.Find(x => x.Email == ADMIN_EMAIL);

      var dreamTerms = obj.Select(x => new DreamTerm
      {
        LanguageId = engLang.Id,
        Term = x.Name.Replace("|", "\'"), // replace due to import formatting hack
        DreamExplanations = x.Explanations.Select(explanation =>
          new DreamExplanation
          {
            Explanation = explanation.Replace("|", "\'") // replace due to import formatting hack
          }).ToList()
      }).ToList();

      Console.WriteLine("Building insertion SQL script...");
      _uow.Db.Database.Log = Console.WriteLine;

      StringBuilder sb = new StringBuilder();

      sb.AppendLine($"USE {_uow.Db.Database.Connection.Database};" + Environment.NewLine);

      // NOTE: Term "Armpit", on iter == 263 imported with error. Fix manually in script
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

          foreach (var de in dt.DreamExplanations)
            sb.AppendLine($"('{de.Explanation.Replace("\'", "''")}', @dtid),");

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


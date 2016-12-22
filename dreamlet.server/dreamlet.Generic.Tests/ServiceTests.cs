using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.BusinessLogicLayer.Services.Providers;
using dreamlet.DataAccessLayer.MongoDbContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using System.Configuration;

namespace dreamlet.Generic.Tests
{
	[TestClass]
	public class ServiceTests
	{
		IDreamTermsService DreamTermsService { get; set; }

		[TestInitialize]
		public void Setup()
		{
			var dbcs = ConfigurationManager.ConnectionStrings["cs"].ToString();
			var mongoClient = new MongoClient(dbcs);
			var settings = mongoClient.Settings;
			var dbContext = new DreamletMongoContext(settings);

			DreamTermsService = new DreamTermsService();
			DreamTermsService.MongoDatabaseContext = dbContext;
		}

		[TestMethod]
		public void Should_fetch_all_dream_terms()
		{
			var allDreamTerms = DreamTermsService.GetAllDreamTerms();

			Assert.IsNotNull(allDreamTerms);
		}
	}
}

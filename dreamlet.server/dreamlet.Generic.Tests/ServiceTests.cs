using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.BusinessLogicLayer.Services.Providers;
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

		}

		[TestMethod]
		public void Should_fetch_all_dream_terms()
		{
			var allADreamTerms = DreamTermsService.GetLetterGroupDreamTerms('a');

			Assert.IsNotNull(allADreamTerms);
		}
	}
}

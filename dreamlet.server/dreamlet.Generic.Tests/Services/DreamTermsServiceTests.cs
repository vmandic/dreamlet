using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.BusinessLogicLayer.Services.Providers;
using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DataAccessLayer.UnitOfWork;
using dreamlet.Models.Transport.DreamTerms;
using DryIoc;
using DryIoc.MefAttributedModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dreamlet.Generic.Services.Tests
{
	[TestClass]
	public class DreamTermsServiceTests
	{
		private IContainer _container;

		IDreamTermsService DreamTermsService
		{
			get
			{
				return _container.Resolve<IDreamTermsService>();
			}
		}

		[TestInitialize]
		public void Setup()
		{
			_container = new Container().WithMefAttributedModel();

			_container.Register<DreamletEfContext>(Reuse.Singleton);
			_container.Register<IDreamTermsService, DreamTermsService>(Reuse.Singleton);
			_container.Register<IUnitOfWork, UnitOfWork>(Reuse.Singleton);
			_container.Register<RepositoryFactory>(Reuse.Singleton);
		}

		[TestMethod]
		public async Task Should_fetch_all_dream_terms_for_letter_a()
		{
			List<DreamTermModel> allADreamTerms = await DreamTermsService.GetLetterGroupDreamTerms('a');

			// ASSERT
			Assert.IsNotNull(allADreamTerms);
		}

		[TestMethod]
		public async Task Should_fetch_single_dream_term()
		{
			string term = "Car";
			DreamTermModel carDream = await DreamTermsService.GetDreamTerm(term);

			// ASSERT
			Assert.IsNotNull(carDream);
			Assert.AreEqual(term, carDream.Name, true);
		}

		[TestMethod]
		public void Should_fetch_letter_c_search_results()
		{
			string term = "c";
			List<DreamTermModel> dreams = DreamTermsService.FindDreamTerms(term).Result;

			// ASSERT
			Assert.IsNotNull(dreams);
			Assert.IsTrue(dreams.Count > 0);
			Assert.IsFalse(dreams.Any(x => x.Name.ToLower().First() != term.First()));
		}
	}
}

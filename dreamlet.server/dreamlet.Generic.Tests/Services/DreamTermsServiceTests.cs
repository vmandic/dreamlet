using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.BusinessLogicLayer.Services.Providers;
using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DataAccessLayer.UnitOfWork;
using dreamlet.Models.Transport.DreamTerms;
using DryIoc;
using DryIoc.MefAttributedModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
		public async Task Should_fetch_letter_c_search_results()
		{
			string term = "c";
			List<DreamTermModel> dreams = await DreamTermsService.FindDreamTerms(term);

			// ASSERT
			Assert.IsNotNull(dreams);
			Assert.IsTrue(dreams.Count > 0);
			Assert.IsFalse(dreams.Any(x => x.Name.ToLower().First() != term.First()));
		}

		[TestMethod]
		public async Task Should_fetch_dream_term_by_id()
		{
			Random r = new Random();
			int id = r.Next(1, 6305); // at the moment of writing 6305 is the latest sequential id

			// ACT
			DreamTermModel dream = await DreamTermsService.GetDreamTermById(id);

			// ASSERT
			Assert.IsNotNull(dream);
			Assert.AreEqual(id, dream.DreamTermId);

		}

		[TestMethod]
		public async Task Should_fetch_top_liked_dreams()
		{
			List<DreamTermStatisticModel> dreams = await DreamTermsService.GetTopLikedDreamTermsByAccess(Models.AccessFilter.General, 25);

			// ASSERT
			Assert.IsNotNull(dreams);
			Assert.IsTrue(dreams.Count > 0);
			Assert.IsTrue(dreams.Count == 25);
		}

		[TestMethod]
		public async Task Should_fetch_top_read_dreams()
		{
			List<DreamTermStatisticModel> dreams = await DreamTermsService.GetTopReadDreamTerms(25);

			// ASSERT
			Assert.IsNotNull(dreams);
			Assert.IsTrue(dreams.Count > 0);
			Assert.IsTrue(dreams.Count == 25);
		}
	}
}

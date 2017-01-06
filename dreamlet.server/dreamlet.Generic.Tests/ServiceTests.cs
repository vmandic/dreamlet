using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.BusinessLogicLayer.Services.Providers;
using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.Models.Transport.DreamTerms;
using DryIoc;
using DryIoc.MefAttributedModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dreamlet.Generic.Tests
{
	[TestClass]
	public class ServiceTests
	{
		private IContainer _container;

		IDreamTermsService DreamTermsService { get { return _container.Resolve<IDreamTermsService>(); } }

		[TestInitialize]
		public void Setup()
		{
			_container = new Container().WithMefAttributedModel();

			_container.Register<DreamletEfContext>(Reuse.Singleton);
			_container.Register<IDreamTermsService, DreamTermsService>(Reuse.Singleton);
		}

		[TestMethod]
		public async Task Should_fetch_all_dream_terms_for_letter_a()
		{
			List<DreamTermModel> allADreamTerms = await DreamTermsService.GetLetterGroupDreamTerms('a');
			Assert.IsNotNull(allADreamTerms);
		}
	}
}

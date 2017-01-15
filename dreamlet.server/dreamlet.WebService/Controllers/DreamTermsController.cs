using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.Models;
using dreamlet.Models.Transport.Base;
using dreamlet.Models.Transport.DreamTerms;
using dreamlet.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Web.Http;

namespace dreamlet.WebService.Controllers
{
	[RoutePrefix("api/v1/dream-terms")]
	public class DreamTermsController : BaseController
	{
		[Import]
		public Func<IDreamTermsService> DreamTermsService { get; set; }

		public DreamTermsController()
		{
			
		}

		[HttpGet]
		[Route("~/api/v1/dream-terms")]
		public async Task<BaseJsonResponse<List<DreamTermStatisticModel>>> GetTopReadDreamTerms()
			=> BaseJsonResponse.Create(await DreamTermsService().GetTopReadDreamTerms());

		/// <summary>
		/// Returns top 50 liked dream terms for a given access type.
		/// </summary>
		[HttpGet]
		[Route("top/liked/{filterByAccess:int}")]
		public async Task<BaseJsonResponse<List<DreamTermStatisticModel>>> GetTopLikedDreamTermsByAccess(AccessFilter filterByAccess)
			=> BaseJsonResponse.Create(await DreamTermsService().GetTopLikedDreamTermsByAccess(filterByAccess));

		/// <summary>
		/// Returns top 50 read dream terms.
		/// </summary>
		[HttpGet]
		[Route("top/read/{filterByAccess:int}")]
		public async Task<BaseJsonResponse<List<DreamTermStatisticModel>>> GetTopReadDreamTermsByAccess(AccessFilter filterByAccess)
			=> BaseJsonResponse.Create(await DreamTermsService().GetTopReadDreamTermsByAccess(filterByAccess));

		[HttpGet]
		[Route("letter/{letterChar:alpha:maxlength(1)}")]
		public async Task<BaseJsonResponse<List<DreamTermModel>>> GetLetterGroupDreamTerms(string letterChar)
			=> BaseJsonResponse.Create(await DreamTermsService().GetLetterGroupDreamTerms(letterChar[0]));

		[HttpGet]
		[Route("term/{termString}")]
		public async Task<BaseJsonResponse<DreamTermWithExplanationsModel>> GetDreamTerm(string termString)
			=> BaseJsonResponse.Create(await DreamTermsService().GetDreamTerm(termString));

		[HttpGet]
		[Route("search/{searchString}")]
		public async Task<BaseJsonResponse<List<DreamTermModel>>> FindDreamTerms(string searchString)
			=> BaseJsonResponse.Create(await DreamTermsService().FindDreamTerms(searchString));

		[HttpGet]
		[Route("id/{termId}")]
		public async Task<BaseJsonResponse<DreamTermWithExplanationsModel>> GetDreamTermById(string termId)
			=> BaseJsonResponse.Create(await DreamTermsService().GetDreamTermById(DreamletCrypto.DecryptToInt(termId)));
	}
}

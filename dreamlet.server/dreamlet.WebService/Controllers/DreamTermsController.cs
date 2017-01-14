using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.Models.Transport.Base;
using dreamlet.Models.Transport.DreamTerms;
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
		[Route("letter/{letterChar:alpha:maxlength(1)}")]
		public async Task<BaseJsonResponse<List<DreamTermModel>>> GetLetterGroupDreamTerms(string letterChar)
			=> BaseJsonResponse.Create(await DreamTermsService().GetLetterGroupDreamTerms(letterChar[0]));

		[HttpGet]
		[Route("term/{termString}")]
		public async Task<BaseJsonResponse<DreamTermWithExplanationsModel>> GetDreamTerm(string termString)
			=> BaseJsonResponse.Create(await DreamTermsService().GetDreamTerm(termString));

		[HttpGet]
		[Route("term/id/{termId}")]
		public async Task<BaseJsonResponse<DreamTermWithExplanationsModel>> GetDreamTermById(Guid termId)
			=> BaseJsonResponse.Create(await DreamTermsService().GetDreamTermById(termId));
	}
}

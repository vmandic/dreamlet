using dreamlet.BusinessLogicLayer.Services.Interfaces;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Web.Http;

namespace dreamlet.WebService.Controllers
{
	[RoutePrefix("api/v1/dream-terms")]
	public class DreamTermsController : BaseController
	{
		[Import]
		public Func<IDreamTermsService> DreamStoriesService { get; set; }

		public DreamTermsController()
		{
			
		}

		[HttpGet]
		[Route("letter/{letterChar:alpha:maxlength(1)}")]
		public async Task<IHttpActionResult> GetLetterGroupDreamTerms(string letterChar)
		{
			if (DreamStoriesService != null)
				return Ok(await DreamStoriesService().GetLetterGroupDreamTerms(letterChar[0]));

			return Ok("Failed!");
		}
	}
}

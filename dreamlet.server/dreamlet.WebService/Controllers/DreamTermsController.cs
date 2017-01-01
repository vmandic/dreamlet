using dreamlet.BusinessLogicLayer.Services.Interfaces;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Web.Http;

namespace dreamlet.WebService.Controllers
{
    public class DreamTermsController : BaseController
    {
		[Import]
		public Func<IDreamTermsService> DreamStoriesService { get; set; }

		public DreamTermsController()
		{
			
		}

		[HttpGet]
		[Route("letter/{letterChar:alpha:maxlength(1)}")]
		public async Task<IHttpActionResult> GetLetterGroupDreamTerms(char letterChar)
		{
			if (DreamStoriesService != null)
			{
				var allDreamTerms = await DreamStoriesService().GetLetterGroupDreamTerms(letterChar);
				return Ok(allDreamTerms);
			}

			return Ok("failed");
		}
    }
}

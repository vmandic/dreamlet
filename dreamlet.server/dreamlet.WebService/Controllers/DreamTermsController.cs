using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.BusinessLogicLayer.Services.Providers;
using dreamlet.Models.Transport.DreamTerms;
using DryIocAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace dreamlet.WebService.Controllers
{
    public class DreamTermsController : BaseController
    {
		[Import]
		public Func<IDreamStoriesService> DreamStoriesService { get; set; }

		public DreamTermsController()
		{
			
		}

		[HttpGet]
		public IHttpActionResult GetAllDreamTermLetterGroups()
		{
			if (DreamStoriesService != null)
			{
				var allDreamTerms = DreamStoriesService().GetAllDreamTerms();
				return Ok(allDreamTerms);
			}

			return Ok("failed");
		}
    }
}

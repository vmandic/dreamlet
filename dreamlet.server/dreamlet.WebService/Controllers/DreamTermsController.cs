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
		public IDreamStoriesService DreamStoriesService { get; set; }

		public DreamTermsController()
		{
			
		}

		[HttpGet]
		public IHttpActionResult GetAllDreamTermLetterGroups()
		{
			if (DreamStoriesService != null)
			{
				var x = 5;
				//IEnumerable<DreamTermLetterGroup> dreamLetterGroups = DreamStoriesService.GetAllDreamTermLetterGroups();
			}

			return Ok("I am OK");
		}
    }
}

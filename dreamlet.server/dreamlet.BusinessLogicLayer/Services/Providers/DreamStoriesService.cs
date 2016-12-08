using dreamlet.BusinessLogicLayer.Services.Base;
using dreamlet.BusinessLogicLayer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dreamlet.Models.Transport.DreamTerms;
using dreamlet.DataAccessLayer.Entities.Models;
using System.ComponentModel.Composition;

namespace dreamlet.BusinessLogicLayer.Services.Providers
{
	public class DreamStoriesService : BaseService, IDreamStoriesService
    {
        public DreamStoriesService()
        {

        }

		public IEnumerable<DreamTermLetterGroup> GetAllDreamTermLetterGroups()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<DreamTermModel> GetAllDreamTerms()
		{
			return Repository<DreamTerm>().Select(x => new DreamTermModel() {
				DreamTermId = x.Id,
				Name = x.Term
			}).ToList();
		}
	}
}

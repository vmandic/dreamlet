using dreamlet.BusinessLogicLayer.Services.Base;
using dreamlet.BusinessLogicLayer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using dreamlet.Models.Transport.DreamTerms;
using dreamlet.DataAccessLayer.Entities.Models;

namespace dreamlet.BusinessLogicLayer.Services.Providers
{
	public class DreamTermsService : BaseService, IDreamTermsService
    {
        public DreamTermsService()
        {

        }

		private IQueryable<DreamTermLetterGroup> _GetAllDreamTermLetterGroupsQueryable()
			=> Repository<DreamTerm>().Select(x => new DreamTermModel()
				{
					DreamTermId = x.Id,
					Name = x.Term
				}).GroupBy(k => new { Letter = k.Name.First() }).Select(groupedDreamTerms => new DreamTermLetterGroup
				{
					LetterGroup = String.Concat(groupedDreamTerms.Key.Letter),
					DreamTerms = groupedDreamTerms
				});

		public DreamTermLetterGroup GetDreamTermLetterGroup(string letter)
		{
			var result = _GetAllDreamTermLetterGroupsQueryable().FirstOrDefault(x => x.LetterGroup == letter);
			return result;
		}

		public IEnumerable<DreamTermLetterGroup> GetAllDreamTermLetterGroups()
		{
			var result = _GetAllDreamTermLetterGroupsQueryable().ToList();
			return result;
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

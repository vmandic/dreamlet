using dreamlet.BusinessLogicLayer.Services.Base;
using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.DataAccessLayer.Entities.Models;
using dreamlet.Models.Transport.DreamTerms;
using DryIocAttributes;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace dreamlet.BusinessLogicLayer.Services.Providers
{
	[Export(typeof(IDreamTermsService)), WebRequestReuse]
	public class DreamTermsService : BaseService, IDreamTermsService
	{
		public DreamTermsService()
		{

		}

		public Task<DreamTermWithExplanationsModel> GetDreamTerm(string termString)
			=> (from ts in termString.ToLowerInvariant().Trim()
				let lowerTerm = ts.ToString()
				select R<DreamTerm>()
					.Filter(x => x.Term.ToLower().Trim() == lowerTerm)
					.Select(x => new DreamTermWithExplanationsModel
					{
						DreamTermId = x.Id,
						Name = x.Term,
						Explanations = x.DreamExplanations.Select(de => de.Explanation)
					}).FirstOrDefaultAsync()
				).FirstOrDefault();

		public Task<DreamTermWithExplanationsModel> GetDreamTermById(Guid id)
			=> R<DreamTerm>()
			.Filter(x => x.Id == id)
			.Select(x => new DreamTermWithExplanationsModel
			{
				DreamTermId = x.Id,
				Name = x.Term,
				Explanations = x.DreamExplanations.Select(de => de.Explanation)
			}).FirstOrDefaultAsync();
			   
		public Task<List<DreamTermModel>> GetLetterGroupDreamTerms(char letter)
			=> (from c in letter.ToString().ToLowerInvariant()
				let sLetter = c.ToString()
				select R<DreamTerm>()
					.Filter(x => x.Term.ToLower().StartsWith(sLetter))
					.Select(x => new DreamTermModel
					{
					DreamTermId = x.Id,
					Name = x.Term
					}).ToListAsync()
				).FirstOrDefault();
	}
}

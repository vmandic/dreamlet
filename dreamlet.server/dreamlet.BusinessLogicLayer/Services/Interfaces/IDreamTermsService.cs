using System.Collections.Generic;
using dreamlet.Models.Transport.DreamTerms;
using dreamlet.BusinessLogicLayer.Services.Base;
using System.Threading.Tasks;
using System;

namespace dreamlet.BusinessLogicLayer.Services.Interfaces
{
	public interface IDreamTermsService : IBaseService
	{ 
		Task<List<DreamTermModel>> GetLetterGroupDreamTerms(char letter);
		Task<DreamTermWithExplanationsModel> GetDreamTerm(string termString);
		Task<DreamTermWithExplanationsModel> GetDreamTermById(Guid id);
	}
}

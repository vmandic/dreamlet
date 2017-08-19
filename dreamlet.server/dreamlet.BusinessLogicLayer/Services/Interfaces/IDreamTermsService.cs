using dreamlet.BusinessLogicLayer.Services.Base;
using dreamlet.Models.Transport.DreamTerms;
using dreamlet.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dreamlet.BusinessLogicLayer.Services.Interfaces
{
	public interface IDreamTermsService : IBaseService
	{ 
		Task<List<DreamTermModel>> GetLetterGroupDreamTerms(char letter);
		Task<DreamTermWithExplanationsModel> GetDreamTerm(string termString);
		Task<DreamTermWithExplanationsModel> GetDreamTermById(int id);
		Task<List<DreamTermModel>> FindDreamTerms(string searchTerm, int howMany = 10);
		Task<List<DreamTermStatisticModel>> GetTopReadDreamTerms(int howMany = 50);
		Task<List<DreamTermStatisticModel>> GetTopReadDreamTermsByAccess(AccessFilter filterByAccess, int howMany = 50);
		Task<List<DreamTermStatisticModel>> GetTopLikedDreamTermsByAccess(AccessFilter filterByAccess, int howMany = 50);
	}
}

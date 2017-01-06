using System.Collections.Generic;
using dreamlet.Models.Transport.DreamTerms;
using dreamlet.BusinessLogicLayer.Services.Base;
using System.Threading.Tasks;

namespace dreamlet.BusinessLogicLayer.Services.Interfaces
{
	public interface IDreamTermsService : IBaseService
	{ 
		Task<List<DreamTermModel>> GetLetterGroupDreamTerms(char letter);
	}
}

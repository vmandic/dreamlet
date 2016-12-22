using System.Collections.Generic;
using dreamlet.Models.Transport.DreamTerms;
using dreamlet.BusinessLogicLayer.Services.Base;

namespace dreamlet.BusinessLogicLayer.Services.Interfaces
{
	public interface IDreamTermsService : IBaseService
	{
		IEnumerable<DreamTermModel> GetAllDreamTerms();
		IEnumerable<DreamTermLetterGroup> GetAllDreamTermLetterGroups();
		DreamTermLetterGroup GetDreamTermLetterGroup(string letter);
	}
}

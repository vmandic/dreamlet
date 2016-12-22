using System.Collections.Generic;
using dreamlet.Models.Transport.DreamTerms;

namespace dreamlet.BusinessLogicLayer.Services.Interfaces
{
	public interface IDreamTermsService
	{
		IEnumerable<DreamTermModel> GetAllDreamTerms();
		IEnumerable<DreamTermLetterGroup> GetAllDreamTermLetterGroups();
		DreamTermLetterGroup GetDreamTermLetterGroup(string letter);
	}
}

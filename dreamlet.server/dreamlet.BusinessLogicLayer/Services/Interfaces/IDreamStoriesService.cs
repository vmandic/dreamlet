using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dreamlet.Models.Transport.DreamTerms;

namespace dreamlet.BusinessLogicLayer.Services.Interfaces
{
	public interface IDreamStoriesService
	{
		IEnumerable<DreamTermModel> GetAllDreamTerms();
		IEnumerable<DreamTermLetterGroup> GetAllDreamTermLetterGroups();
	}
}

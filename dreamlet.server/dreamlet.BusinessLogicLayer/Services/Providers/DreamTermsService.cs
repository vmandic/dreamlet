using dreamlet.BusinessLogicLayer.Services.Base;
using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.DataAccessLayer.Entities.Models;
using dreamlet.Models.Transport.DreamTerms;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace dreamlet.BusinessLogicLayer.Services.Providers
{
	public class DreamTermsService : BaseService, IDreamTermsService
	{
		public DreamTermsService()
		{

		}

		public Task<List<DreamTermModel>> GetLetterGroupDreamTerms(char letter)
		{
			letter = letter.ToString().ToLowerInvariant()[0];

			return R<DreamTerm>()
				.Filter(x => x.Term.ToLower().First() == letter)
				.Select(x => new DreamTermModel
				{
					DreamTermId = x.Id,
					Name = x.Term
				}).ToListAsync();
		}
	}
}

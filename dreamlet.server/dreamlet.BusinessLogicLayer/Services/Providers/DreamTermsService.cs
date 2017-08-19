using dreamlet.BusinessLogicLayer.Services.Base;
using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.DbEntities.Models;
using dreamlet.Models.Transport.DreamTerms;
using DryIocAttributes;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using dreamlet.Utilities;

namespace dreamlet.BusinessLogicLayer.Services.Providers
{
  [Export(typeof(IDreamTermsService)), WebRequestReuse]
  public class DreamTermsService : BaseService, IDreamTermsService
  {
    public DreamTermsService()
    {

    }

    public Task<List<DreamTermModel>> FindDreamTerms(string searchTerm, int howMany = 10)
      => (from st in searchTerm.ToLowerInvariant().Trim()
          let lowerSearchTerm = st.ToString()
          select R<DreamTerm>()
            .FilterActive(x => x.Term.ToLower().Trim().StartsWith(lowerSearchTerm))
            .OrderByDescending(x => x.Term)
            .Take(howMany)
            .Select(x => new DreamTermModel
            {
              Name = x.Term,
              DreamTermId = x.Id
            }).ToListAsync()
           ).FirstOrDefault();

    public Task<DreamTermWithExplanationsModel> GetDreamTerm(string termString)
      => R<DreamTerm>()
        .FilterActive(x => x.Term.ToLower().Trim() == termString.ToLower().Trim())
        .Select(x => new DreamTermWithExplanationsModel
        {
          DreamTermId = x.Id,
          Name = x.Term,
          Explanations = x.DreamExplanations.Select(de => de.Explanation)
        }).FirstOrDefaultAsync();

    public Task<DreamTermWithExplanationsModel> GetDreamTermById(int id)
      => R<DreamTerm>()
      .FilterActive(x => x.Id == id)
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
            .FilterActive(x => x.Term.ToLower().StartsWith(sLetter))
            .Select(x => new DreamTermModel
            {
              DreamTermId = x.Id,
              Name = x.Term
            }).ToListAsync()
        ).FirstOrDefault();

    public Task<List<DreamTermStatisticModel>> GetTopLikedDreamTermsByAccess(AccessFilter filterByAccess, int howMany = 50)
    {
      Expression<Func<DreamTerm, long>> orderBy = null;

      switch (filterByAccess)
      {
        default:
        // TODO: implement or remove sometime later
        case AccessFilter.Public:
        case AccessFilter.User:
        case AccessFilter.General:
          orderBy = x => x.DreamTermStatistic.LikeCount;
          break;
      }

      return R<DreamTerm>()
          .FilterActive()
          .OrderByDescending(orderBy)
          .Take(howMany)
          .Select(x => new DreamTermStatisticModel
          {
            DreamTermId = x.Id,
            Name = x.Term,
            LikeCount = x.DreamTermStatistic.LikeCount,
            VisitCount = x.DreamTermStatistic.VisitCount
          }).ToListAsync();
    }

    public Task<List<DreamTermStatisticModel>> GetTopReadDreamTerms(int howMany = 50)
      => GetTopReadDreamTermsByAccess(AccessFilter.General, howMany);

    public Task<List<DreamTermStatisticModel>> GetTopReadDreamTermsByAccess(AccessFilter filterByAccess, int howMany = 50)
    {
      Expression<Func<DreamTerm, long>> orderBy = null;

      switch (filterByAccess)
      {
        default:
        // TODO: implement or remove sometime later
        case AccessFilter.Public:
        case AccessFilter.User:
        case AccessFilter.General:
          orderBy = x => x.DreamTermStatistic.VisitCount;
          break;
      }

      return R<DreamTerm>()
          .FilterActive()
          .OrderByDescending(orderBy)
          .Take(howMany)
          .Select(x => new DreamTermStatisticModel
          {
            DreamTermId = x.Id,
            Name = x.Term,
            LikeCount = x.DreamTermStatistic.LikeCount,
            VisitCount = x.DreamTermStatistic.VisitCount
          }).ToListAsync();
    }
  }
}

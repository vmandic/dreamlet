using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.DataAccessLayer.Entities.Base;
using dreamlet.DataAccessLayer.Repository;
using System.Collections.Generic;

namespace dreamlet.BusinessLogicLayer.Services.Base
{
	public abstract class BaseService : IBaseService
	{
		DreamletEfContext _context;
		private static readonly object _locker = new object();
		private Dictionary<string, object> _repositories;
		private RepositoryFactory _factory;

		public RepositoryFactory Factory
		{
			get
			{
				return _factory ?? (_factory = new RepositoryFactory(DreamletContext));
			}
		}

		public DreamletEfContext DreamletContext
		{
			get
			{
				return _context ?? (_context = new DreamletEfContext());
			}

			set
			{
				_context = value;
			}
		}

		public IRepository<TDocument> Repository<TDocument>() where TDocument : class, IBaseEntity => Factory.Get<TDocument>(DreamletContext);
	}
}

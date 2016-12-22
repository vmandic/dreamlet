using dreamlet.DataAccessLayer.MongoDbContext;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DataAccessLayer.Entities.Base;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace dreamlet.BusinessLogicLayer.Services.Base
{
	public abstract class BaseService : IBaseService
	{
		IMongoContext _context;
		private static readonly object _locker = new object();
		private Dictionary<string, object> _repositories;
		private RepositoryFactory _factory;

		public RepositoryFactory Factory
		{
			get
			{
				return _factory ?? (_factory = new RepositoryFactory(MongoDatabaseContext));
			}
		}

		public IMongoContext MongoDatabaseContext
		{
			get
			{
				return _context ?? (_context = new DreamletMongoContext());
			}

			set
			{
				_context = value;
			}
		}

		public IRepository<TDocument> Repository<TDocument>() where TDocument : IBaseMongoEntity
			=> Factory.Get<TDocument>(MongoDatabaseContext);

	}
}

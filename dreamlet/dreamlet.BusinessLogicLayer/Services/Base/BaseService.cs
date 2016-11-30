using dreamlet.DataAccessLayer.MongoDbContext;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DataAccessLayer.Entities.Base;
using System.Collections.Generic;

namespace dreamlet.BusinessLogicLayer.Services.Base
{
    public abstract class BaseService : IBaseService
    {
        IMongoContext _context;
        private static readonly object _locker = new object();
        private Dictionary<string, object> _repositories;

        public IMongoContext MongoDatabaseContext
        {
            get
            {
                return _context ?? (_context = new DreamletMongoContext());
            }
        }

        public IRepository<TDocument> Repository<TDocument>() where TDocument : IBaseMongoEntity
        {
            lock (_locker)
            {
                if (_repositories == null)
                    _repositories = new Dictionary<string, object>();

                var type = typeof(TDocument).Name;

                if (!_repositories.ContainsKey(type))
                    _repositories.Add(type, new GenericMongoRepository<TDocument>(MongoDatabaseContext));

                return (IRepository<TDocument>)_repositories[type];
            }
        }

    }
}

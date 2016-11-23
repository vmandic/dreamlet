using dreamlet.DataAccessLayer.MongoDbContext;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DatabaseEntites.Base;
using System.Collections.Generic;

namespace dreamlet.BusinessLogicLayer.Base
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

        public IRepository<TDocument, string> Repository<TDocument>() where TDocument : IBaseMongoEntity<string>
        {
            lock (_locker)
            {
                if (_repositories == null)
                    _repositories = new Dictionary<string, object>();

                var type = typeof(TDocument).Name;

                if (!_repositories.ContainsKey(type))
                    _repositories.Add(type, new GenericMongoRepository<TDocument, string>(MongoDatabaseContext));

                return (IRepository<TDocument, string>)_repositories[type];
            }
        }

    }
}

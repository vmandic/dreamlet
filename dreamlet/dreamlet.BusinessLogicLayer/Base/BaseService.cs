using dreamlet.DataAccessLayer.MongoDbContext;

namespace dreamlet.BusinessLogicLayer.Base
{
    public abstract class BaseService : IBaseService
    {
        IMongoContext _context;

        public IMongoContext MongoDatabaseContext
        {
            get
            {
                return _context ?? (_context = new DreamletMongoContext());
            }
        }
    }
}

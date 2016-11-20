using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dreamlet.DataAccessLayer.MongoDbContext;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DatabaseEntites.Base;

namespace dreamlet.DataAccessLayer.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private IMongoContext _context;

        public UnitOfWork()
        {
            this._context = new MongoContext();
        }

        public IMongoContext Context
        {
            get
            {
                return _context;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        public IRepository<TDocument, TKey> Repository<TDocument, TKey>() where TDocument : IBaseMongoEntity<TKey>
        {
            throw new NotImplementedException();
        }
    }
}

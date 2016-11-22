using dreamlet.DataAccessLayer.MongoDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.BusinessLogicLayer.Base
{
    public interface IBaseService
    {
        public IMongoContext MongoDatabaseContext { get; }
    }
}

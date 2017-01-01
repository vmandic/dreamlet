using dreamlet.DataAccessLayer.Repository;
using dreamlet.DataAccessLayer.Entities.Base;
using dreamlet.DataAccessLayer.EfDbContext;

namespace dreamlet.BusinessLogicLayer.Services.Base
{
    public interface IBaseService
    {
		DreamletEfContext DreamletContext { get; set; }

        /// <summary>
        /// Resolves a string Id based cached repository instance which exposes the MongoDatabase and MongoCollection.
        /// </summary>
        IRepository<TEntity> Repository<TEntity>() where TEntity : class, IBaseEntity;
    }
}

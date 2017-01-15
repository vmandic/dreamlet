using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.DataAccessLayer.Entities.Base;

namespace dreamlet.DataAccessLayer.Repository
{
	public interface IRepositoryFactory
	{
		IRepository<TEntity> Get<TEntity>() where TEntity : class, IBaseEntity;
	}
}

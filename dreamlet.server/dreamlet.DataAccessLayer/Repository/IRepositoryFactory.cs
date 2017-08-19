using dreamlet.DataAccessLayer.DbContext;
using dreamlet.DbEntities.Base;

namespace dreamlet.DataAccessLayer.Repository
{
	public interface IRepositoryFactory
	{
		IRepository<TEntity> Get<TEntity>() where TEntity : class, IBaseEntity;
	}
}

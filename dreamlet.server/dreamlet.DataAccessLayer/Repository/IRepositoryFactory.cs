using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.DataAccessLayer.Entities.Base;

namespace dreamlet.DataAccessLayer.Repository
{
	interface IRepositoryFactory
	{
		DreamletEfContext DreamletContext { get; set; }

		IRepository<TEntity> Get<TEntity>() where TEntity : class, IBaseEntity;
	}
}

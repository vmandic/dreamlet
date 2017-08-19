using dreamlet.DbEntities.Base;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DataAccessLayer.UnitOfWork;

namespace dreamlet.BusinessLogicLayer.Services.Base
{
	public interface IBaseService
	{
		IUnitOfWork Uow { get; set; }

		/// <summary>
		/// Resolves a string Id based cached repository instance which exposes the MongoDatabase and MongoCollection.
		/// </summary>
		IRepository<TEntity> R<TEntity>() where TEntity : class, IBaseEntity;
	}
}

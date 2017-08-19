using dreamlet.DbEntities.Base;
using dreamlet.DataAccessLayer.Repository;
using dreamlet.DataAccessLayer.UnitOfWork;
using System.ComponentModel.Composition;

namespace dreamlet.BusinessLogicLayer.Services.Base
{
	public abstract class BaseService : IBaseService
	{
		[Import]
		public IUnitOfWork Uow { get; set; }

		[Import]
		private RepositoryFactory Factory { get; set; }

		public BaseService()
		{

		}

		public IRepository<TEntity> R<TEntity>() where TEntity : class, IBaseEntity => Factory.Get<TEntity>();
	}
}

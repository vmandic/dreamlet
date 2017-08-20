using dreamlet.DataAccessLayer.UnitOfWork;
using System.ComponentModel.Composition;

namespace dreamlet.BusinessLogicLayer.Services.Base
{
	public abstract class BaseService : IBaseService
	{
		[Import]
		public IUnitOfWork Uow { get; set; }

		public BaseService() { }
	}
}

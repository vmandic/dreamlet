using dreamlet.DataAccessLayer.UnitOfWork;

namespace dreamlet.BusinessLogicLayer.Services.Base
{
	public interface IBaseService
	{
		IUnitOfWork Uow { get; set; }
	}
}

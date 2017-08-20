using dreamlet.DataAccessLayer.DbContext;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.UnitOfWork
{
	public interface IUnitOfWork
	{
		DreamletDbContext Db { get; set; }

		bool Commit();

		Task<bool> CommitAsync();
	}
}

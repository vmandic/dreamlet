using dreamlet.DataAccessLayer.EfDbContext;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.UnitOfWork
{
	public interface IUnitOfWork
	{
		DreamletEfContext DreamletContext { get; set; }

		bool Commit();

		Task<bool> CommitAsync();
	}
}

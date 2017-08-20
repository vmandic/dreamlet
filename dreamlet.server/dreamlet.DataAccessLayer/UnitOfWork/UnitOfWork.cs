using dreamlet.DataAccessLayer.DbContext;
using DryIocAttributes;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.UnitOfWork
{
	[Export(typeof(IUnitOfWork)), WebRequestReuse]
	public class UnitOfWork : IUnitOfWork
	{
		public UnitOfWork(DreamletDbContext db)
    {
      Db = db;
    }

		public DreamletDbContext Db { get; set; }

		public bool Commit()
		{
			try
			{
				Db.SaveChanges();
				return true;
			}
			catch (Exception)
			{
				// TODO: handle or log ex
				return false;
			}
		}

		public async Task<bool> CommitAsync()
		{
			try
			{
				await Db.SaveChangesAsync();
				return true;
			}
			catch (Exception)
			{
				// TODO: handle or log ex
				return false;
			}
		}
	}
}

using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.DataAccessLayer.Entities.Base;
using dreamlet.DataAccessLayer.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace dreamlet.BusinessLogicLayer.Services.Base
{
	public abstract class BaseService : IBaseService
	{
		DreamletEfContext _context;
		private static readonly object _locker = new object();
		private Dictionary<string, object> _repositories;
		private RepositoryFactory _factory;

		public BaseService()
		{

		}

		public RepositoryFactory Factory
		{
			get
			{
				return _factory ?? (_factory = new RepositoryFactory(DreamletContext));
			}
		}

		[Import]
		public DreamletEfContext DreamletContext
		{
			get
			{
				return _context ?? (_context = new DreamletEfContext());
			}

			set
			{
				_context = value;
			}
		}

		public IRepository<TDocument> R<TDocument>() where TDocument : class, IBaseEntity => Factory.Get<TDocument>(DreamletContext);

		public bool Commit()
		{
			try
			{
				DreamletContext.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				// TODO: handle or log ex
				return false;
			}
		}

		public async Task<bool> CommitAsync()
		{
			try
			{
				await DreamletContext.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				// TODO: handle or log ex
				return false;
			}
		}
	}
}

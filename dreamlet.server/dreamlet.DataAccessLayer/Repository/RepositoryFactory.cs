using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.DataAccessLayer.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.Repository
{
	public class RepositoryFactory : IRepositoryFactory
	{
		private static readonly object _locker = new object();
		private Dictionary<string, object> _repositories;

		[Import]
		public DreamletEfContext DreamletContext { get; set; }

		public RepositoryFactory(DreamletEfContext context = null)
		{
			if (context != null)
				DreamletContext = context;
		}

		public IRepository<TEntity> Get<TEntity>(DreamletEfContext context = null) where TEntity : class, IBaseEntity
		{
			lock (_locker)
			{
				if (_repositories == null)
					_repositories = new Dictionary<string, object>();

				var type = typeof(TEntity).Name;

				if (!_repositories.ContainsKey(type))
					_repositories.Add(type, new Repository<TEntity>(DreamletContext ?? context));

				return (IRepository<TEntity>)_repositories[type];
			}
		}
	}
}

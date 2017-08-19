using dreamlet.DataAccessLayer.DbContext;
using dreamlet.DbEntities.Base;
using DryIocAttributes;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace dreamlet.DataAccessLayer.Repository
{
	[Export, WebRequestReuse]
	public class RepositoryFactory : IRepositoryFactory
	{
		private static readonly object _locker = new object();
		private Dictionary<string, object> _repositories;

		[Import]
		public DreamletDbContext DreamletContext { get; set; }

		public RepositoryFactory(DreamletDbContext context = null)
		{
			if (context != null)
				DreamletContext = context;
		}

		public IRepository<TEntity> Get<TEntity>() where TEntity : class, IBaseEntity
		{
			lock (_locker)
			{
				if (_repositories == null)
					_repositories = new Dictionary<string, object>();

				var type = typeof(TEntity).Name;

				if (!_repositories.ContainsKey(type))
					_repositories.Add(type, new Repository<TEntity>(DreamletContext));

				return (IRepository<TEntity>)_repositories[type];
			}
		}
	}
}

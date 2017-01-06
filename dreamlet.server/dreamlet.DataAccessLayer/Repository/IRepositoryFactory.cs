using dreamlet.DataAccessLayer.EfDbContext;
using dreamlet.DataAccessLayer.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.Repository
{
	interface IRepositoryFactory
	{
		DreamletEfContext DreamletContext { get; set; }

		IRepository<TEntity> Get<TEntity>(DreamletEfContext context = null) where TEntity : class, IBaseEntity;
	}
}

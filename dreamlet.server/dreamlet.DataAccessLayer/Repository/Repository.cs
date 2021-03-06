﻿using dreamlet.DataAccessLayer.DbContext;
using dreamlet.DbEntities.Base;
using dreamlet.Utilities;
using DryIocAttributes;
using EntityFramework.Extensions;
using LinqKit;
using System;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.Repository
{
  [Export(typeof(IRepository<>)), WebRequestReuse]
	public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IBaseEntity
	{
		private static object _locker = new object();

    [Import]
		public DreamletDbContext Db { get; set; }
		public IDbSet<TEntity> Set { get; set; }

		public Repository(DreamletDbContext db)
		{
      Db = db;
			Set = Db.Set<TEntity>();
		}

		public void Dispose()
		{
			if (Db != null)
				Db.Dispose();
		}

		public virtual IQueryable<TEntity> Filter(Expression<Func<TEntity, bool>> predicate)
		{
			return Set.Where(predicate);
		}

		public virtual IQueryable<TEntity> Filter(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
		{
			var query = Set.AsQueryable();

			if (includes != null)
				foreach (var i in includes)
					query = query.Include(i);

			return query.Where(predicate);
		}

		public bool Contains(Expression<Func<TEntity, bool>> predicate)
		{
			return Set.Count(predicate) > 0;
		}

		public virtual TEntity Find(Expression<Func<TEntity, bool>> predicate)
		{
			return Set.FirstOrDefault(predicate);
		}

		public virtual TEntity Create(TEntity TEntity)
		{
			lock (_locker)
			{
				var newEntry = Set.Add(TEntity);
				return newEntry;
			}
		}

		public int UpdateForceAttached(TEntity t)
		{
			var entry = Db.Entry(t);
			entry.State = EntityState.Modified;

			return 0;
		}

		public virtual int Count
		{
			get
			{
				return Set.Count();
			}
		}

		public int Delete(TEntity TEntity)
		{
			Set.Remove(TEntity);

			return 0;
		}

		public virtual int Update(TEntity TEntity)
		{
			if (Set.Find(TEntity.Uid) == null)
				Set.Attach(TEntity);

			var entry = Db.Entry(TEntity);
			entry.State = EntityState.Modified;

			return 0;
		}

		public virtual int Delete(Expression<Func<TEntity, bool>> predicate)
		{
			var objects = Filter(predicate);

			foreach (var obj in objects)
				Set.Remove(obj);

			return 0;
		}

		public virtual async Task<bool> DeleteAsyncAndSave(Expression<Func<TEntity, bool>> predicate)
		{
			try
			{
				var objects = Filter(predicate);

				foreach (var obj in objects)
					Set.Remove(obj);

				await Db.SaveChangesAsync();
				return true;
			}
			catch (Exception)
			{
				// TODO: log ex
				return false;
			}
		}

		public IQueryable<TEntity> Include(params string[] includes)
		{
			var q = Db.Set<TEntity>() as IQueryable<TEntity>;

			foreach (var include in includes)
				q = q.Include(include);

			return q;
		}

		public TEntity Create()
		{
			return Db.Set<TEntity>().Create();
		}

		public void InsertGraph(TEntity entity)
		{
			Set.Add(entity);
		}

		public TEntity FindById(object id, bool reload = false)
		{
			var entity = Set.Find(id);

			if (entity != null && reload)
			{
				var entry = Db.Entry(entity);
				entry.Reload();
			}

			return entity;
		}

		public TEntity FirstOrDefaultByIdAndInclude(Guid id, bool reload, params Expression<Func<TEntity, object>>[] includes)
		{
			var query = Set.AsQueryable();

			if (includes != null)
				foreach (var i in includes)
					query = query.Include(i);

			var entity = query.FirstOrDefault(x => x.Uid == id);

			if (entity != null && reload)
				Db.Entry(entity).Reload();

			return entity;
		}

		public TEntity FirstOrDefaultByIdAndInclude(Expression<Func<TEntity, bool>> predicate, bool reload, params Expression<Func<TEntity, object>>[] includes)
		{
			var query = Set.AsQueryable();

			if (includes != null)
				foreach (var i in includes)
					query = query.Include(i);

			var entity = query.FirstOrDefault(predicate);

			if (entity != null && reload)
				Db.Entry(entity).Reload();

			return entity;
		}

		public bool Any(Expression<Func<TEntity, bool>> predicate)
		{
			lock (_locker)
			{
				return Set.Any(predicate);
			}
		}

		public bool All(Expression<Func<TEntity, bool>> predicate)
		{
			return Set.All(predicate);
		}

		public void Update(TEntity entity, params Expression<Func<TEntity, object>>[] properties)
		{
			var entry = Db.Entry(entity);
			entry.State = EntityState.Unchanged;

			// unwraps the entity properties to only update the sent properties
			foreach (var property in properties)
			{
				string propertyName = property.GetPropertyName();
				entry.Property(propertyName).IsModified = true;
			}
		}

		public IOrderedQueryable<TEntity> FilterOrdered<OrderingType>(Expression<Func<TEntity, OrderingType>> orderBy, Expression<Func<TEntity, bool>> predicate = null, bool isOrderedDescending = false, bool asExpandable = false)
		{
			var query = Set.AsQueryable();

			if (asExpandable)
				query = query.AsExpandable();

			if (predicate != null)
				query = query.Where(predicate);

			return isOrderedDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
		}

		public IQueryable<TEntity> FilterPage(IOrderedQueryable<TEntity> orderedQuery, out int total, int page = 1, int size = 24)
		{
			IQueryable<TEntity> query;

			//If size is -1, return all
			if (size == -1)
			{
				query = orderedQuery;
			}
			else
			{
				int skipCount = (page - 1) * size;
				query = skipCount == 0 ? orderedQuery.Take(size) : orderedQuery.Skip(skipCount).Take(size);
			}

			total = orderedQuery.Count();

			return query;
		}

		public IQueryable<TEntity> FilterOrderedPage<OrderingType>(Expression<Func<TEntity, OrderingType>> orderBy, out int total, int page = 1, int size = 24, bool isOrderedDescending = false, Expression<Func<TEntity, bool>> predicate = null, bool asExpandable = false)
		{
			return FilterPage(FilterOrdered(orderBy, predicate, isOrderedDescending, asExpandable), out total, page, size);
		}

		public IQueryable<TEntity> FilterOrderedWithSkip<OrderingType>(Expression<Func<TEntity, OrderingType>> orderBy, out int total, out int totalFiltered, int skip = 1, int size = 24, bool isOrderedDescending = false, Expression<Func<TEntity, bool>> accountPredicate = null, Expression<Func<TEntity, bool>> searchPredicate = null, bool asExpandable = false)
		{
			var query = Set.AsQueryable();

			if (asExpandable)
				query = query.AsExpandable();

			if (accountPredicate != null)
				query = query.Where(accountPredicate);

			var _total = query.FutureCount();

			if (searchPredicate != null)
				query = query.Where(searchPredicate);

			var _totalFiltered = query.FutureCount();

			total = _total;
			totalFiltered = _totalFiltered;

			var orderedQuery = isOrderedDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

			// if size is -1, return all
			if (size == -1)
				return orderedQuery;
			else
				return skip == 0 ? orderedQuery.Take(size) : orderedQuery.Skip(skip).Take(size);
		}

		public IQueryable<TEntity> FilterOrderedPageQueryable<OrderingType>(IQueryable<TEntity> query, Expression<Func<TEntity, OrderingType>> orderBy, out int total, int page = 1, int size = 24, bool isOrderedDescending = false, Expression<Func<TEntity, bool>> predicate = null)
		{
			if (predicate != null)
				query = query.Where(predicate);

			var orderedQuery = isOrderedDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

			//If size is -1, return all
			if (size == -1)
			{
				query = orderedQuery;
			}
			else
			{
				int skipCount = (page - 1) * size;
				query = skipCount == 0 ? orderedQuery.Take(size) : orderedQuery.Skip(skipCount).Take(size);
			}
			total = orderedQuery.Count();

			return query;
		}

		public async Task<TEntity> FirstOrDefaultAndIncludeAsync(Expression<Func<TEntity, bool>> predicate, bool reload = false, params Expression<Func<TEntity, object>>[] includes)
		{
			var query = Set.AsQueryable();

			if (includes != null)
				foreach (var i in includes)
					query = query.Include(i);

			var entity = await query.FirstOrDefaultAsync(predicate);

			if (reload)
				Db.Entry(entity).Reload();

			return entity;
		}

		public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
		{
			return await Set.AnyAsync(predicate);
		}
		public virtual async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate)
		{
			return await Set.FirstOrDefaultAsync(predicate);
		}

		public async Task<bool> CreateAndSaveAsync(TEntity entity)
		{
			try
			{
				Create(entity);
				await Db.SaveChangesAsync();

				return true;
			}
			catch (Exception)
			{
				// TODO: log ex
				return false;
			}
		}

		public async Task<bool> UpdateAndSaveAsync(TEntity entity, params Expression<Func<TEntity, object>>[] properties)
		{
			try
			{
				Update(entity, properties);
				await Db.SaveChangesAsync();
				return true;
			}
			catch (Exception)
			{
				// TODO: log ex
				return false;
			}
		}

		public async Task<TEntity> FirstOrDefaultByIdAndIncludeAsync(Guid id, bool reload, params Expression<Func<TEntity, object>>[] includes)
		{
			var query = Set.AsQueryable();

			if (includes != null)
				foreach (var i in includes)
					query = query.Include(i);

			var entity = await query.FirstOrDefaultAsync(x => x.Uid == id);

			if (entity != null && reload)
				await Db.Entry(entity).ReloadAsync();

			return entity;
		}

		public async Task<TEntity> FirstOrDefaultByIdAsNoTrackingAndIncludeAsync(Guid id, bool reload, params Expression<Func<TEntity, object>>[] includes)
		{
			var query = Set.AsNoTracking();

			if (includes != null)
				foreach (var i in includes)
					query = query.Include(i);

			var entity = await query.FirstOrDefaultAsync(x => x.Uid == id);

			if (entity != null && reload)
				await Db.Entry(entity).ReloadAsync();

			return entity;
		}

		public IQueryable<TEntity> FilterActive(Expression<Func<TEntity, bool>> predicate = null)
			=> predicate == null ? Filter(x => x.ActiveState == ActiveState.Active) : Filter(x => x.ActiveState == ActiveState.Active).Where(predicate);
	}
}

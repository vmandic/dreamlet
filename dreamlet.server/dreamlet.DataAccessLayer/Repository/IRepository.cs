using dreamlet.DataAccessLayer.DbContext;
using dreamlet.DbEntities.Base;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.Repository
{
	public interface IRepository<TEntity> : IDisposable where TEntity : class, IBaseEntity
	{
		IDbSet<TEntity> Set { get; set; }

		bool HasAny(Expression<Func<TEntity, bool>> predicate);

		bool HasAll(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		/// Adds the entity to the current graph context.
		/// </summary>
		void InsertGraph(TEntity entity);

		/// <summary>
		/// Gets all objects from database with includes
		/// </summary>
		IQueryable<TEntity> Include(params string[] includes);

		/// <summary>
		/// Gets objects from database by filter.
		/// </summary>
		/// <param name="predicate">Filter entites by a lookup expression.</param>
		IQueryable<TEntity> Filter(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		/// Gets ActiveStatus = 1 TEntity objects by combining with the lookup predicate.
		/// </summary>
		/// <param name="predicate">Filter entites by a lookup expression.</param>
		IQueryable<TEntity> FilterActive(Expression<Func<TEntity, bool>> predicate = null);

		/// <summary>
		/// Gets objects from database by filter with related tables.
		/// </summary>
		/// <param name="predicate">Filter entites by a lookup expression.</param>
		/// <param name="includes">Specified related entites</param>
		IQueryable<TEntity> Filter(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);

		/// <summary>
		/// Gets objects ordered by a sort selector with desired entites included.
		/// </summary>
		/// <param name="predicate">Filter entites by a lookup expression.</param>
		/// <param name="orederBy">Specified a lambda sort property</param>
		/// <param name="total">Returns the total records count of the filter</param>
		/// <param name="index">Specified the page index.</param>
		/// <param name="size">Specified the page size</param>
		IOrderedQueryable<TEntity> FilterOrdered<OrderingType>(Expression<Func<TEntity, OrderingType>> orderBy, Expression<Func<TEntity, bool>> predicate = null, bool isOrderedDescending = false, bool asExpandable = false);

		IQueryable<TEntity> FilterOrderedPage<OrderingType>(Expression<Func<TEntity, OrderingType>> orderBy, out int total, int page = 1, int size = 24, bool isOrderedDescending = false, Expression<Func<TEntity, bool>> predicate = null, bool asExpandable = false);

		IQueryable<TEntity> FilterOrderedWithSkip<OrderingType>(Expression<Func<TEntity, OrderingType>> orderBy, out int total, out int totalFiltered, int skip = 1, int size = 24, bool isOrderedDescending = false, Expression<Func<TEntity, bool>> accountPredicate = null, Expression<Func<TEntity, bool>> searchPredicate = null, bool asExpandable = false);

		/// <summary>
		/// Gets objects ordered by a sort selector and selects a targeted number of entites for a targeted page.
		/// </summary>
		/// <param name="orderedQuery">An ordered query to query upon</param>
		/// <param name="page">The number of page to get entites for</param>
		/// <param name="total">Returns the total records count of the filter</param>
		/// <param name="size">Specified the page size</param>
		IQueryable<TEntity> FilterPage(IOrderedQueryable<TEntity> orderedQuery, out int total, int page = 1, int size = 24);

		/// <summary>
		/// Gets the object(s) is exists in database by specified filter.
		/// </summary>
		/// <param name="predicate">Specified the filter expression</param>
		bool Contains(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		/// Find object by specified expression.
		/// </summary>
		/// <param name="predicate"></param>
		TEntity Find(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		/// Find object by specified id.
		/// </summary>
		/// <param name="id">The primary key value of the current entity.</param>
		TEntity FindById(object id, bool reload = false);

		/// <summary>
		/// Finds an entity in the current DbSet with the included related entites.
		/// </summary>
		/// <param name="id">The int Primary Key ID column property value</param>
		/// <param name="includes">Lambda expression references to related entites to be included.</param>
		/// <returns>A found entity from the DB or a null.</returns>
		/// <remarks>To enable searching by ID, the Repository implementation will expect to have Entity objects implemnting the IBaseEntity with the int ID property.</remarks>
		TEntity FirstOrDefaultByIdAndInclude(Guid id, bool reload, params Expression<Func<TEntity, object>>[] includes);

		Task<TEntity> FirstOrDefaultByIdAndIncludeAsync(Guid id, bool reload, params Expression<Func<TEntity, object>>[] includes);

		Task<TEntity> FirstOrDefaultByIdAsNoTrackingAndIncludeAsync(Guid id, bool reload, params Expression<Func<TEntity, object>>[] includes);

		/// <summary>
		/// Finds an entity in the current DbSet with the included related entites.
		/// </summary>
		/// <param name="predicate">A lambda expression predicate which defines the lookup query, i.e. where clause.</param>
		/// <param name="includes">Lambda expression references to related entites to be included.</param>
		/// <returns>A found entity from the DB or a null.</returns>
		TEntity FirstOrDefaultByIdAndInclude(Expression<Func<TEntity, bool>> predicate, bool reload, params Expression<Func<TEntity, object>>[] includes);

		/// <summary>
		/// Create a new object to database.
		/// </summary>
		/// <param name="t">Specified a new object to create.</param>
		TEntity Create(TEntity t);

		/// <summary>
		/// Creates an empty instance of the given entity in the graph.
		/// </summary>
		TEntity Create();

		/// <summary>
		/// Creates a new entity for the database and try to immediatelly persist the change.
		/// </summary>
		/// <returns></returns>
		Task<bool> CreateAndSaveAsync(TEntity t);

		/// <summary>
		/// Updates the entity and tries to immediatelly persist the change in the database.
		/// </summary>
		Task<bool> UpdateAndSaveAsync(TEntity entity, params Expression<Func<TEntity, object>>[] properties);

		/// <summary>
		/// Delete the object from database.
		/// </summary>
		/// <param name="t">Specified a existing object to delete.</param>        
		int Delete(TEntity t);

		/// <summary>
		/// Delete objects from database by specified filter expression.
		/// </summary>
		/// <param name="predicate"></param>
		int Delete(Expression<Func<TEntity, bool>> predicate);

		Task<bool> DeleteAsyncAndSave(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		/// Update object changes and save to database.
		/// </summary>
		/// <param name="t">Specified the object to save.</param>
		int Update(TEntity t);

		int UpdateForceAttached(TEntity t);

		/// <summary>
		/// Get the total objects count.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Generates a single update clause for the database instead of the standard fetch and update method of EntityFramework.
		/// </summary>
		/// <param name="entity">A in memory entity object with the ID value set up.</param>
		/// <param name="properties">Lambda expression params array of properties to be updated.</param>
		void Update(TEntity entity, params Expression<Func<TEntity, object>>[] properties);

		IQueryable<TEntity> FilterOrderedPageQueryable<OrderingType>(IQueryable<TEntity> query, Expression<Func<TEntity, OrderingType>> orderBy, out int total, int page = 1, int size = 24, bool isOrderedDescending = false, Expression<Func<TEntity, bool>> predicate = null);

		Task<TEntity> FirstOrDefaultAndIncludeAsync(Expression<Func<TEntity, bool>> predicate, bool reload = false, params Expression<Func<TEntity, object>>[] includes);
		Task<bool> HasAnyAsync(Expression<Func<TEntity, bool>> predicate);
		/// <summary>
		/// Find object by specified expression async.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate);
	}
}

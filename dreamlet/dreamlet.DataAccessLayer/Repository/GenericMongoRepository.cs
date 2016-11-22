using dreamlet.DataAccessLayer.MongoDbContext;
using dreamlet.DatabaseEntites.Base;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace dreamlet.DataAccessLayer.Repository
{
    public class GenericMongoRepository<TDocument, TKey> : IRepository<TDocument, TKey> where TDocument : IBaseMongoEntity<TKey>
    {
        /// <summary>
        /// MongoCollection field.
        /// </summary>
        private IMongoCollection<TDocument> _collection;
        private bool _Compare(TKey a, TKey b) => EqualityComparer<TKey>.Default.Equals(a, b);

        public GenericMongoRepository(IMongoContext context)
        {
            this._collection = context.Collection<TDocument>();
        }

        /// <summary>
        /// Gets the Mongo collection (to perform advanced operations).
        /// </summary>
        /// <remarks>
        /// One can argue that exposing this property (and with that, access to it's Database property for instance
        /// (which is a "parent")) is not the responsibility of this class. Use of this property is highly discouraged;
        /// for most purposes you can use the GenericMongoRepositoryManager&lt;T&gt;
        /// </remarks>
        /// <value>The Mongo collection (to perform advanced operations).</value>
        public IMongoCollection<TDocument> Collection
        {
            get { return this._collection; }
        }

        /// <summary>
        /// Returns the T by its given id.
        /// </summary>
        /// <param name="id">The Id of the entity to retrieve.</param>
        /// <returns>The Entity T.</returns>
        public virtual TDocument GetById(TKey id)
            => this._collection.Find(x => _Compare(x.Id, id)).FirstOrDefault();
        
        /// <summary>
        /// Adds the new entity in the repository.
        /// </summary>
        /// <param name="entity">The entity T.</param>
        /// <returns>The added entity including its new ObjectId.</returns>
        public virtual TDocument Add(TDocument entity)
        {
            this._collection.InsertOne(entity);
            return entity;
        }

        /// <summary>
        /// Adds the new entities in the repository.
        /// </summary>
        /// <param name="entities">The entities of type T.</param>
        public virtual void Add(IEnumerable<TDocument> entities)
            => this._collection.InsertMany(entities);

        /// <summary>
        /// Upserts an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The updated entity.</returns>
        public virtual TDocument Update(TDocument entity)
        {
            var result = this._collection.ReplaceOne(x => _Compare(x.Id, entity.Id), entity);
            return entity;
        }

        /// <summary>
        /// Upserts the entities.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        public virtual void Update(IEnumerable<TDocument> entities)
            => entities.ToList().ForEach(entity => this.Update(entity));

        /// <summary>
        /// Deletes an entity from the repository by its id.
        /// </summary>
        /// <param name="id">The entity's id.</param>
        public virtual void Delete(TKey id)
            => this._collection.DeleteOne(x => _Compare(x.Id, id));

        /// <summary>
        /// Deletes the given entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public virtual void Delete(TDocument entity)
            => this.Delete(entity.Id);

        /// <summary>
        /// Deletes all entities in the repository.
        /// </summary>
        public virtual void DeleteAll()
            => this._collection.DeleteMany(new BsonDocument());

        /// <summary>
        /// Counts the total entities in the repository.
        /// </summary>
        /// <returns>Count of entities in the collection.</returns>
        public virtual long Count()
            => this._collection.Count(new BsonDocument());

        /// <summary>
        /// Checks if the entity exists for given predicate.
        /// </summary>
        /// <param name="predicate">The expression.</param>
        /// <returns>True when an entity matching the predicate exists, false otherwise.</returns>
        public virtual bool Exists(Expression<Func<TDocument, bool>> predicate)
            => this._collection.AsQueryable().Any(predicate);

        #region IQueryable<T>
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator&lt;T&gt; object that can be used to iterate through the collection.</returns>
        public virtual IEnumerator<TDocument> GetEnumerator()
        {
            return this._collection.AsQueryable().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._collection.AsQueryable().GetEnumerator();
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of IQueryable is executed.
        /// </summary>
        public virtual Type ElementType
        {
            get { return this._collection.AsQueryable().ElementType; }
        }

        /// <summary>
        /// Gets the expression tree that is associated with the instance of IQueryable.
        /// </summary>
        public virtual Expression Expression
        {
            get { return this._collection.AsQueryable().Expression; }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        public virtual IQueryProvider Provider
        {
            get { return this._collection.AsQueryable().Provider; }
        }
        #endregion
    }
}

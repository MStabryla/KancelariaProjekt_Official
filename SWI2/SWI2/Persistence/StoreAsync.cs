using Microsoft.EntityFrameworkCore;
using SWI2DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Persistence
{

    public class StoreAsync<T> : IStoreAsync<T> where T : BaseModel
    {
        #region Fields

        private readonly SWIDbContext _context;
        private DbSet<T> _entities;

        #endregion

        #region (Constru)Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="context">Object context</param>
        public StoreAsync(SWIDbContext context)
        {
            this._context = context;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        public virtual async Task<T> GetById(object id)
        {
            var ent = await Entities.FindAsync(id);
            if (ent != null)
            {
                foreach (var collection in _context.Entry(ent).Collections)
                {
                    await collection.LoadAsync();
                }
            }
            return ent;
        }

        /// <summary>
        /// Get IQueryable <typeparamref name="T"/>  object to query data from.
        /// </summary>
        /// <typeparam name="T">A type of Entity.</typeparam>
        /// <returns>IQueryable</returns>
        public virtual IQueryable<T> AsQueryable()
        {
            return Entities;
        }


        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual async Task Insert(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                Entities.Add(entity);

                await _context.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                throw new Exception("Wystąpił błąd przy wstawianiu wiersza do bazy danych!", dbEx);
            }
        }

        /// <summary>
        /// Insert entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual async Task Insert(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException("entities");

                foreach (var entity in entities)
                {
                    this.Entities.Add(entity);
                }

                await this._context.SaveChangesAsync();

            }
            catch (Exception dbEx)
            {
                throw new Exception("Wystąpił błąd przy wstawianiu wierszy do bazy danych!", dbEx);
            }
        }

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual async Task Update(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                await this._context.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                throw new Exception("Wystąpił błąd przy aktualizacji wiersza!", dbEx);
            }
        }

        /// <summary>
        /// Update entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual async Task Update(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException("entities");

                await this._context.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                throw new Exception("Wystąpił błąd przy aktualizacji wierszy!", dbEx);
            }
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual async Task Delete(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                this.Entities.Remove(entity);

                await this._context.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                throw new Exception("Wystąpił błąd przy usuwaniu wiersza!", dbEx);
            }
        }

        /// <summary>
        /// Delete entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual async Task Delete(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException("entities");

                foreach (var entity in entities)
                    this.Entities.Remove(entity);

                await this._context.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                throw new Exception("Wystąpił błąd przy usuwaniu wierszy!", dbEx);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IQueryable<T> Table
        {
            get
            {
                return this.Entities;
            }
        }

        /// <summary>
        /// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        public virtual IQueryable<T> TableNoTracking
        {
            get
            {
                return this.Entities.AsNoTracking();
            }
        }

        /// <summary>
        /// Entities
        /// </summary>
        protected virtual DbSet<T> Entities
        {
            get
            {
                if (_entities == null)
                    _entities = _context.Set<T>();
                return _entities;
            }
        }

        public SWIDbContext Context
        {
            get
            {
                return _context;
            }
        }

        #endregion
    }
}

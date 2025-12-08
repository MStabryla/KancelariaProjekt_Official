using SWI2DB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Persistence
{
    public interface IStoreAsync<T> where T : BaseModel
    {



        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        Task<T> GetById(object id);

        /// <summary>
        /// Get IQueryable <typeparamref name="T"/>  object to query data from.
        /// </summary>
        /// <typeparam name="T">A type of Entity.</typeparam>
        /// <returns>IQueryable</returns>
        IQueryable<T> AsQueryable();
        //  PagedResult<T> GetPaged(int v1, int v2);//where T : class;

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        Task Insert(T entity);

        /// <summary>
        /// Insert entities
        /// </summary>
        /// <param name="entities">Entities</param>
        Task Insert(IEnumerable<T> entities);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        Task Update(T entity);

        /// <summary>
        /// Update entities
        /// </summary>
        /// <param name="entities">Entities</param>
        Task Update(IEnumerable<T> entities);

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        Task Delete(T entity);

        /// <summary>
        /// Delete entities
        /// </summary>
        /// <param name="entities">Entities</param>
        Task Delete(IEnumerable<T> entities);

        /// <summary>
        /// Gets a table
        /// </summary>
        IQueryable<T> Table { get; }

        /// <summary>
        /// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        IQueryable<T> TableNoTracking { get; }

        SWIDbContext Context { get; }


    }
}
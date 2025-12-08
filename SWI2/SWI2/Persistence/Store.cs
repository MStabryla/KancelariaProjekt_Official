using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SWI2DB;
using SWI2DB.Models.Entries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SWI2.Persistence
{

    public class Store<T> : IStore<T> where T : BaseModel
    {
        #region Fields

        private readonly SWIDbContext _context;
        private DbSet<T> _entities;
        private readonly ILogger<Store<T>> _logger;

        #endregion

        #region (Constru)Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="context">Object context</param>
        public Store(SWIDbContext context,
          ILogger<Store<T>> logger)
        {
            this._context = context;
            this._logger = logger;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        public virtual T GetById(object id)
        {
            var ent = this.Entities.Find(id);
            if (ent != null)
            {
                foreach (var collection in _context.Entry(ent).Collections)
                {
                    collection.Load();
                }
                foreach (var reference in _context.Entry(ent).References)
                {
                    reference.Load();
                }
            }
            return ent;
        }

        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        public async virtual Task<T> GetByIdAsync(object id)
        {
            var ent = await this.Entities.FindAsync(id);
            if (ent != null)
            {
                foreach (var collection in _context.Entry(ent).Collections)
                {
                    await collection.LoadAsync();
                }
                foreach (var reference in _context.Entry(ent).References)
                {
                    await reference.LoadAsync();
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
            return this.Entities;
        }


        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual async Task<bool> InsertAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                await this.Entities.AddAsync(entity);

                this._context.SaveChanges();

                return true;

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
        public virtual async Task<bool> InsertAsync(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException("entities");

                foreach (var entity in entities)
                {
                    await this.Entities.AddAsync(entity);
                }

                this._context.SaveChanges();
                return true;
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
        public virtual async Task<T> Update(T entity, JObject changedData)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");
                entity = PartialUpdate(entity, changedData);
                await this._context.SaveChangesAsync();
                return entity;
            }
            catch (Exception dbEx)
            {
                throw new Exception("Wystąpił błąd przy aktualizacji wiersza!", dbEx);
            }
        }
        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual async Task<T> Update(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                await this._context.SaveChangesAsync();

                return entity;
            }
            catch (Exception dbEx)
            {
                throw new Exception("Wystąpił błąd przy aktualizacji wiersza!", dbEx);
            }
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual bool Delete(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                this.Entities.Remove(entity);

                this._context.SaveChanges();
                return true;
            }
            catch (Exception dbEx)
            {
                throw new Exception("Wystąpił błąd przy usuwaniu wiersza!", dbEx);
            }
        }
        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual async Task DeleteAsync(T entity)
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
        public virtual bool Delete(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException("entities");

                this.Entities.RemoveRange(entities);


                this._context.SaveChanges();
                return true;
            }
            catch (Exception dbEx)
            {
                throw new Exception("Wystąpił błąd przy usuwaniu wierszy!", dbEx);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task DeleteAsync(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException("entities");

                this.Entities.RemoveRange(entities);


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

        #region Helpers
        private T PartialUpdate<T>(T entity, JObject changedData) where T : class
        {
            foreach (JProperty propertyToChange in changedData.Properties())
            {
                PropertyInfo propertyInfo = entity.GetType().GetProperty(propertyToChange.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null && propertyInfo.Name != "Id")
                {
                    if (propertyToChange.Value.Type == JTokenType.Object)
                    {
                        var entityPart = propertyInfo.GetValue(entity);
                        if (propertyToChange.Value.Count() == 1)
                        {
                            entityPart = _context.Find(propertyInfo.PropertyType, propertyToChange.Value.First.First.ToObject<long>());
                            _logger.LogInformation(new EventId(200, "PartialUpdate"), "object of type" + entityPart.GetType().ToString() + " found: " + entityPart.GetType().GetProperty("Id"));

                        }
                        else
                        {
                            if (propertyInfo.PropertyType.GetProperty("Id").GetValue(entityPart).ToString() == propertyToChange.Select(p => p["id"]).First().ToObject<string>())
                            {
                                var ifCreatedProperty = entityPart.GetType().GetProperty("Updated");
                                if (ifCreatedProperty != null)
                                {
                                    ifCreatedProperty.SetValue(entityPart, DateTime.Now);
                                }
                                entityPart = PartialUpdate(entityPart, (JObject)propertyToChange.Value);
                            }
                            else if (propertyToChange.Select(p => p["id"]).First().ToObject<string>() == "0")
                            {
                                entityPart = Activator.CreateInstance(propertyInfo.PropertyType);
                                var ifCreatedProperty = entityPart.GetType().GetProperty("Created");
                                if (ifCreatedProperty != null)
                                {
                                    ifCreatedProperty.SetValue(entityPart,DateTime.Now);
                                }
                                entityPart = PartialUpdate(entityPart, (JObject)propertyToChange.Value);
                            }
                            /*                            else
                                                        {
                                                            entityPart = _context.Find(entityType, propertyToChange.Select(p => p["id"]));
                                                        }*/
                        }
                        propertyInfo.SetValue(entity, entityPart, null);
                    }
                    else if (propertyToChange.Value.Type == JTokenType.Array)
                    {

                        var entityList = (IList)propertyInfo.GetValue(entity);
                        Type entityType = entityList.GetType().GetGenericArguments()[0];
                        MethodInfo elementAtMethod = typeof(System.Linq.Enumerable).GetMethod("ElementAtOrDefault").MakeGenericMethod(entityType);
                        List<int> elementIndexesToDelate = new List<int>();
                        var counter = 0;
                        foreach (var ptc in propertyToChange.Value)
                        {
                            if (ptc.Count() != 0)
                            {
                                object entityPart = elementAtMethod.Invoke(entityList, new object[] { entityList, counter });
                                if (entityPart != null)
                                {

                                    if (ptc.Count() > 1)
                                    {
                                        entityList[counter] = PartialUpdate(entityPart, (JObject)ptc);
                                    }
                                    else
                                    {
                                        if ((long)entityPart.GetType().GetProperty("Id").GetValue(entityPart) == ptc["id"].ToObject<long>())
                                        {
                                            elementIndexesToDelate.Add(counter);
                                        }
                                        else
                                        {
                                            entityList[counter] = _context.Find(entityType, ptc["id"].ToObject<long>());
                                        _logger.LogInformation(new EventId(200, "PartialUpdate"), "object of type"+ entityPart.GetType().ToString() + " found: "+ entityPart.GetType().GetProperty("Id"));
                                        }
                                    }
                                }
                                else
                                {
                                    if (ptc.Count() > 1)
                                    {
                                        entityPart = Activator.CreateInstance(entityType);
                                        entityPart = PartialUpdate(entityPart, (JObject)ptc);
                                    }
                                    else
                                    {
                                        entityPart = _context.Find(entityType, ptc["id"].ToObject<long>());
                                        _logger.LogInformation(new EventId(200, "PartialUpdate"), "object of type"+ entityPart.GetType().ToString() + " found: "+ entityPart.GetType().GetProperty("Id"));

                                    }
                                    entityList.Add(entityPart);
                                }
                            }
                            counter++;
                        }
                        elementIndexesToDelate.Sort((a, b) => b.CompareTo(a));
                        foreach (int index in elementIndexesToDelate)
                        {
                            entityList.RemoveAt(index);
                        }
                        propertyInfo.SetValue(entity, entityList);
                    }
                    else
                    {
                        var type = propertyInfo.PropertyType;
                        propertyInfo.SetValue(entity, ((JValue)propertyToChange.Value).ToObject(type), null);
                    }
                }
                else
                {

                    _logger.LogError(new EventId(200, "PartialUpdate"), "trying to update with wron property name :" + propertyInfo != null ? propertyInfo.Name : "(null property Info)");
                }
            }
            return entity;
        }
        #endregion
    }
}
using KidsMealApi.DataAccess.Interfaces;

namespace KidsMealApi.DataAccess
{
    public abstract class BaseDataAccessService<T> where T : class
    {
        protected readonly KidsMealDbContext _dbContext;

        public BaseDataAccessService(KidsMealDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Abstract Methods
        protected abstract IQueryable<T> GetAll();
        #endregion

        /// <summary>
        /// Creates a new entity wihtin the data source
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>For IUniqueEntity types, we return a refreshed entity from the database otherwise the entity passed in is returned.</returns>
        protected async Task<T> CreateAsync (T entity, bool saveChanges = true)
        {
            await _dbContext.AddAsync<T>(entity);
            if (saveChanges)
                await _dbContext.SaveChangesAsync();
            else
                return entity; //entity is tracked by ef but wont have an id to look-up by yet

            if (entity is IUniqueEntity)
                return await GetByIdAsync(((IUniqueEntity)entity).Id);
                
            return entity;
        }

        /// <summary>
        /// Retrieves an entity by it's unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        protected async Task<T> GetByIdAsync (int id)
        {
            if (!typeof(T).GetInterfaces().Contains(typeof(IUniqueEntity)))
                throw new NotSupportedException($"The entity {typeof(T)} does not support look-ups by id.");

            var existingEntiy = await _dbContext.FindAsync<T>(id);
            if (existingEntiy == null)
                throw new KeyNotFoundException($"Unable to find an entity '{typeof(T)}' by {id}");
            
            return existingEntiy;
        }

        /// <summary>
        /// Updates a given entity
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="applyAnyUpdates">The action delegate/pointer to the method which will update the entity</param>
        /// <param name="saveChanges">A flag to indicate it the changes should be saved immediately or not</param>
        /// <returns>The entity will all the updates applied</returns>
        protected async Task<T> UpdateAsync (T entity, Action<T> applyAnyUpdates, bool saveChanges = true)
        {
            //Attach the entity to DbContext with initial state of 'UnChanged'.
            //This will prevent EF from trying to insert values for every single property when changes are applied to the entity
            //Especially the exception below:
            //System.InvalidCastException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported.
            _dbContext.Attach<T>(entity);

            //Apply necessary updates to attached entity 
            applyAnyUpdates(entity);
            if (saveChanges)
                await _dbContext.SaveChangesAsync();

            return entity;
        }

        protected async Task<T> UpdateAsync (int id, Action<T> applyAnyUpdates, bool saveChanges = true)
        {
            if (id <= 0)
                throw new InvalidOperationException("Updates can only be performed on existing entities.");

            var entity = await GetByIdAsync(id);
            return await UpdateAsync(entity, applyAnyUpdates, saveChanges);
        }
        
        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="saveChanges"></param>
        /// <returns></returns>
        protected async Task DeleteAsync(T entity, bool saveChanges = true)
        {
            _dbContext.Remove<T>(entity);
            if (saveChanges)
                await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Deletes an entity using it's unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected async Task DeleteByIdAsync(int id)
        {
            var existingEntity = await GetByIdAsync(id);
            await DeleteAsync(existingEntity);
        }

    }
}
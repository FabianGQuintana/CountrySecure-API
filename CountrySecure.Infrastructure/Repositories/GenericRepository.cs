using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly CountrySecureDbContext Db;

        public GenericRepository(CountrySecureDbContext dbContext)
        {
            Db = dbContext;
        }

        protected DbSet<T> Set => Db.Set<T>();

        public virtual Task<T?> GetByIdAsync(Guid id)
            => Set.FirstOrDefaultAsync(e => e.Id == id);

        public virtual Task<T?> GetByIdIncludingDeletedAsync(Guid id)
            => Set.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id);

        public virtual async Task<T> AddAsync(T entity)
        {
            await Set.AddAsync(entity);
            return entity;
        }

        public virtual Task<T> UpdateAsync(T entity)
        {
            Db.Entry(entity).State = EntityState.Modified;
            return Task.FromResult(entity);
        }
    }
}

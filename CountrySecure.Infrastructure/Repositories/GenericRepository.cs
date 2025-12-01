using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Interfaces;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories
{
    // Esta clase implementa la interfaz IGenericRepository para cualquier entidad T
    public class GenericRepository<T> : IGenericRepository<T> where T : class , IStatusEntity
    {
        private readonly CountrySecureDbContext _dbContext;

        public GenericRepository(CountrySecureDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 1. Implementación de GetByIdAsync
        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            // Busca la entidad por su clave primaria
            return await _dbContext.Set<T>().FindAsync(id);
        }

        // 2. Implementación de GetAllAsync (CRUD)
        public virtual async Task<IEnumerable<T>> GetAllAsync(int pageNumber, int pageSize)
        {
            // Implementación básica con paginación
            return await _dbContext.Set<T>()
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync();
        }

        // 3. Implementación de AddAsync (CRUD)
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity;
        }

        // 4. Implementación de UpdateAsync (CRUD)
        public virtual Task<T> UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            return Task.FromResult(entity);
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            // 1. Encontrar la entidad (usando T)
            var entityToDelete = await _dbContext.Set<T>().FindAsync(id);

            if (entityToDelete == null)
            {
                return false;
            }

            
            if (entityToDelete is BaseEntity baseEntity)
            {
                baseEntity.Status = "Inactive";

                // Marcar el estado de la entidad en el DbContext para que EF Core la rastree como UPDATE
                _dbContext.Entry(baseEntity).State = EntityState.Modified;

                // El SaveChanges lo hará la Unidad de Trabajo
                return true;
            }

            // Si la entidad no es una BaseEntity (no debería pasar si se sigue la arquitectura)
            return false;
        }

    }
}
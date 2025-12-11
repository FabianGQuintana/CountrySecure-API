using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Interfaces;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories
{
    // Esta clase implementa la interfaz IGenericRepository para cualquier entidad T
    public class GenericRepository<T> : IGenericRepository<T> where T : class
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

        public virtual async Task<T?> SoftDeleteToggleAsync(Guid id)
        {
            // 1. Encontrar la entidad (usando T)
            var entityToToggle = await _dbContext.Set<T>().FindAsync(id);

            if (entityToToggle == null)
            {
                return null;
            }

            // 2. Verificar que sea una BaseEntity (para poder acceder a EntryPermissionState y DeletedAt)
            if (entityToToggle is BaseEntity baseEntity)
            {
                if (baseEntity.IsDeleted) // Equivalente a: baseEntity.DeletedAt.HasValue
                {
                    // Caso: Inactivo → Activo (Reactivar)
                    baseEntity.DeletedAt = null;
                    baseEntity.Status = "Active";
                }
                else
                {
                    // Caso: Activo → Inactivo (Baja Lógica)
                    baseEntity.DeletedAt = DateTime.UtcNow;
                    baseEntity.Status = "Inactive";
                }

                // Auditoría
                baseEntity.UpdatedAt = DateTime.UtcNow;
               
                _dbContext.Set<T>().Update(entityToToggle);

                // El SaveChanges lo hará la Unidad de Trabajo
                return entityToToggle;
            }

            // Si la entidad no es una BaseEntity, no se puede realizar el Soft Delete/Toggle
            return null;
        }

    }
}
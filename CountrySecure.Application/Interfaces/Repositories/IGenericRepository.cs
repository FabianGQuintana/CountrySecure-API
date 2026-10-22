using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    /// <summary>
    /// Operaciones comunes. Las consultas aplican el filtro global de baja lógica (solo entidades activas);
    /// para operar sobre dadas de baja (p. ej. restaurar) usar <see cref="GetByIdIncludingDeletedAsync"/>.
    /// </summary>
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> GetByIdIncludingDeletedAsync(Guid id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
    }
}

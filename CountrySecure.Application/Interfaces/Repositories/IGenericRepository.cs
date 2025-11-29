using System;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        
        Task<T?> GetByIdAsync(int id); 
        Task<IEnumerable<T>> GetAllAsync(int pageNumber, int pageSize);

        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);

        Task<bool> DeleteAsync(int id);
    }
}
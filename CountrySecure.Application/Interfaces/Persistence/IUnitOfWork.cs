using CountrySecure.Application.Interfaces.Repositories;

namespace CountrySecure.Application.Interfaces.Persistence
{
    public interface IUnitOfWork
    {
        // Para cada repo 
        IUserRepository Users { get; }

        Task<int> SaveChangesAsync(); // EF devulve la cantidad de entidades afectadas/modificadas
        
    }
}
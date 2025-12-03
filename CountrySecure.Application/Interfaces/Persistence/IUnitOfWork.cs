using CountrySecure.Application.Interfaces.Repositories;

<<<<<<< Updated upstream
namespace CountrySecure.Application.Interfaces.Persistence
=======
namespace CountrySecure.Application.Interfaces.UnitOfWork
>>>>>>> Stashed changes
{
    public interface IUnitOfWork
    {
        // Para cada repo 
        IUserRepository Users { get; }

        Task<int> SaveChangesAsync(); // EF devulve la cantidad de entidades afectadas/modificadas
    }
}
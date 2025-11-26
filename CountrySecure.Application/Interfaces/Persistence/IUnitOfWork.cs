namespace CountrySecure.Application.Interfaces.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(); // EF devulve la cantidad de entidades afectadas/modificadas
    }
}
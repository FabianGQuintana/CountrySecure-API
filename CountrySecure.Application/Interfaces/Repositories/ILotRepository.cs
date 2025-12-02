using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface ILotRepository : IGenericRepository<Lot>
    {
        Task<Lot?> GetLotByNameLotAsync(string nameLot);

        Task<Lot?> GetLotByNameBlockAsync(string nameBlock);

        // Método para obtener la lista de nombres de bloques únicos (para filtros)
        Task<IEnumerable<string>> GetDistinctBlockNamesAsync();

        Task<IEnumerable<Lot>> GetLotsByStatusAsync(Domain.Enums.LotStatus status, int pageNumber, int pageSize);
    }
}

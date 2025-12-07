using CountrySecure.Application.DTOs.Properties;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;

namespace CountrySecure.Application.Interfaces.Repositories;

public interface IVisitRepository : IGenericRepository<Visit>
{
    Task<IReadOnlyList<Visit>> GetVisitsByDniAsync(int dniVisit);
    Task<Visit?> GetVisitWithPermitsAsync(Guid visitId);
    Task<IEnumerable<EntryPermission>> GetPermitsByVisitIdAsync(Guid visitId);
    Task<IEnumerable<Visit>> GetAllWithoutPaginationAsync();
    Task<EntryPermission?> GetValidPermitByVisitIdAsync(Guid visitId);
}




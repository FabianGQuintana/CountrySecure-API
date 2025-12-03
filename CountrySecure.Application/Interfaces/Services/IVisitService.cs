using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs;
using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.DTOs.EntryPermits;

namespace CountrySecure.Application.Interfaces.Services;

public interface IVisitService
{
    Task<VisitDto> AddNewVisitAsync(CreateVisitDto newVisitDto);
    Task<VisitDto?> GetVisitByIdAsync(Guid visitId);
    Task<IEnumerable<VisitDto>> GetVisitsByDniAsync(int dniVisit);
    Task<VisitDto?> GetVisitWithPermitsAsync(Guid visitId);
    Task<IEnumerable<EntryPermitDto>> GetPermitsByVisitIdAsync(Guid visitId);
    Task<EntryPermitDto?> GetValidPermitByVisitIdAsync(Guid visitId);
    Task UpdateVisitAsync(UpdateVisitDto updateVisitDto);
    Task<bool> SoftDeleteVisitAsync(Guid visitId);
}


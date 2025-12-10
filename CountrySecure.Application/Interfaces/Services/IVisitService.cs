using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IVisitService
    {
        Task<VisitResponseDto> AddNewVisitAsync(CreateVisitDto newVisitDto, Guid currentUserId);
        Task<VisitResponseDto?> GetVisitByIdAsync(Guid visitId);
        Task<IEnumerable<VisitResponseDto>> GetVisitsByDniAsync(int dniVisit);
        Task<IEnumerable<VisitResponseDto>> GetAllVisitsAsync(int pageNumber, int pageSize);
        Task<VisitWithPermitsDto?> GetVisitWithPermitsAsync(Guid visitId);
        Task<IEnumerable<VisitResponseDto>> GetAllVisitsWithoutFilterAsync();
        Task<IEnumerable<EntryPermissionResponseDto>> GetPermitsByVisitIdAsync(Guid visitId);
        Task<IEnumerable<VisitEntryPermissionDto>> GetValidPermitsByVisitIdAsync(Guid visitId);
        Task<VisitResponseDto> UpdateVisitAsync(Guid visitId, UpdateVisitDto updateVisitDto);
        Task<VisitResponseDto?> SoftDeleteVisitAsync(Guid visitId);

    }
}



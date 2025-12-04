using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.DTOs.EntryPermit;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IVisitService
    {
        Task<VisitResponseDto> AddNewVisitAsync(CreateVisitDto newVisitDto, Guid currentUserId);
        Task<VisitResponseDto?> GetVisitByIdAsync(Guid visitId);
        Task<IEnumerable<VisitResponseDto>> GetVisitsByDniAsync(int dniVisit);
        Task<IEnumerable<VisitResponseDto>> GetAllVisitsAsync(int pageNumber, int pageSize);
        Task<VisitResponseDto?> GetVisitWithPermitsAsync(Guid visitId);
        Task<IEnumerable<VisitResponseDto>> GetAllVisitsWithoutFilterAsync();
        Task<IEnumerable<EntryPermitResponseDto>> GetPermitsByVisitIdAsync(Guid visitId);
        Task<EntryPermitResponseDto?> GetValidPermitByVisitIdAsync(Guid visitId);
        Task UpdateVisitAsync(UpdateVisitDto updateVisitDto);
        Task<bool> SoftDeleteVisitAsync(Guid visitId);
    }
}



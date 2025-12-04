using CountrySecure.Application.Mappers;
// using CountrySecure.Application.DTOs.EntryPermit;
using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CountrySecure.Application.Services.Visits
{
    public class VisitService : IVisitService
    {
        private readonly IVisitRepository _visitRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VisitService(IVisitRepository visitRepository, IUnitOfWork unitOfWork)
        {
            _visitRepository = visitRepository;
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // MÉTODOS DE ESCRITURA
        // ============================================================

        public async Task<VisitResponseDto> AddNewVisitAsync(CreateVisitDto newVisitDto, Guid currentUserId)
        {
            // Mapear DTO → Entidad
            var newVisitEntity = newVisitDto.ToEntity();

            // Auditoría
            newVisitEntity.CreatedBy = currentUserId.ToString();
            newVisitEntity.CreatedAt = DateTime.UtcNow;
            newVisitEntity.Status = "Active";

            var addedVisit = await _visitRepository.AddAsync(newVisitEntity);
            await _unitOfWork.SaveChangesAsync();

            return addedVisit.ToResponseDto();
        }

        public async Task UpdateVisitAsync(UpdateVisitDto updateVisitDto)
        {
            var existingEntity = await _visitRepository.GetByIdAsync(updateVisitDto.VisitId);

            if (existingEntity == null || existingEntity.DeletedAt != null)
            {
                throw new KeyNotFoundException($"Visit with ID {updateVisitDto.VisitId} not found.");
            }

            // Aplicar cambios utilizando el mapper
            updateVisitDto.MapToEntity(existingEntity);

            // Auditoría
            existingEntity.LastModifiedAt = DateTime.UtcNow;
          

            await _visitRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> SoftDeleteVisitAsync(Guid visitId)
        {
            var existingEntity = await _visitRepository.GetByIdAsync(visitId);

            if (existingEntity == null || existingEntity.DeletedAt != null)
                return false;

            existingEntity.DeletedAt = DateTime.UtcNow;
            existingEntity.Status = "Inactive";

            await _visitRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // ============================================================
        // MÉTODOS DE CONSULTA
        // ============================================================

        public async Task<VisitResponseDto?> GetVisitByIdAsync(Guid visitId)
        {
            var visitEntity = await _visitRepository.GetByIdAsync(visitId);

            if (visitEntity == null || visitEntity.DeletedAt != null)
                return null;

            return visitEntity.ToResponseDto();
        }

        public async Task<IEnumerable<VisitResponseDto>> GetVisitsByDniAsync(int dniVisit)
        {
            var visits = await _visitRepository.GetVisitsByDniAsync(dniVisit);

            var filtered = visits.Where(v => v.DeletedAt == null);

            return filtered.ToResponseDto();
        }

        public async Task<IEnumerable<VisitResponseDto>> GetAllVisitsAsync(int pageNumber, int pageSize)
        {
            var visits = await _visitRepository.GetAllAsync(pageNumber, pageSize);

            var filtered = visits.Where(v => v.DeletedAt == null);

            return filtered.ToResponseDto();
        }

        public async Task<IEnumerable<VisitResponseDto>> GetAllVisitsWithoutFilterAsync()
        {
            var visits = await _visitRepository.GetAllWithoutPaginationAsync();

            // No filtramos nada → devolvemos TODO, incluso los soft deleted
            return visits.ToResponseDto();
        }


        // public async Task<VisitResponseDto?> GetVisitWithPermitsAsync(Guid visitId)
        // {
        //     var visitEntity = await _visitRepository.GetVisitWithPermitsAsync(visitId);

        //     if (visitEntity == null || visitEntity.DeletedAt != null)
        //         return null;

        //     return visitEntity.ToResponseDto();
        // }

        // public async Task<IEnumerable<EntryPermitResponseDto>> GetPermitsByVisitIdAsync(Guid visitId)
        // {
        //     var permits = await _visitRepository.GetPermitsByVisitIdAsync(visitId);
        //     return permits.ToPermitDto();
        // }

        // public async Task<EntryPermitResponseDto?> GetValidPermitByVisitIdAsync(Guid visitId)
        // {
        //     var permit = await _visitRepository.GetValidPermitByVisitIdAsync(visitId);
        //     return permit?.ToPermitDto();
        // }
    }
}

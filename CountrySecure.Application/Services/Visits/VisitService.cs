using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CountrySecure.Application.Services.Visits
{
    public class VisitService : IVisitService
    {
        private readonly IVisitRepository _visitRepository;
        private readonly IEntryPermissionRepository _entryPermissionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VisitService(IVisitRepository visitRepository,IEntryPermissionRepository entryPermissionRepository, IUnitOfWork unitOfWork)
        {
            _entryPermissionRepository = entryPermissionRepository;
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

        public async Task<VisitResponseDto> UpdateVisitAsync(Guid visitId, UpdateVisitDto updateVisitDto)
        {
            var existingEntity = await _visitRepository.GetByIdWithoutFiltersAsync(visitId);


            if (existingEntity == null )
                throw new KeyNotFoundException($"Visit with ID {visitId} not found.");

            updateVisitDto.MapToEntity(existingEntity);

            existingEntity.LastModifiedAt = DateTime.UtcNow;

            await _visitRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();

            // Volver a cargar si necesitás incluir permisos
            return existingEntity.ToResponseDto();
        }



        public async Task<VisitResponseDto?> SoftDeleteVisitAsync(Guid visitId)
        {
            var visit = await _visitRepository.SoftDeleteVisitAsync(visitId);

            if (visit == null)
                return null;

            await _unitOfWork.SaveChangesAsync();

            return visit.ToResponseDto();
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

            

            return visits.ToResponseDto();
        }

        public async Task<IEnumerable<VisitResponseDto>> GetAllVisitsWithoutFilterAsync()
        {
            var visits = await _visitRepository.GetAllWithoutPaginationAsync();

            // No filtramos nada → devolvemos TODO, incluso los soft deleted
            return visits.ToResponseDto();
        }


        public async Task<VisitWithPermitsDto?> GetVisitWithPermitsAsync(Guid visitId)
        {
            var visitEntity = await _visitRepository.GetVisitWithPermitsAsync(visitId);

            if (visitEntity == null || visitEntity.DeletedAt != null)
                return null;

            return visitEntity.ToVisitWithPermitsDto();
        }


        public async Task<IEnumerable<EntryPermissionResponseDto>> GetPermitsByVisitIdAsync(Guid visitId)
        {
            var permits = await _entryPermissionRepository.GetEntryPermissionsByVisitIdAsync(visitId);
            return permits.ToResponseDto();
        }

        public async Task<IEnumerable<VisitEntryPermissionDto>> GetValidPermitsByVisitIdAsync(Guid visitId)
        {
            var permits = await _entryPermissionRepository.GetEntryPermissionsByVisitIdAsync(visitId);

            // Tomamos todos los permisos válidos (Pending)
            var valid = permits
                .Where(p => p.Status == PermissionStatus.Pending)
                .Select(p => p.ToVisitEntryPermissionDto()) // ← tu mapper liviano
                .ToList();

            return valid;
        }




    }
}

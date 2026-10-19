using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Exceptions;

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

        public async Task<VisitResponseDto> CreateAsync(CreateVisitDto dto, Guid currentUserId)
        {
            await EnsureDniIsFreeAsync(dto.DniVisit, exceptVisitId: null);

            var visit = dto.ToEntity();
            visit.CreatedBy = currentUserId.ToString();

            await _visitRepository.AddAsync(visit);
            await _unitOfWork.SaveChangesAsync();

            return visit.ToResponseDto();
        }

        public async Task<VisitResponseDto> UpdateAsync(Guid id, UpdateVisitDto dto, Guid currentUserId)
        {
            var visit = await GetOrThrowAsync(id);

            if (dto.DniVisit.HasValue && dto.DniVisit.Value != visit.DniVisit)
                await EnsureDniIsFreeAsync(dto.DniVisit.Value, exceptVisitId: id);

            dto.MapToEntity(visit);
            visit.Touch(currentUserId.ToString());

            await _visitRepository.UpdateAsync(visit);
            await _unitOfWork.SaveChangesAsync();

            return visit.ToResponseDto();
        }

        public async Task<VisitResponseDto> DeleteAsync(Guid id, Guid currentUserId)
        {
            var visit = await GetOrThrowAsync(id);
            visit.SoftDelete(currentUserId.ToString());

            await _visitRepository.UpdateAsync(visit);
            await _unitOfWork.SaveChangesAsync();

            return visit.ToResponseDto();
        }

        public async Task<VisitResponseDto> RestoreAsync(Guid id, Guid currentUserId)
        {
            var visit = await _visitRepository.GetByIdIncludingDeletedAsync(id)
                        ?? throw new KeyNotFoundException($"Visit with ID {id} not found.");

            visit.Restore(currentUserId.ToString());

            await _visitRepository.UpdateAsync(visit);
            await _unitOfWork.SaveChangesAsync();

            return visit.ToResponseDto();
        }

        public async Task<VisitResponseDto> GetByIdAsync(Guid id)
            => (await GetOrThrowAsync(id)).ToResponseDto();

        public async Task<VisitWithPermitsDto> GetWithPermitsAsync(Guid id)
        {
            var visit = await _visitRepository.GetWithPermitsAsync(id)
                        ?? throw new KeyNotFoundException($"Visit with ID {id} not found.");
            return visit.ToVisitWithPermitsDto();
        }

        public async Task<IReadOnlyList<VisitEntryPermissionDto>> GetValidPermitsAsync(Guid id)
        {
            var visit = await _visitRepository.GetWithPermitsAsync(id)
                        ?? throw new KeyNotFoundException($"Visit with ID {id} not found.");

            // Permisos vigentes: pendientes y sin dar de baja
            return visit.EntryPermissions
                        .Where(p => p.EntryPermissionState == PermissionStatus.Pending && !p.IsDeleted)
                        .Select(p => p.ToVisitEntryPermissionDto())
                        .ToList();
        }

        public async Task<PagedResult<VisitResponseDto>> GetPagedAsync(PageQuery paging, int? dni, string? search, bool includeInactive)
        {
            var page = await _visitRepository.GetPagedAsync(paging, dni, search, includeInactive);
            return page.Map(v => v.ToResponseDto());
        }

        // El DNI es único en toda la tabla (incluidas las visitas dadas de baja)
        private async Task EnsureDniIsFreeAsync(int dni, Guid? exceptVisitId)
        {
            var existing = await _visitRepository.GetByDniIncludingDeletedAsync(dni);
            if (existing != null && existing.Id != exceptVisitId)
            {
                throw new ConflictException(existing.IsDeleted
                    ? $"A visitor with DNI {dni} already exists but is inactive. Restore it instead."
                    : $"A visitor with DNI {dni} already exists.");
            }
        }

        private async Task<Visit> GetOrThrowAsync(Guid id)
            => await _visitRepository.GetByIdAsync(id)
               ?? throw new KeyNotFoundException($"Visit with ID {id} not found.");
    }
}

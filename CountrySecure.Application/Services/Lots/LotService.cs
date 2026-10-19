using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Lots;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Exceptions;

namespace CountrySecure.Application.Services.Lots
{
    public class LotService : ILotService
    {
        private readonly ILotRepository _lotRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LotService(ILotRepository lotRepository, IUnitOfWork unitOfWork)
        {
            _lotRepository = lotRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<LotResponseDto> CreateAsync(CreateLotDto dto, Guid currentUserId)
        {
            await EnsureNameIsFreeAsync(dto.LotName, exceptLotId: null);

            var lot = dto.ToEntity();
            lot.CreatedBy = currentUserId.ToString();

            await _lotRepository.AddAsync(lot);
            await _unitOfWork.SaveChangesAsync();

            return lot.ToResponseDto();
        }

        public async Task<LotResponseDto> UpdateAsync(Guid id, UpdateLotDto dto, Guid currentUserId)
        {
            var lot = await GetOrThrowAsync(id);

            if (!string.IsNullOrWhiteSpace(dto.LotName) && dto.LotName != lot.LotName)
                await EnsureNameIsFreeAsync(dto.LotName, exceptLotId: id);

            dto.MapToEntity(lot);
            lot.Touch(currentUserId.ToString());

            await _lotRepository.UpdateAsync(lot);
            await _unitOfWork.SaveChangesAsync();

            return lot.ToResponseDto();
        }

        public async Task<LotResponseDto> DeleteAsync(Guid id, Guid currentUserId)
        {
            var lot = await GetOrThrowAsync(id);

            lot.SoftDelete(currentUserId.ToString());
            lot.LotState = LotStatus.Inactive;

            await _lotRepository.UpdateAsync(lot);
            await _unitOfWork.SaveChangesAsync();

            return lot.ToResponseDto();
        }

        public async Task<LotResponseDto> RestoreAsync(Guid id, Guid currentUserId)
        {
            var lot = await _lotRepository.GetByIdIncludingDeletedAsync(id)
                      ?? throw new KeyNotFoundException($"Lot with ID {id} not found.");

            lot.Restore(currentUserId.ToString());
            lot.LotState = LotStatus.Available;

            await _lotRepository.UpdateAsync(lot);
            await _unitOfWork.SaveChangesAsync();

            return lot.ToResponseDto();
        }

        public async Task<LotResponseDto> GetByIdAsync(Guid id)
            => (await GetOrThrowAsync(id)).ToResponseDto();

        public Task<IReadOnlyList<string>> GetBlockNamesAsync()
            => _lotRepository.GetDistinctBlockNamesAsync();

        public async Task<PagedResult<LotResponseDto>> GetPagedAsync(PageQuery paging, LotStatus? state, string? search, bool includeInactive)
        {
            var page = await _lotRepository.GetPagedAsync(paging, state, search, includeInactive);
            return page.Map(l => l.ToResponseDto());
        }

        // El nombre del lote es único en toda la tabla (incluidos los dados de baja)
        private async Task EnsureNameIsFreeAsync(string lotName, Guid? exceptLotId)
        {
            var existing = await _lotRepository.GetByNameIncludingDeletedAsync(lotName);
            if (existing != null && existing.Id != exceptLotId)
            {
                throw new ConflictException(existing.IsDeleted
                    ? $"A lot named '{lotName}' already exists but is inactive. Restore it instead."
                    : $"A lot named '{lotName}' already exists.");
            }
        }

        private async Task<Lot> GetOrThrowAsync(Guid id)
            => await _lotRepository.GetByIdAsync(id)
               ?? throw new KeyNotFoundException($"Lot with ID {id} not found.");
    }
}

using AutoMapper;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Lots;
using CountrySecure.Application.Interfaces.Persistence;

namespace CountrySecure.Application.Services.Lots
{
    public class LotService : ILotService
    {
        private readonly ILotRepository _lotRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LotService(ILotRepository lotRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _lotRepository = lotRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<LotResponseDto> AddNewLotAsync(CreateLotDto newLotDto)
        {
            var newLotEntity = _mapper.Map<Lot>(newLotDto);

            var addedLot = await _lotRepository.AddAsync(newLotEntity);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<LotResponseDto>(addedLot);

        }

        public async Task<LotResponseDto?> GetLotByIdAsync(Guid lotId)
        {
            var lotEntity = await _lotRepository.GetByIdAsync(lotId);
            if (lotEntity == null)
            {
                return null;
            }
            return _mapper.Map<LotResponseDto>(lotEntity);
        }

        public async Task<IEnumerable<LotResponseDto>> GetAllLotsAsync(int pageNumber, int pageSize)
        {
            var lotEntities = await _lotRepository.GetAllAsync(pageNumber, pageSize);
            return _mapper.Map<IEnumerable<LotResponseDto>>(lotEntities);
        }

        public async Task<IEnumerable<string>> GetAllBlockNamesAsync()
        {
            return await _lotRepository.GetDistinctBlockNamesAsync();
        }

        public async Task UpdateLotAsync(UpdateLotDto updateLot, Guid currentId)
        {
            var existingEntity = await _lotRepository.GetByIdAsync(updateLot.Id);

            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"Lot with ID {updateLot.Id} not found.");
            }

            _mapper.Map(updateLot, existingEntity);

           
            await _lotRepository.UpdateAsync(existingEntity);

            
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> SoftDeleteLotAsync(Guid lotId, Guid currentUserId)
        {
            var existingLot = await _lotRepository.GetByIdAsync(lotId);
            if (existingLot == null)
            {
                return false;
            }

            bool marked = await _lotRepository.DeleteAsync(lotId);

            if (marked)
            {
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<LotResponseDto>> GetLotsByStatusAsync(LotStatus status, int pageNumber, int pageSize)
        {
            var lotEntities = await _lotRepository.GetLotsByStatusAsync(status, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<LotResponseDto>>(lotEntities);
        }

    }
}


using AutoMapper;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Interfaces.UnitOfWork;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Visits;

namespace CountrySecure.Application.Services.Visits
{
    public class VisitService : IVisitService
    {
        
        private readonly IVisitRepository _visitRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper; 

        public VisitService(IVisitRepository visitRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _visitRepository = visitRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        // ============================================================
        // 1. Crear una nueva visita
        // ============================================================
        public async Task<VisitDto> AddNewVisitAsync(CreateVisitDto newVisitDto)
        {
            var visit = _mapper.Map<Visit>(newVisitDto);

            await _visitRepository.AddAsync(visit);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<VisitDto>(visit);
        }

        // ============================================================
        // 2. Obtener visita por ID
        // ============================================================
        public async Task<VisitDto?> GetVisitByIdAsync(Guid visitId)
        {
            var visit = await _visitRepository.GetByIdAsync(visitId);

            if (visit == null)
                return null;

            return _mapper.Map<VisitDto>(visit);
        }

        // ============================================================
        // 3. Obtener visitas por DNI
        // ============================================================
        public async Task<IEnumerable<VisitDto>> GetVisitsByDniAsync(int dniVisit)
        {
            var visits = await _visitRepository.GetVisitsByDniAsync(dniVisit);

            return _mapper.Map<IEnumerable<VisitDto>>(visits);
        }

        // ============================================================
        // 4. Obtener visita con todos sus permisos (1:N)
        // ============================================================
        public async Task<VisitDto?> GetVisitWithPermitsAsync(Guid visitId)
        {
            var visit = await _visitRepository.GetVisitWithPermitsAsync(visitId);

            if (visit == null)
                return null;

            return _mapper.Map<VisitDto>(visit);
        }

        // ============================================================
        // 5. Actualizar visita
        // ============================================================
        public async Task UpdateVisitAsync(UpdateVisitDto updateVisitDto)
        {
            var visit = await _visitRepository.GetByIdAsync(updateVisitDto.Id);

            if (visit == null)
                throw new Exception("Visit not found.");

            _mapper.Map(updateVisitDto, visit);

            await _visitRepository.UpdateAsync(visit);
            await _unitOfWork.SaveChangesAsync();
        }

        // ============================================================
        // 6. Soft Delete REVISAR
        // ============================================================
        public async Task<bool> SoftDeleteVisitAsync(Guid visitId)
        {
            var visit = await _visitRepository.GetByIdAsync(visitId);

            if (visit == null)
                return false;

            visit.Status = EntityState.Inactive;

            await _visitRepository.UpdateAsync(visit);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<EntryPermitDto>> GetPermitsByVisitIdAsync(Guid visitId)
        {
            var permits = await _visitRepository.GetPermitsByVisitIdAsync(visitId);
            return _mapper.Map<IEnumerable<EntryPermitDto>>(permits);
        }

        public async Task<EntryPermitDto?> GetValidPermitByVisitIdAsync(Guid visitId)
        {
            var permit = await _visitRepository.GetValidPermitByVisitIdAsync(visitId);
            return _mapper.Map<EntryPermitDto?>(permit);
        }

    }
}

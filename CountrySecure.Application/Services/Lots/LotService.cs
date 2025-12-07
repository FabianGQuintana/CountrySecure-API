using CountrySecure.Application.Mappers;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Lots;
using CountrySecure.Application.Interfaces.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; 

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

        // --- MÉTODOS DE ESCRITURA ---

        public async Task<LotResponseDto> AddNewLotAsync(CreateLotDto newLotDto, Guid currentUserId)
        {
            // 1. Convertir DTO a Entidad
            var newLotEntity = newLotDto.ToEntity();

            // 2. ASIGNACIÓN DE CAMPOS REQUIRED (del token)
            newLotEntity.CreatedBy = currentUserId.ToString();
            newLotEntity.Status = "Active"; // Asignar el estado inicial por defecto
            newLotEntity.CreatedAt = DateTime.UtcNow; // Auditoría

            var addedLot = await _lotRepository.AddAsync(newLotEntity);
            await _unitOfWork.SaveChangesAsync();

            // 3. Mapeo de Entidad a DTO de Respuesta
            return addedLot.ToResponseDto();
        }

        public async Task UpdateAsync(UpdateLotDto updateLot, Guid lotId, Guid currentUserId)
        {
            // 1. Buscar la entidad existente por ID
            // Se usa GetByIdAsync del repositorio IGenericRepository<T>
            var lotToUpdate = await _lotRepository.GetByIdAsync(lotId);

            // 2. Manejar el caso de no encontrado
            if (lotToUpdate == null)
            {
                // Es crucial lanzar una excepción para notificar que el recurso no existe.
                throw new KeyNotFoundException($"El Lote con ID {lotId} no fue encontrado.");
            }

            // 3. Mapear o aplicar las modificaciones del DTO a la entidad
            // Se aplican solo los valores que tienen datos en el DTO.
            if (!string.IsNullOrWhiteSpace(updateLot.LotName))
            {
                lotToUpdate.LotName = updateLot.LotName;
            }

            if (!string.IsNullOrWhiteSpace(updateLot.BlockName))
            {
                lotToUpdate.BlockName = updateLot.BlockName;
            }

            // 4. Actualizar el Status (si viene en el DTO)
            // Se convierte el enum LotStatus del DTO a string para el BaseEntity.Status
            if (updateLot.Status.HasValue)
            {
                lotToUpdate.Status = updateLot.Status.Value.ToString();
            }


            // 5. **Auditoría:** Actualizar campos de modificación (heredados de BaseEntity)
            // Se utiliza la lógica de auditoría de tu BaseEntity
            lotToUpdate.LastModifiedAt = DateTime.UtcNow;
            lotToUpdate.LastModifiedBy = currentUserId.ToString();


            // 6. Guardar los cambios en el repositorio
            // Se utiliza UpdateAsync del repositorio IGenericRepository<T>
            await _lotRepository.UpdateAsync(lotToUpdate);
        }
        public async Task<bool> SoftDeleteLotAsync(Guid lotId, Guid currentUserId)
        {

            var existingLot = await _lotRepository.GetByIdAsync(lotId);
            if (existingLot == null)
            {
                return false;
            }

            // Llama al repositorio para marcar el estado como "Inactive"
            bool marked = await _lotRepository.DeleteAsync(lotId);

            if (marked)
            {
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            return false;
        }

        // --- MÉTODOS DE CONSULTA ---

        public async Task<LotResponseDto?> GetLotByIdAsync(Guid lotId)
        {
            var lotEntity = await _lotRepository.GetByIdAsync(lotId);
            if (lotEntity == null) return null;

            // Usar ToResponseDto()
            return lotEntity.ToResponseDto();
        }

        public async Task<IEnumerable<LotResponseDto>> GetAllLotsAsync(int pageNumber, int pageSize)
        {
            var lotEntities = await _lotRepository.GetAllAsync(pageNumber, pageSize);

            // Usar la extensión de colección ToResponseDto()
            return lotEntities.ToResponseDto();
        }

        public async Task<IEnumerable<string>> GetAllBlockNamesAsync()
        {
            return await _lotRepository.GetDistinctBlockNamesAsync();
        }

        public async Task<IEnumerable<LotResponseDto>> GetLotsByStatusAsync(LotStatus status, int pageNumber, int pageSize)
        {
            var lotEntities = await _lotRepository.GetLotsByStatusAsync(status, pageNumber, pageSize);

            // Usar la extensión de colección ToResponseDto()
            return lotEntities.ToResponseDto();
        }
    }
}
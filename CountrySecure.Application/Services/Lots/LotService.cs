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
            // 1. Convertir DTO a Entidad (Ahora mapea LotState)
            var newLotEntity = newLotDto.ToEntity();

            // 2. ASIGNACIÓN DE AUDITORÍA Y CAMPOS BASE

            // Asignación del Creador
            newLotEntity.CreatedBy = currentUserId.ToString();

            //  Asignamos el EntryPermissionState de la BaseEntity (string) a "Active"
            newLotEntity.Status = "Active";

            // El LotState (el enum que vino del DTO) ya está asignado.

            newLotEntity.CreatedAt = DateTime.UtcNow; // Auditoría

            var addedLot = await _lotRepository.AddAsync(newLotEntity);
            await _unitOfWork.SaveChangesAsync();

            // 3. Mapeo de Entidad a DTO de Respuesta
            return addedLot.ToResponseDto();
        }

        public async Task<LotResponseDto> UpdateAsync(UpdateLotDto updateLot, Guid lotId, Guid currentUserId)
        {
            // 1. Buscar la entidad existente por ID
            var lotToUpdate = await _lotRepository.GetByIdAsync(lotId);

            // 2. Manejar el caso de no encontrado (Termina aquí si no existe)
            if (lotToUpdate == null)
            {
                throw new KeyNotFoundException($"El Lote con ID {lotId} no fue encontrado.");
            }

            // 3. Aplicar mapeo manual de los campos

            // El método String.IsNullOrWhiteSpace es robusto para validar la entrada
            if (!string.IsNullOrWhiteSpace(updateLot.LotName))
            {
                lotToUpdate.LotName = updateLot.LotName;
            }

            if (!string.IsNullOrWhiteSpace(updateLot.BlockName))
            {
                lotToUpdate.BlockName = updateLot.BlockName;
            }

            // Los tipos de valor nulleables (Enum?) requieren chequeo HasValue
            if (updateLot.Status.HasValue)
            {
                // Asignamos el valor del DTO al campo LotState de la entidad
                lotToUpdate.LotState = updateLot.Status.Value;
            }

            // 4. Auditoría (Heredado de BaseEntity)
            lotToUpdate.LastModifiedAt = DateTime.UtcNow;
            lotToUpdate.LastModifiedBy = currentUserId.ToString();

            // 5. Persistencia
            // Es buena práctica capturar el resultado de UpdateAsync (aunque puede ser el mismo objeto)
            var updatedEntity = await _lotRepository.UpdateAsync(lotToUpdate);
            await _unitOfWork.SaveChangesAsync();

            // 6. Retorno (Garantizado: Siempre devuelve un DTO si la ejecución llega aquí)
            return updatedEntity.ToResponseDto();
        }
        public async Task<LotResponseDto?> SoftDeleteToggleAsync(Guid lotId, Guid currentUserId)
        {
            // 1. Usar el repositorio genérico para alternar el estado (DeletedAt, Status)
            var lot = await _lotRepository.SoftDeleteToggleAsync(lotId);

            if (lot == null)
                return null; // Not found

            // 2. Aplicar Auditoría:
            lot.LastModifiedAt = DateTime.UtcNow;
            lot.LastModifiedBy = currentUserId.ToString();

            // 3. Lógica Específica del Enum (LotState)
            if (lot.Status == "Inactive")
            {
                // Si se acaba de desactivar, marcamos el estado funcional (Enum) como Inactive.
                lot.LotState = LotStatus.Inactive;
            }
            else
            {
                // Si se acaba de reactivar, marcamos el estado funcional (Enum) como Available.
                lot.LotState = LotStatus.Available;
            }

            // 4. Persistencia (Guardar los cambios de Auditoría y LotState)
            var updatedEntity = await _lotRepository.UpdateAsync(lot);
            await _unitOfWork.SaveChangesAsync();

            // 5. Mapeo de Retorno
            return updatedEntity.ToResponseDto();
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
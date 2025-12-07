using CountrySecure.Application.DTOs.Turns;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Enums;


namespace CountrySecure.Application.Services.Turns
{
    public class TurnService : ITurnService
    {
        private readonly ITurnRepository _turnRepository;

        private readonly IUnitOfWork _unitOfWork;

        public TurnService(ITurnRepository turnRepository, IUnitOfWork unitOfWork)
        {
            _turnRepository = turnRepository;
            _unitOfWork = unitOfWork;
        }

        // -------------------------------------------------------------------
        // MÉTODOS DE ESCRITURA
        // -------------------------------------------------------------------

        public async Task<TurnResponseDto> AddNewTurnAsync(CreateTurnDto newTurnDto, Guid currentUserId)
        {
            // 1. Mapeo DTO -> Entidad (solo mapea FKs, Start/End Time)
            var newTurnEntity = newTurnDto.ToEntity();

            // 2. Asignación de campos de auditoría y de negocio
            newTurnEntity.UserId = currentUserId; // El UserId viene del token
            newTurnEntity.CreatedBy = currentUserId.ToString();
            newTurnEntity.CreatedAt = DateTime.UtcNow;
            //newTurnEntity.Status = TurnStatus.Pending; // Estado inicial

            // 3. Persistencia
            var addedTurn = await _turnRepository.AddAsync(newTurnEntity);
            await _unitOfWork.SaveChangesAsync();

            // 4. Devolver DTO de Respuesta
            return addedTurn.ToResponseDto();
        }

        public async Task<TurnResponseDto?> UpdateTurnAsync(Guid turnId, UpdateTurnDto updateTurnDto, Guid currentUserId)
        {
            // 1. Buscar entidad existente
            var existingTurn = await _turnRepository.GetByIdAsync(turnId);

            if (existingTurn == null)
            {
                return null; // El Controller devolverá 404 Not Found
            }

            // 2. Aplicar la lógica de negocio/autorización
            // Ejemplo: Solo el creador o un admin puede modificar.
            if (existingTurn.CreatedBy != currentUserId.ToString())
            {
                throw new UnauthorizedAccessException("User is not authorized to update this Turn.");
            }

            // 3. Mapear DTO -> Entidad Existente
            //updateTurnDto.MapToEntity(existingTurn);

            // 4. Actualizar Auditoría
            existingTurn.LastModifiedAt = DateTime.UtcNow;
            existingTurn.LastModifiedBy = currentUserId.ToString();

            // 5. Persistencia
            var updatedTurn = await _turnRepository.UpdateAsync(existingTurn);
            await _unitOfWork.SaveChangesAsync();

            // 6. Devolver DTO de Respuesta
            return updatedTurn.ToResponseDto();
        }

        public async Task<bool> SoftDeleteTurnAsync(Guid turnId, Guid currentUserId)
        {
            // 1. Buscar y validar
            var existingTurn = await _turnRepository.GetByIdAsync(turnId);

            if (existingTurn == null)
            {
                return false; // No se encontró
            }

            // 2. Lógica de Baja Lógica (Actualización de BaseEntity)
            existingTurn.DeletedAt = DateTime.UtcNow;
            existingTurn.LastModifiedBy = currentUserId.ToString();

            // Opcional: Podrías cambiar el estado del turno a Cancelled/Inactive si lo requieres.
            //existingTurn.Status = TurnStatus.CancelledByAdmin; 

            await _turnRepository.UpdateAsync(existingTurn);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // --- MÉTODOS DE LECTURA (R) ---

        public async Task<TurnResponseDto?> GetTurnByIdAsync(Guid turnId)
        {
            // Nota: Se debe asegurar que el repositorio cargue las propiedades de navegación (User, Amenity)
            // para que ToResponseDto no lance la excepción InvalidOperationException.
            var turn = await _turnRepository.GetByIdAsync(turnId);

            return turn?.ToResponseDto();
        }

        public async Task<IEnumerable<TurnResponseDto>> GetTurnsByUserIdAsync(Guid userId)
        {
            var turns = await _turnRepository.GetTurnsByUserId(userId);
            return turns.ToResponseDto();
        }

        public async Task<IEnumerable<TurnResponseDto>> GetTurnsByAmenityIdAsync(Guid amenityId)
        {
            var turns = await _turnRepository.GetTurnsByAmenityId(amenityId);
            return turns.ToResponseDto();
        }

        public async Task<IEnumerable<TurnResponseDto>> GetTurnsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var turns = await _turnRepository.GetTurnsByDateRange(startDate, endDate);
            return turns.ToResponseDto();
        }


    }
}

using CountrySecure.Application.DTOs.Turns;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Entities;
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
            // 1. Mapeo DTO -> Entidad
            var newTurnEntity = newTurnDto.ToEntity();

            // 2. Asignación de campos de auditoría y de negocio
            newTurnEntity.UserId = currentUserId;
            newTurnEntity.CreatedBy = currentUserId.ToString();
            newTurnEntity.CreatedAt = DateTime.UtcNow;
            newTurnEntity.Status = "Active"; // Asignar estado de BaseEntity (string)


            // 3. Persistencia inicial
            var addedTurn = await _turnRepository.AddAsync(newTurnEntity);
            await _unitOfWork.SaveChangesAsync();

            //Eager Loading de las propiedades de navegación necesarias para el DTO
            var fullTurn = await _turnRepository.GetByIdWithIncludesAsync(addedTurn.Id);

            if (fullTurn == null)
            {
                // Esto no debería suceder, pero es defensivo
                throw new InvalidOperationException("Turn was created but could not be retrieved with details.");
            }

            // 5. Devolver DTO de Respuesta (usando la entidad completa)
            return fullTurn.ToResponseDto();
        }

        public async Task<TurnResponseDto?> UpdateTurnAsync(Guid turnId, UpdateTurnDto updateTurnDto, Guid currentUserId)
        {
            // 1. Buscar entidad existente
            var existingTurn = await _turnRepository.GetByIdAsync(turnId);

            if (existingTurn == null)
            {
                return null; // El Controller devolverá 404 Not Found
            }

            // 2. Aplicar la lógica de negocio/autorización (OK)
            if (existingTurn.CreatedBy != currentUserId.ToString())
            {
                throw new UnauthorizedAccessException("User is not authorized to update this Turn.");
            }

            // 3. Mapear DTO -> Entidad Existente (DESCOMENTADO Y REQUERIDO)
            updateTurnDto.MapToEntity(existingTurn);

            // 4. Actualizar Auditoría (OK)
            existingTurn.LastModifiedAt = DateTime.UtcNow;
            existingTurn.LastModifiedBy = currentUserId.ToString();

            // 5. Persistencia
            var updatedTurn = await _turnRepository.UpdateAsync(existingTurn);
            await _unitOfWork.SaveChangesAsync();

            var fullTurn = await _turnRepository.GetByIdWithIncludesAsync(updatedTurn.Id);

            if (fullTurn == null)
            {
                // En caso de fallo post-inserción/update
                throw new InvalidOperationException("Turn was updated but could not be retrieved with full relations.");
            }

            return fullTurn.ToResponseDto();


        }

        public async Task<TurnResponseDto?> SoftDeleteTurnAsync(Guid turnId, Guid currentUserId)
        {
            var existingTurn = await _turnRepository.SoftDeleteToggleAsync(turnId);

            if (existingTurn == null)
            {
                return null; // No se encontró.
            }

            // 1. Aplicar Auditoría
            existingTurn.LastModifiedAt = DateTime.UtcNow;
            existingTurn.LastModifiedBy = currentUserId.ToString();

            // 2. Lógica Específica de la Entidad (TurnStatus)
            if (existingTurn.Status == "Inactive")
            {
                existingTurn.TurnStatus = TurnStatus.Cancelled;
            }
            else
            {
                existingTurn.TurnStatus = TurnStatus.Pending;
            }

            await _turnRepository.UpdateAsync(existingTurn); // Asegura que auditoría y TurnStatus se persistan
            await _unitOfWork.SaveChangesAsync();

            // 3. Devolver el DTO de Respuesta
            // Se necesita una versión con includes para el DTO. Usaré GetByIdWithIncludesAsync
            var fullTurn = await _turnRepository.GetByIdWithIncludesAsync(existingTurn.Id);

            // Si la entidad se acaba de cargar (fullTurn), significa que la operación fue exitosa.
            return fullTurn?.ToResponseDto();
        }
        // --- MÉTODOS DE LECTURA (R) ---

        public async Task<TurnResponseDto?> GetTurnByIdAsync(Guid turnId)
        {

            var turn = await _turnRepository.GetByIdWithIncludesAsync(turnId);

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

        public async Task<IEnumerable<TurnResponseDto>> GetAllTurnsAsync(int pageNumber, int pageSize)
        {
            // 1. Obtener la colección con las relaciones cargadas
            var turns = await _turnRepository.GetAllWithIncludesAsync(pageNumber, pageSize);

            // 2. Filtrar los turnos que tienen baja lógica (IsDeleted)
            // Usamos el filtro explícito en LINQ para excluir los soft-deleted
            var filteredTurns = turns.Where(t => !t.IsDeleted);

            // 3. Mapeo a DTOs
            return filteredTurns.ToResponseDto();
        }


    }
}

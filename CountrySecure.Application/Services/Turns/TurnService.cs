using CountrySecure.Application.DTOs.Turns;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Exceptions;


namespace CountrySecure.Application.Services.Turns
{
    public class TurnService : ITurnService
    {
        private readonly ITurnRepository _turnRepository;

        private readonly IAmenityRepository _amenityRepository;

        private readonly IUnitOfWork _unitOfWork;

        public TurnService(ITurnRepository turnRepository, IAmenityRepository amenityRepository, IUnitOfWork unitOfWork)
        {
            _turnRepository = turnRepository;
            _amenityRepository = amenityRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Reglas de reserva:
        ///  1. StartTime &lt; EndTime y el turno no puede empezar en el pasado.
        ///  2. El amenity debe existir y estar activo.
        ///  3. El mismo usuario no puede tener dos reservas solapadas en el mismo amenity.
        ///  4. La cantidad de reservas simultáneas no puede superar Capacity
        ///     (Capacity = 1 equivale a uso exclusivo: ningún solapamiento permitido).
        /// </summary>
        private async Task EnsureTurnIsBookableAsync(Guid amenityId, Guid userId, DateTime start, DateTime end, Guid? excludeTurnId = null)
        {
            if (start >= end)
                throw new BusinessRuleException("La hora de inicio debe ser anterior a la hora de fin.");

            if (excludeTurnId == null && start < DateTime.UtcNow)
                throw new BusinessRuleException("No se puede reservar un turno en el pasado.");

            var amenity = await _amenityRepository.GetByIdAsync(amenityId);
            if (amenity == null || amenity.IsDeleted)
                throw new KeyNotFoundException($"El amenity con ID {amenityId} no existe o está inactivo.");

            var overlapping = await _turnRepository.GetActiveOverlappingTurnsAsync(amenityId, start, end, excludeTurnId);

            if (overlapping.Any(t => t.UserId == userId))
                throw new ConflictException("Ya tenés una reserva en este amenity que se superpone con ese horario.");

            var capacity = Math.Max(amenity.Capacity, 1);
            if (overlapping.Count >= capacity)
                throw new ConflictException(capacity == 1
                    ? $"'{amenity.AmenityName}' ya está reservado en ese horario."
                    : $"'{amenity.AmenityName}' alcanzó su capacidad máxima ({capacity}) en ese horario.");
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

            // Reglas de dominio: horario válido, sin solapamiento y dentro de la capacidad
            await EnsureTurnIsBookableAsync(newTurnEntity.AmenityId, currentUserId, newTurnEntity.StartTime, newTurnEntity.EndTime);


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

            // 3. Mapear DTO -> Entidad Existente
            updateTurnDto.MapToEntity(existingTurn);

            // Revalidar reglas si el turno sigue activo (puede haber cambiado horario o amenity)
            if (existingTurn.TurnStatus != TurnStatus.Cancelled)
                await EnsureTurnIsBookableAsync(existingTurn.AmenityId, existingTurn.UserId, existingTurn.StartTime, existingTurn.EndTime, existingTurn.Id);

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

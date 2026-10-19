using CountrySecure.Application.DTOs.Common;
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
        private readonly ICurrentUserService _currentUser;

        public TurnService(
            ITurnRepository turnRepository,
            IAmenityRepository amenityRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _turnRepository = turnRepository;
            _amenityRepository = amenityRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<TurnResponseDto> CreateAsync(CreateTurnDto dto, Guid currentUserId)
        {
            var turn = dto.ToEntity();
            turn.UserId = currentUserId;
            turn.CreatedBy = currentUserId.ToString();

            // Reglas de dominio: horario válido, sin solapamiento y dentro de la capacidad
            await EnsureTurnIsBookableAsync(turn.AmenityId, turn.UserId, turn.StartTime, turn.EndTime);

            await _turnRepository.AddAsync(turn);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(turn.Id);
        }

        public async Task<TurnResponseDto> UpdateAsync(Guid id, UpdateTurnDto dto, Guid currentUserId)
        {
            var turn = await GetOwnedOrThrowAsync(id);

            // Solo un Admin puede reasignar una reserva a otro usuario
            if (dto.UserId.HasValue && dto.UserId != turn.UserId && !_currentUser.IsAdmin)
                throw new UnauthorizedAccessException("Only an Admin can reassign a reservation.");

            dto.MapToEntity(turn);
            turn.Touch(currentUserId.ToString());

            // Revalidar reglas si la reserva sigue activa (puede haber cambiado el horario o el amenity)
            if (turn.TurnStatus != TurnStatus.Cancelled)
                await EnsureTurnIsBookableAsync(turn.AmenityId, turn.UserId, turn.StartTime, turn.EndTime, turn.Id);

            await _turnRepository.UpdateAsync(turn);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id);
        }

        public async Task<TurnResponseDto> DeleteAsync(Guid id, Guid currentUserId)
        {
            var turn = await GetOwnedOrThrowAsync(id);

            turn.SoftDelete(currentUserId.ToString());
            turn.TurnStatus = TurnStatus.Cancelled;

            await _turnRepository.UpdateAsync(turn);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id, includeDeleted: true);
        }

        public async Task<TurnResponseDto> RestoreAsync(Guid id, Guid currentUserId)
        {
            var turn = await _turnRepository.GetByIdIncludingDeletedAsync(id)
                       ?? throw new KeyNotFoundException($"Turn with ID {id} not found.");

            if (!_currentUser.IsAdmin && turn.UserId != _currentUser.UserId)
                throw new UnauthorizedAccessException("You can only restore your own reservations.");

            // Al restaurar hay que volver a comprobar que el horario sigue libre
            await EnsureTurnIsBookableAsync(turn.AmenityId, turn.UserId, turn.StartTime, turn.EndTime, turn.Id);

            turn.Restore(currentUserId.ToString());
            turn.TurnStatus = TurnStatus.Pending;

            await _turnRepository.UpdateAsync(turn);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id);
        }

        public async Task<TurnResponseDto> GetByIdAsync(Guid id)
        {
            var turn = await _turnRepository.GetByIdWithIncludesAsync(id)
                       ?? throw new KeyNotFoundException($"Turn with ID {id} not found.");

            if (!_currentUser.IsAdmin && turn.UserId != _currentUser.UserId)
                throw new UnauthorizedAccessException("You can only view your own reservations.");

            return turn.ToResponseDto();
        }

        public async Task<PagedResult<TurnResponseDto>> GetPagedAsync(
            PageQuery paging, Guid? userId, Guid? amenityId, DateTime? from, DateTime? to, TurnStatus? status, bool onlyMine, bool includeInactive)
        {
            var admin = _currentUser.IsAdmin;
            if (onlyMine || !admin) userId = _currentUser.GetRequiredUserId();

            // Las bajas (reservas canceladas) solo las ve un Admin o su propio dueño (historial personal)
            var ownHistory = userId == _currentUser.UserId;
            var page = await _turnRepository.GetPagedAsync(paging, userId, amenityId, from, to, status, includeInactive && (admin || ownHistory));
            return page.Map(t => t.ToResponseDto());
        }

        // --- helpers ---

        private async Task<TurnResponseDto> ReloadAsync(Guid id, bool includeDeleted = false)
        {
            var turn = await _turnRepository.GetByIdWithIncludesAsync(id, includeDeleted)
                       ?? throw new KeyNotFoundException($"Turn with ID {id} not found.");
            return turn.ToResponseDto();
        }

        private async Task<Turn> GetOwnedOrThrowAsync(Guid id)
        {
            var turn = await _turnRepository.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException($"Turn with ID {id} not found.");

            if (!_currentUser.IsAdmin && turn.UserId != _currentUser.UserId)
                throw new UnauthorizedAccessException("You can only modify your own reservations.");

            return turn;
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

            var amenity = await _amenityRepository.GetByIdAsync(amenityId)
                          ?? throw new KeyNotFoundException($"El amenity con ID {amenityId} no existe o está inactivo.");

            var overlapping = await _turnRepository.GetActiveOverlappingTurnsAsync(amenityId, start, end, excludeTurnId);

            if (overlapping.Any(t => t.UserId == userId))
                throw new ConflictException("Ya tenés una reserva en este amenity que se superpone con ese horario.");

            var capacity = Math.Max(amenity.Capacity, 1);
            if (overlapping.Count >= capacity)
                throw new ConflictException(capacity == 1
                    ? $"'{amenity.AmenityName}' ya está reservado en ese horario."
                    : $"'{amenity.AmenityName}' alcanzó su capacidad máxima ({capacity}) en ese horario.");
        }
    }
}

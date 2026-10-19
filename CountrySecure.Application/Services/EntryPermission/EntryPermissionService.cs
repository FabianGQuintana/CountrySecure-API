using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Exceptions;
using PermissionEntity = CountrySecure.Domain.Entities.EntryPermission;

namespace CountrySecure.Application.Services.EntryPermission
{
    public class EntryPermissionService : IEntryPermissionService
    {
        private readonly IEntryPermissionRepository _permissionRepository;
        private readonly IVisitRepository _visitRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public EntryPermissionService(
            IEntryPermissionRepository permissionRepository,
            IVisitRepository visitRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _permissionRepository = permissionRepository;
            _visitRepository = visitRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        // -------------------------------------------------------------------
        // ESCRITURA
        // -------------------------------------------------------------------

        public async Task<EntryPermissionResponseDto> CreateAsync(CreateEntryPermissionDto dto, Guid currentUserId)
        {
            var permission = dto.ToEntity();

            // Un residente siempre crea permisos a su nombre; solo un Admin puede elegir otro titular
            if (!_currentUser.IsAdmin) permission.UserId = currentUserId;

            EnsureValidWindow(permission.ValidFrom, permission.ValidTo);

            if (await _visitRepository.GetByIdAsync(permission.VisitId) == null)
                throw new KeyNotFoundException($"Visit with ID {permission.VisitId} not found.");

            if (permission.OrderId.HasValue && await _orderRepository.GetByIdAsync(permission.OrderId.Value) == null)
                throw new KeyNotFoundException($"Service with ID {permission.OrderId} not found.");

            permission.QrCodeValue = Guid.NewGuid().ToString();
            permission.CreatedBy = currentUserId.ToString();

            await _permissionRepository.AddAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(permission.Id);
        }

        public async Task<EntryPermissionResponseDto> UpdateAsync(Guid id, UpdateEntryPermissionDto dto, Guid currentUserId)
        {
            var permission = await GetOwnedOrThrowAsync(id);

            if (permission.EntryTime.HasValue)
                throw new ConflictException("El permiso ya fue utilizado y no se puede modificar.");

            dto.MapToEntity(permission);
            EnsureValidWindow(permission.ValidFrom, permission.ValidTo);

            if (permission.OrderId.HasValue && await _orderRepository.GetByIdAsync(permission.OrderId.Value) == null)
                throw new KeyNotFoundException($"Service with ID {permission.OrderId} not found.");

            // Un permiso pendiente cuya vigencia ya terminó queda vencido
            if (permission.ValidTo.HasValue && DateTime.UtcNow > permission.ValidTo.Value
                && permission.EntryPermissionState == PermissionStatus.Pending)
            {
                permission.EntryPermissionState = PermissionStatus.Expired;
            }

            permission.Touch(currentUserId.ToString());

            await _permissionRepository.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id);
        }

        public async Task<EntryPermissionResponseDto> DeleteAsync(Guid id, Guid currentUserId)
        {
            var permission = await GetOwnedOrThrowAsync(id);

            if (permission.EntryTime.HasValue)
                throw new ConflictException("El permiso ya fue utilizado y no se puede cancelar.");

            permission.SoftDelete(currentUserId.ToString());
            permission.EntryPermissionState = PermissionStatus.Cancelled;

            await _permissionRepository.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id, includeDeleted: true);
        }

        public async Task<EntryPermissionResponseDto> RestoreAsync(Guid id, Guid currentUserId)
        {
            var permission = await _permissionRepository.GetByIdIncludingDeletedAsync(id)
                             ?? throw new KeyNotFoundException($"Entry permission with ID {id} not found.");

            EnsureCanAccess(permission);

            permission.Restore(currentUserId.ToString());
            permission.EntryPermissionState = PermissionStatus.Pending;

            await _permissionRepository.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id);
        }

        // -------------------------------------------------------------------
        // LECTURA
        // -------------------------------------------------------------------

        public async Task<EntryPermissionResponseDto> GetByIdAsync(Guid id)
        {
            var permission = await _permissionRepository.GetWithDetailsAsync(id)
                             ?? throw new KeyNotFoundException($"Entry permission with ID {id} not found.");

            EnsureCanAccess(permission);
            return permission.ToResponseDto();
        }

        public async Task<PagedResult<EntryPermissionResponseDto>> GetPagedAsync(
            PageQuery paging,
            PermissionType? type,
            PermissionStatus? status,
            Guid? userId,
            Guid? visitId,
            Guid? orderId,
            string? search,
            bool includeInactive)
        {
            // Un residente solo ve sus propios permisos
            if (_currentUser.Role == Domain.Constants.RoleTypes.Resident)
                userId = _currentUser.GetRequiredUserId();

            // Los permisos cancelados solo los ve un Admin o su titular (historial personal)
            var ownHistory = userId.HasValue && userId == _currentUser.UserId;
            var page = await _permissionRepository.GetPagedAsync(
                paging, type, status, userId, visitId, orderId, search, includeInactive && (_currentUser.IsAdmin || ownHistory));

            return page.Map(p => p.ToResponseDto());
        }

        public async Task<PagedResult<EntryPermissionResponseDto>> GetTodayAsync(PageQuery paging)
        {
            // Inicio del día local del servidor, expresado en UTC (PostgreSQL timestamptz)
            var dayStartUtc = DateTime.Today.ToUniversalTime();
            var page = await _permissionRepository.GetTodayAsync(paging, dayStartUtc);
            return page.Map(p => p.ToResponseDto());
        }

        public async Task<PagedResult<EntryPermissionResponseDto>> GetHistoryAsync(PageQuery paging, string? search, PermissionType? type)
        {
            var page = await _permissionRepository.GetHistoryAsync(paging, search, type);
            return page.Map(p => p.ToResponseDto());
        }

        // -------------------------------------------------------------------
        // PORTERÍA
        // -------------------------------------------------------------------

        public async Task<GateCheckResponseDto> ValidateQrCodeAsync(string qrCodeValue)
        {
            var permission = await _permissionRepository.GetByQrCodeAsync(qrCodeValue, includeDeleted: true)
                             ?? throw new KeyNotFoundException("QR Code no encontrado en el sistema.");

            // Cancelado o dado de baja -> 409
            EnsureNotCancelled(permission);

            // Ya registró entrada y salida (Luz Roja) -> 409
            if (permission.EntryTime != null && permission.DepartureTime != null)
                throw new ConflictException("El visitante ya registró ingreso y salida. El permiso está consumido.");

            // Si todavía no ingresó, debe estar dentro de su ventana de vigencia -> 422
            // (si ya está adentro, se le permite registrar la salida aunque la ventana haya terminado)
            if (permission.EntryTime == null)
                EnsureWithinWindow(permission);

            var visitorName = permission.Visit != null ? $"{permission.Visit.NameVisit} {permission.Visit.LastNameVisit}" : string.Empty;
            var residentName = permission.User != null ? $"{permission.User.Name} {permission.User.Lastname}" : string.Empty;
            var inside = permission.EntryTime != null;

            return new GateCheckResponseDto
            {
                PermissionId = permission.Id,
                VisitorFullName = visitorName,
                ResidentFullName = residentName,
                VisitorDni = permission.Visit?.DniVisit ?? 0,
                EntryTime = permission.EntryTime,
                DepartureTime = null,
                // Estados que consume el front para el semáforo del guardia
                CheckResultStatus = inside ? "Dentro - Marcar Salida" : "Autorizado - Marcar Entrada",
                Message = inside
                    ? "Visitante actualmente dentro. Datos corroborados. Proceder a registrar SALIDA."
                    : "Permiso Válido. Datos corroborados. Proceder a registrar ENTRADA."
            };
        }

        public async Task<EntryPermissionResponseDto> RegisterCheckInAsync(Guid permissionId, Guid guardId)
        {
            var permission = await _permissionRepository.GetWithDetailsAsync(permissionId)
                             ?? throw new KeyNotFoundException($"Permiso de Entrada con ID {permissionId} no encontrado.");

            EnsureNotCancelled(permission);

            if (permission.EntryPermissionState != PermissionStatus.Pending)
                throw new ConflictException($"El permiso ID {permissionId} ya tiene un registro de entrada o no está pendiente.");

            // Solo se permite ingresar dentro de la ventana ValidFrom..ValidTo
            EnsureWithinWindow(permission);

            permission.EntryTime = DateTime.UtcNow;
            permission.EntryPermissionState = PermissionStatus.Completed;
            permission.CheckInGuardId = guardId;
            permission.Touch(guardId.ToString());

            await _permissionRepository.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(permissionId);
        }

        public async Task<EntryPermissionResponseDto> RegisterCheckOutAsync(Guid permissionId, Guid guardId)
        {
            var permission = await _permissionRepository.GetWithDetailsAsync(permissionId)
                             ?? throw new KeyNotFoundException($"Permiso de Entrada con ID {permissionId} no encontrado.");

            if (permission.EntryTime == null)
                throw new ConflictException($"No se puede registrar la salida del permiso ID {permissionId}. La entrada no fue registrada.");

            if (permission.DepartureTime.HasValue)
                throw new ConflictException($"La salida del permiso ID {permissionId} ya fue registrada.");

            permission.DepartureTime = DateTime.UtcNow;
            permission.CheckOutGuardId = guardId;
            permission.Touch(guardId.ToString());

            await _permissionRepository.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(permissionId);
        }

        // -------------------------------------------------------------------
        // REGLAS DE DOMINIO Y AUXILIARES
        // -------------------------------------------------------------------

        private async Task<EntryPermissionResponseDto> ReloadAsync(Guid id, bool includeDeleted = false)
        {
            var permission = await _permissionRepository.GetWithDetailsAsync(id, includeDeleted)
                             ?? throw new KeyNotFoundException($"Entry permission with ID {id} not found.");
            return permission.ToResponseDto();
        }

        private async Task<PermissionEntity> GetOwnedOrThrowAsync(Guid id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id)
                             ?? throw new KeyNotFoundException($"Entry permission with ID {id} not found.");

            // Editar o cancelar: solo el titular o un Admin (Seguridad no gestiona permisos ajenos)
            if (!_currentUser.IsAdmin && permission.UserId != _currentUser.UserId)
                throw new UnauthorizedAccessException("You can only manage your own entry permissions.");

            return permission;
        }

        // Leer: el titular, Admin o Seguridad
        private void EnsureCanAccess(PermissionEntity permission)
        {
            var role = _currentUser.Role;
            var privileged = role == Domain.Constants.RoleTypes.Admin || role == Domain.Constants.RoleTypes.Security;
            if (!privileged && permission.UserId != _currentUser.UserId)
                throw new UnauthorizedAccessException("You can only access your own entry permissions.");
        }

        private static void EnsureValidWindow(DateTime validFrom, DateTime? validTo)
        {
            if (validTo.HasValue && validTo.Value <= validFrom)
                throw new BusinessRuleException("La fecha 'ValidTo' debe ser posterior a 'ValidFrom'.");
        }

        private static void EnsureNotCancelled(PermissionEntity permission)
        {
            if (permission.EntryPermissionState == PermissionStatus.Cancelled || permission.IsDeleted)
                throw new ConflictException("El permiso asociado a este QR se encuentra cancelado o dado de baja.");
        }

        private static void EnsureWithinWindow(PermissionEntity permission)
        {
            var now = DateTime.UtcNow;

            if (now < permission.ValidFrom)
                throw new BusinessRuleException($"El permiso todavía no está vigente. Habilitado desde {permission.ValidFrom:yyyy-MM-dd HH:mm} UTC.");

            if (permission.ValidTo.HasValue && now > permission.ValidTo.Value)
                throw new BusinessRuleException("El permiso ha caducado. La fecha límite de uso ha pasado.");
        }
    }
}

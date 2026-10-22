using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using PermissionEntity = CountrySecure.Domain.Entities.EntryPermission;

namespace CountrySecure.Application.Services.AccessControl
{
    /// <summary>
    /// Flujo de guardia: escanear el QR de un pase, validarlo y registrar el ingreso o la salida.
    /// Cada escaneo (aprobado o rechazado) queda en la tabla de auditoría <see cref="AccessLog"/>.
    /// </summary>
    public class AccessControlService : IAccessControlService
    {
        private const int TokenPrefixLength = 12;

        private readonly IEntryPermissionRepository _permissions;
        private readonly IAccessLogRepository _logs;
        private readonly INotificationRepository _notifications;
        private readonly IPassTokenService _tokens;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public AccessControlService(
            IEntryPermissionRepository permissions,
            IAccessLogRepository logs,
            INotificationRepository notifications,
            IPassTokenService tokens,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _permissions = permissions;
            _logs = logs;
            _notifications = notifications;
            _tokens = tokens;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        // -------------------------------------------------------------------
        // ESCANEO
        // -------------------------------------------------------------------

        public async Task<ScanResultDto> ScanAsync(ScanRequestDto dto, Guid guardId)
        {
            var token = dto.Token.Trim();
            var now = ResolveScanTime(dto);

            // 1. Existencia: la firma es válida y el token pertenece a un permiso registrado
            var (permission, tokenIsForged) = await FindPermissionAsync(token);
            if (tokenIsForged)
                return await RejectAsync(null, null, token, guardId, now, dto, ScanReason.InvalidToken,
                    "Código QR inválido: la firma no es correcta.");
            if (permission == null)
                return await RejectAsync(null, null, token, guardId, now, dto, ScanReason.NotFound,
                    "El pase no existe en el sistema.");

            var pass = await BuildPassAsync(permission);

            // 2. Estado: cancelado, ya utilizado o ya adentro
            if (permission.IsDeleted || permission.EntryPermissionState == PermissionStatus.Cancelled)
                return await RejectAsync(permission, pass, token, guardId, now, dto, ScanReason.Cancelled,
                    "Pase cancelado por el residente.");

            var single = permission.PassType == PassType.SingleEntry;
            var inside = permission.EntryTime != null && permission.DepartureTime == null;

            if (dto.Action == ScanAction.CheckOut)
            {
                if (!inside)
                    return await RejectAsync(permission, pass, token, guardId, now, dto, ScanReason.NotInside,
                        "El visitante no tiene un ingreso abierto: no registró entrada o ya salió.");

                permission.DepartureTime = now;
                permission.CheckOutGuardId = guardId;
                permission.Touch(guardId.ToString());
                return await ApproveAsync(permission, pass, token, guardId, now, dto, ScanOutcome.CheckOut,
                    "Salida registrada.");
            }

            if (single && permission.EntryTime != null)
                return await RejectAsync(permission, pass, token, guardId, now, dto, ScanReason.AlreadyUsed,
                    "Invitación ya utilizada.");

            if (inside)
                return await RejectAsync(permission, pass, token, guardId, now, dto, ScanReason.AlreadyInside,
                    "El visitante ya se encuentra dentro del barrio.");

            // 3. Ventana temporal
            if (now < permission.ValidFrom)
                return await RejectAsync(permission, pass, token, guardId, now, dto, ScanReason.NotYetValid,
                    $"Fuera de horario permitido: el pase es válido desde {permission.ValidFrom:dd/MM/yyyy HH:mm} UTC.");

            if (permission.ValidTo.HasValue && now > permission.ValidTo.Value)
                return await RejectAsync(permission, pass, token, guardId, now, dto, ScanReason.Expired,
                    "Fuera de horario permitido: el pase venció.");

            // 4. Restricciones: lista negra
            if (permission.Visit?.IsBlocked == true)
                return await RejectAsync(permission, pass, token, guardId, now, dto, ScanReason.Blocked,
                    "Visitante bloqueado. No se permite el ingreso.");

            if (permission.User?.IsBlocked == true)
                return await RejectAsync(permission, pass, token, guardId, now, dto, ScanReason.Blocked,
                    "El residente titular está bloqueado. No se permite el ingreso.");

            // Aprobado: el pase de un solo ingreso queda utilizado; el de varios sigue vigente
            permission.EntryTime = now;
            permission.DepartureTime = null;
            permission.CheckInGuardId = guardId;
            permission.CheckOutGuardId = null;
            permission.EntryPermissionState = single ? PermissionStatus.Completed : PermissionStatus.Pending;
            permission.Touch(guardId.ToString());

            return await ApproveAsync(permission, pass, token, guardId, now, dto, ScanOutcome.CheckIn,
                "Ingreso autorizado.");
        }

        // -------------------------------------------------------------------
        // PASES
        // -------------------------------------------------------------------

        public async Task<PassDto> GetPassAsync(Guid permissionId)
        {
            var permission = await _permissions.GetWithDetailsAsync(permissionId)
                             ?? throw new KeyNotFoundException($"No se encontró el permiso de ingreso {permissionId}.");

            var privileged = _currentUser.Role == Domain.Constants.RoleTypes.Admin
                             || _currentUser.Role == Domain.Constants.RoleTypes.Security;
            if (!privileged && permission.UserId != _currentUser.UserId)
                throw new UnauthorizedAccessException("Solo podés ver tus propios permisos de ingreso.");

            return await BuildPassAsync(permission);
        }

        public async Task<PassDto> GetPublicPassAsync(string token)
        {
            // Un token mal formado, falsificado o de un pase cancelado responde igual: no existe
            var (permission, forged) = await FindPermissionAsync(token.Trim());
            if (forged || permission == null || permission.IsDeleted)
                throw new KeyNotFoundException("Pase no encontrado.");

            return await BuildPassAsync(permission);
        }

        public async Task<SyncResponseDto> GetSyncAsync()
        {
            var now = DateTime.UtcNow;
            // Pases que pueden usarse desde hace 2 horas hasta dentro de 24 horas
            var permissions = await _permissions.GetActiveForSyncAsync(now.AddHours(-2), now.AddHours(24), max: 1000);
            var destinations = await _permissions.GetDestinationsAsync(permissions.Select(p => p.UserId).Distinct().ToList());

            var items = permissions.Select(p =>
            {
                var pass = ToPass(p, destinations.TryGetValue(p.UserId, out var d) ? d : []);
                return new SyncPassDto
                {
                    PermissionId = pass.PermissionId, Token = pass.Token, Status = pass.Status, PassType = pass.PassType,
                    ResidentName = pass.ResidentName, Destination = pass.Destination, VisitorName = pass.VisitorName,
                    VisitorDni = pass.VisitorDni, CompanionsCount = pass.CompanionsCount, VehicleType = pass.VehicleType,
                    VehiclePlate = pass.VehiclePlate, VehicleModel = pass.VehicleModel, ValidFrom = pass.ValidFrom,
                    ValidTo = pass.ValidTo, Description = pass.Description,
                    EntryTime = p.EntryTime, DepartureTime = p.DepartureTime,
                    Blocked = p.Visit?.IsBlocked == true || p.User?.IsBlocked == true
                };
            }).ToList();

            return new SyncResponseDto { GeneratedAt = now, Items = items };
        }

        public async Task<PagedResult<AccessLogResponseDto>> GetLogsAsync(
            PageQuery paging, Guid? permissionId, ScanOutcome? outcome, DateTime? from, DateTime? to)
        {
            var page = await _logs.GetPagedAsync(paging, permissionId, outcome, from, to);
            return page.Map(l => new AccessLogResponseDto
            {
                Id = l.Id,
                PermissionId = l.PermissionId,
                GuardId = l.GuardId,
                GuardName = l.Guard != null ? $"{l.Guard.Name} {l.Guard.Lastname}" : "(guardia eliminado)",
                VisitorName = l.Permission?.Visit != null ? $"{l.Permission.Visit.NameVisit} {l.Permission.Visit.LastNameVisit}" : null,
                Outcome = l.Outcome,
                Reason = l.Reason,
                Message = l.Message,
                ScannedAt = l.ScannedAt,
                Offline = l.Offline
            });
        }

        // -------------------------------------------------------------------
        // AUXILIARES
        // -------------------------------------------------------------------

        /// <summary>
        /// Los pases nuevos llevan "id.firma": se verifica la firma antes de tocar la base.
        /// Los pases emitidos antes de las firmas (un GUID) se buscan por su valor.
        /// </summary>
        private async Task<(PermissionEntity? Permission, bool Forged)> FindPermissionAsync(string token)
        {
            if (token.Contains('.'))
            {
                if (!_tokens.TryValidate(token, out var id))
                    return (null, true);

                var signed = await _permissions.GetWithDetailsAsync(id, includeDeleted: true);
                // Un token con firma válida pero distinto al emitido (p. ej. de un permiso regenerado) no sirve
                return (signed != null && signed.QrCodeValue == token ? signed : null, false);
            }

            return (await _permissions.GetByQrCodeAsync(token, includeDeleted: true), false);
        }

        private async Task<PassDto> BuildPassAsync(PermissionEntity permission)
            => ToPass(permission, await _permissions.GetDestinationsAsync(permission.UserId));

        private static PassDto ToPass(PermissionEntity permission, IReadOnlyCollection<string> destinations)
        {
            return new PassDto
            {
                PermissionId = permission.Id,
                Token = permission.QrCodeValue,
                Status = permission.EntryPermissionState,
                PassType = permission.PassType,
                ResidentName = permission.User != null ? $"{permission.User.Name} {permission.User.Lastname}" : "(residente eliminado)",
                Destination = destinations.Count > 0 ? string.Join(" / ", destinations) : "Sin domicilio asignado",
                VisitorName = permission.Visit != null ? $"{permission.Visit.NameVisit} {permission.Visit.LastNameVisit}" : "(visitante eliminado)",
                VisitorDni = permission.Visit?.DniVisit ?? 0,
                CompanionsCount = permission.CompanionsCount,
                VehicleType = permission.VehicleType,
                VehiclePlate = permission.VehiclePlate,
                VehicleModel = permission.VehicleModel,
                ValidFrom = permission.ValidFrom,
                ValidTo = permission.ValidTo,
                Description = permission.Description
            };
        }

        /// <summary>Un escaneo offline conserva la hora real de la guardia (nunca una hora futura).</summary>
        private static DateTime ResolveScanTime(ScanRequestDto dto)
        {
            var now = DateTime.UtcNow;
            if (!dto.Offline || !dto.ScannedAt.HasValue) return now;

            var at = dto.ScannedAt.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dto.ScannedAt.Value, DateTimeKind.Utc)
                : dto.ScannedAt.Value.ToUniversalTime();
            return at > now ? now : at;
        }

        private async Task<ScanResultDto> ApproveAsync(
            PermissionEntity permission, PassDto pass, string token, Guid guardId, DateTime at, ScanRequestDto dto,
            ScanOutcome outcome, string message)
        {
            await _permissions.UpdateAsync(permission);
            await _logs.AddAsync(NewLog(permission.Id, token, guardId, at, dto.Offline, outcome, ScanReason.None, message));

            // Aviso en tiempo real al residente titular (lo ve en la campana del panel)
            var visitor = pass.VisitorName;
            await _notifications.AddAsync(new Notification
            {
                UserId = permission.UserId,
                PermissionId = permission.Id,
                Type = outcome == ScanOutcome.CheckIn ? "VisitCheckIn" : "VisitCheckOut",
                Title = outcome == ScanOutcome.CheckIn ? "Tu visita ingresó" : "Tu visita salió",
                Message = outcome == ScanOutcome.CheckIn
                    ? $"Tu visita {visitor} ha ingresado al barrio."
                    : $"Tu visita {visitor} salió del barrio.",
                CreatedBy = guardId.ToString()
            });
            await _unitOfWork.SaveChangesAsync();

            pass.Status = permission.EntryPermissionState;
            return new ScanResultDto { Success = true, Outcome = outcome, Reason = ScanReason.None, Message = message, ScannedAt = at, Pass = pass };
        }

        private async Task<ScanResultDto> RejectAsync(
            PermissionEntity? permission, PassDto? pass, string token, Guid guardId, DateTime at, ScanRequestDto dto,
            ScanReason reason, string message)
        {
            await _logs.AddAsync(NewLog(permission?.Id, token, guardId, at, dto.Offline, ScanOutcome.Rejected, reason, message));
            await _unitOfWork.SaveChangesAsync();

            return new ScanResultDto { Success = false, Outcome = ScanOutcome.Rejected, Reason = reason, Message = message, ScannedAt = at, Pass = pass };
        }

        private static AccessLog NewLog(
            Guid? permissionId, string token, Guid guardId, DateTime at, bool offline,
            ScanOutcome outcome, ScanReason reason, string message) => new()
        {
            PermissionId = permissionId,
            GuardId = guardId,
            Outcome = outcome,
            Reason = reason,
            Message = message,
            ScannedAt = at,
            Offline = offline,
            // Solo el comienzo del token: suficiente para rastrear intentos sin guardar el secreto completo
            TokenPrefix = token.Length > TokenPrefixLength ? token[..TokenPrefixLength] : token,
            CreatedBy = guardId.ToString()
        };
    }
}

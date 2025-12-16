using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers; 
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Services.EntryPermission
{
    public class EntryPermissionService : IEntryPermissionService
    {
        private readonly IEntryPermissionRepository _entryPermissionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EntryPermissionService(IEntryPermissionRepository entryPermissionRepository, IUnitOfWork unitOfWork)
        {
            _entryPermissionRepository = entryPermissionRepository;
            _unitOfWork = unitOfWork;
        }

        // -------------------------------------------------------------------
        // MÉTODOS DE ESCRITURA
        // -------------------------------------------------------------------

        public async Task<EntryPermissionResponseDto> AddNewEntryPermissionAsync(CreateEntryPermissionDto dto, Guid currentUserId)
        {
            var entity = dto.ToEntity();

            entity.QrCodeValue = Guid.NewGuid().ToString();
            entity.CreatedBy = currentUserId.ToString();
            entity.CreatedAt = DateTime.UtcNow;
            entity.LastModifiedAt = DateTime.UtcNow;

            entity.EntryPermissionState = PermissionStatus.Pending;
            entity.EntryTime = null;
            entity.DepartureTime = null;

            await _entryPermissionRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var full = await _entryPermissionRepository.GetEntryPermissionWithDetailsAsync(entity.Id);
            return full!.ToResponseDto();
        }


        public async Task<EntryPermissionResponseDto?> UpdateEntryPermissionAsync(UpdateEntryPermissionDto dto, Guid id, Guid currentUserId)
        {
            var entity = await _entryPermissionRepository.GetByIdAsync(id);
            if (entity == null) return null;

            dto.MapToEntity(entity);

            // --- LÓGICA DE NEGOCIO ---

            // 1. Si se setea la hora de ingreso
            if (dto.EntryTime.HasValue && entity.EntryPermissionState == PermissionStatus.Pending)
            {
                entity.EntryPermissionState = PermissionStatus.Completed;
            }

            // 2. Si se setea la hora de salida
            if (dto.DepartureTime.HasValue)
            {
                entity.EntryPermissionState = PermissionStatus.Completed;
            }

            // 3. Verificar expiración
            if (DateTime.UtcNow > entity.ValidFrom && entity.EntryPermissionState == PermissionStatus.Pending)
            {
                entity.EntryPermissionState = PermissionStatus.Expired;
            }

            // Auditoría
            entity.LastModifiedAt = DateTime.UtcNow;
            entity.LastModifiedBy = currentUserId.ToString();

            await _entryPermissionRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var full = await _entryPermissionRepository.GetByIdWithIncludesAsync(id);
            return full!.ToResponseDto();
        }


        public async Task<EntryPermissionResponseDto?> SoftDeleteEntryPermissionAsync(Guid entryPermissionId, Guid currentUserId)
        {
            
            var entity = await _entryPermissionRepository.SoftDeleteToggleAsync(entryPermissionId);

            if (entity == null)
                return null; // No se encontró

            // 2. Aplicar Auditoría:
            entity.LastModifiedAt = DateTime.UtcNow;
            entity.LastModifiedBy = currentUserId.ToString();

            
            if (entity.Status == "Inactive")
            {
                
                entity.EntryPermissionState = PermissionStatus.Cancelled; 

                
            }
            else
            {
                // Si se acaba de reactivar, debería volver al estado inicial (Pending)
                entity.EntryPermissionState = PermissionStatus.Pending; // Usamos el enum para el estado funcional si existe
                                                         
            }

            // 4. Persistencia
            // Usamos UpdateAsync para guardar los cambios de auditoría y PermissionStatus.
            var updatedEntity = await _entryPermissionRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // 5. Devolver el DTO con los detalles
            // Necesitamos recargar con los includes si el DTO lo requiere (User/Visit)
            var fullPermission = await _entryPermissionRepository.GetEntryPermissionWithDetailsAsync(updatedEntity.Id);

            return fullPermission?.ToResponseDto();
        }


        public async Task<GateCheckResponseDto> ValidateQrCodeAsync(string qrCode)
        {
            // 1. Buscar la entidad con navegación incluida (User y Visit)
            var entity = await _entryPermissionRepository.GetByQrCodeValueAsync(qrCode);

            // 2. Manejo de Not Found
            if (entity == null)
                throw new KeyNotFoundException("QR Code no encontrado en el sistema.");

            // 3. Validar si está cancelado
            if ( entity.Status == "Cancelled")
                throw new InvalidOperationException("El permiso asociado a este QR se encuentra cancelado o dado de baja.");

            // 4. Validar expiración (si aplica)
            if (entity.ValidTo.HasValue && entity.ValidTo.Value < DateTime.UtcNow)
                throw new InvalidOperationException("El QR ha caducado. La fecha límite de uso ha pasado.");

            // 5. Validar si ya se registró entrada y salida (Luz Roja)
            if (entity.EntryTime != null && entity.DepartureTime != null)
                throw new InvalidOperationException("El visitante ya registró ingreso y salida. El permiso está consumido.");

            // 6. Si ya ingresó (EntryTime != null) pero AÚN no salió (DepartureTime == null) -> MARCAR SALIDA (Luz Amarilla)
            if (entity.EntryTime != null && entity.DepartureTime == null)
            {
                return new GateCheckResponseDto
                {
                    PermissionId = entity.Id,
                    VisitorFullName = entity.Visit != null ? $"{entity.Visit.NameVisit} {entity.Visit.LastNameVisit}" : string.Empty,
                    ResidentFullName = entity.User != null ? $"{entity.User.Name} {entity.User.Lastname}" : string.Empty,
                    VisitorDni = entity.Visit?.DniVisit ?? 0,
                    EntryTime = entity.EntryTime,
                    DepartureTime = null,
                    CheckResultStatus = "Dentro - Marcar Salida", // Estado clave para el Front-end
                    Message = "Visitante actualmente dentro. Datos corroborados. Proceder a registrar SALIDA."
                };
            }

            // 7. Válido y listo para ingresar (EntryTime == null) -> MARCAR ENTRADA (Luz Verde)
            return new GateCheckResponseDto
            {
                PermissionId = entity.Id,
                VisitorFullName = entity.Visit != null ? $"{entity.Visit.NameVisit} {entity.Visit.LastNameVisit}" : string.Empty,
                ResidentFullName = entity.User != null ? $"{entity.User.Name} {entity.User.Lastname}" : string.Empty,
                VisitorDni = entity.Visit?.DniVisit ?? 0,
                EntryTime = null,
                DepartureTime = null,
                CheckResultStatus = "Autorizado - Marcar Entrada", // Estado clave para el Front-end
                Message = "Permiso Válido. Datos corroborados. Proceder a registrar ENTRADA."
            };
        }

        // -------------------------------------------------------------------
        // MÉTODOS DE LECTURA
        // -------------------------------------------------------------------

        public async Task<EntryPermissionResponseDto?> GetEntryPermissionByIdAsync(Guid entryPermissionId)
        {
            var entity = await _entryPermissionRepository.GetByIdWithIncludesAsync(entryPermissionId);

            if (entity == null) return null;

            return entity.ToResponseDto(); 
        }

        public async Task<IEnumerable<EntryPermissionResponseDto>> GetAllEntryPermissionsAsync(int pageNumber, int pageSize)
        {
           
            var entities = await _entryPermissionRepository.GetAllWithIncludesAsync(pageNumber, pageSize);


            return entities.ToResponseDto(); 
        }

        public async Task<IEnumerable<EntryPermissionResponseDto>> GetEntryPermissionsByUserIdAsync(Guid userId)
        {
            var entities = await _entryPermissionRepository.GetEntryPermissionsByUserIdAsync(userId);
            return entities.ToResponseDto();
        }

        public async Task<IEnumerable<EntryPermissionResponseDto>> GetEntryPermissionsByServiceIdAsync(Guid serviceId)
        {
            var entities = await _entryPermissionRepository.GetEntryPermissionsByServiceIdAsync(serviceId);
            return entities.ToResponseDto();
        }

        public async Task<IEnumerable<EntryPermissionResponseDto>> GetEntryPermissionsByVisitIdAsync(Guid visitId)
        {
            
            var entities = await _entryPermissionRepository.GetEntryPermissionsByVisitIdAsync(visitId);
            return entities.ToResponseDto();
        }

        public async Task<IEnumerable<EntryPermissionResponseDto>> GetEntryPermissionsByTypeAsync(PermissionType permissionType, int pageNumber, int pageSize)
        {
            
            var entities = await _entryPermissionRepository.GetEntryPermissionsByTypeAsync(permissionType, pageNumber, pageSize);
            return entities.ToResponseDto();
        }

        public async Task<IEnumerable<EntryPermissionResponseDto>> GetEntryPermissionsByStatusAsync(string status, int pageNumber, int pageSize)
        {
            var entities = await _entryPermissionRepository.GetEntryPermissionsStatusAsync(status, pageNumber, pageSize);
            return entities.ToResponseDto();
        }
        public async Task<PaginatedResult<EntryPermissionResponseDto>> GetActivePermissionsForDateAsync(
            DateTime date,
            int pageNumber,
            int pageSize)
        {
            // 1. Sanitización básica de parámetros
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize > 0 ? pageSize : 100;

            // AVISO: Eliminamos startOfDay/endOfDay para no confundir con el filtro interno.
            // Usamos la fecha original del Controller.
            var dateFilter = date.Date;

            // 2. Llamada al repositorio (Obtenemos datos paginados y el TotalCount)
            var paginatedEntities = await _entryPermissionRepository.GetByDateRangeWithDetailsAsync(
                dateFilter, // Usamos dateFilter como inicio
                dateFilter, // Usamos dateFilter como fin (para el rango de un día)
                pageNumber,
                pageSize
            );

            // 3. Mapear SOLO los ítems de la página actual
            var mappedItems = paginatedEntities.Items.ToResponseDto();

            // 4. Devolver el resultado paginado con el mapeo y el conteo total
            return new PaginatedResult<EntryPermissionResponseDto>(mappedItems, paginatedEntities.TotalCount);
        }

        public async Task<EntryPermissionResponseDto> RegisterCheckInAsync(Guid permissionId, Guid currentUserId)
        {
            var entity = await _entryPermissionRepository.GetByIdWithIncludesAsync(permissionId);

            if (entity == null)
                throw new KeyNotFoundException($"Permiso de Entrada con ID {permissionId} no encontrado.");

            if (entity.EntryPermissionState != PermissionStatus.Pending)
                throw new InvalidOperationException($"El permiso ID {permissionId} ya tiene un registro de entrada o está cancelado.");
            // ⛔ VALIDACIÓN CRÍTICA
            if (entity.ValidTo.HasValue && entity.ValidTo.Value < DateTime.UtcNow)
                throw new InvalidOperationException(
                    "El permiso está vencido y no permite check-in."
                );


            // Aplicar Check-In
            entity.EntryTime = DateTime.UtcNow;
            entity.EntryPermissionState = PermissionStatus.Completed;


            entity.CheckInGuardId = currentUserId;

            // Auditoría
            entity.LastModifiedAt = DateTime.UtcNow;
            entity.LastModifiedBy = currentUserId.ToString();

            await _entryPermissionRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // El DTO se generará con los datos de auditoría del guardia (CheckInGuardId)
            return entity.ToResponseDto();
        }

        public async Task<EntryPermissionResponseDto> RegisterCheckOutAsync(Guid permissionId, Guid currentUserId)
        {
            var entity = await _entryPermissionRepository.GetByIdWithIncludesAsync(permissionId);

            if (entity == null)
                throw new KeyNotFoundException($"Permiso de Entrada con ID {permissionId} no encontrado.");

            if (entity.EntryTime == null)
                throw new InvalidOperationException($"No se puede registrar la salida del permiso ID {permissionId}. La entrada no fue registrada.");

            if (entity.DepartureTime.HasValue)
                throw new InvalidOperationException($"La salida del permiso ID {permissionId} ya fue registrada.");

            // Aplicar Check-Out
            entity.DepartureTime = DateTime.UtcNow;

            
            entity.CheckOutGuardId = currentUserId;

            // Auditoría
            entity.LastModifiedAt = DateTime.UtcNow;
            entity.LastModifiedBy = currentUserId.ToString();

            await _entryPermissionRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToResponseDto();
        }

        public async Task<IEnumerable<EntryPermissionResponseDto>> GetEntryLogsAsync(
    int pageNumber,
    int pageSize,
    string? search,
    string? type
)
        {
            var entities = await _entryPermissionRepository
                .GetAllHistoryWithDetailsAsync(pageNumber, pageSize, search, type);

            return entities.ToResponseDto();
        }



    }
}
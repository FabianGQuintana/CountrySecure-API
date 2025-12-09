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
            // 1. Mapeo DTO a Entidad
            var newPermissionEntity = dto.ToEntity();

            // 2. GENERACIÓN DE VALOR CRÍTICO
            // El campo QrCodeValue se genera en el servidor
            newPermissionEntity.QrCodeValue = Guid.NewGuid().ToString();

            // 3. Asignación de Auditoría y Estado Inicial

            // a. Creador
            newPermissionEntity.CreatedBy = currentUserId.ToString();

            // b. Fechas de Creación/Modificación (Auditoría)
            newPermissionEntity.CreatedAt = DateTime.UtcNow;
            newPermissionEntity.LastModifiedAt = DateTime.UtcNow;

            // c. Estado Funcional (Defensivo)
            newPermissionEntity.Status = PermissionStatus.Pending;

            // 4. Guardar en Repositorio (Solo inserta la fila, NO carga User/Visit)
            var addedPermission = await _entryPermissionRepository.AddAsync(newPermissionEntity);
            await _unitOfWork.SaveChangesAsync();

            //Obtenemos la entidad Completa para que no lance error.
            var fullPermission = await _entryPermissionRepository.GetEntryPermissionWithDetailsAsync(addedPermission.Id);

            // Verificación defensiva (aunque no debería ser nulo)
            if (fullPermission == null)
            {
                throw new InvalidOperationException("Permission was created but could not be retrieved for mapping.");
            }

            // 6. Mapeo de Entidad Completa a DTO de Respuesta
            return fullPermission.ToResponseDto();
        }

        public async Task<EntryPermissionResponseDto?> UpdateEntryPermissionAsync(UpdateEntryPermissionDto dto, Guid entryPermissionId, Guid currentUserId)
        {
            var existingEntity = await _entryPermissionRepository.GetByIdAsync(entryPermissionId);

            if (existingEntity == null)
            {
                // Devolvemos NULL para que el controlador retorne 404 Not Found
                return null;
            }

            if (existingEntity.CreatedBy != currentUserId.ToString())
            {
                throw new UnauthorizedAccessException("User is not authorized to update this EntryPermission. Only the creator may modify it.");
            }

            dto.MapToEntity(existingEntity);

           
            existingEntity.LastModifiedAt = DateTime.UtcNow;
            existingEntity.LastModifiedBy = currentUserId.ToString();

            
            var updatedEntity = await _entryPermissionRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();

            // NOTA: Para que ToResponseDto funcione correctamente, la entidad updatedEntity 
            // debe tener sus propiedades de navegación (User, Visit) cargadas. 
            // Si no lo están, la llamada al repositorio debe usar Eager Loading (Include()).
            return updatedEntity.ToResponseDto();
        }

        public async Task<bool> SoftDeleteEntryPermissionAsync(Guid entryPermissionId)
        {
            // 1. La eliminación se maneja en el Repositorio (marcando IsDeleted/DeletedAt)
            bool marked = await _entryPermissionRepository.DeleteAsync(entryPermissionId);

            if (marked)
            {
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            // Devuelve false si no se encontró la entidad con el ID
            return false;
        }


        public async Task<GateCheckResponseDto> ValidateQrCodeAsync(string qrCode)
        {
            // 1. Buscar la entidad con las propiedades de navegación cargadas (User, Visit)
            var entity = await _entryPermissionRepository.GetByQrCodeValueAsync(qrCode);

            // --- Manejo de Not Found ---
            if (entity == null)
            {
                throw new KeyNotFoundException("QR Code not found in the system.");
            }

            // --- Validaciones de Reglas de Negocio ---
            if (entity.Status != PermissionStatus.Pending)
            {
                // Si el estado es "Used", "Expired", etc.
                return new GateCheckResponseDto
                {
                    PermissionId = entity.Id,
                    VisitorFullName = entity.Visit != null
                        ? $"{entity.Visit.NameVisit} {entity.Visit.LastNameVisit}"
                        : string.Empty,
                    ResidentFullName = entity.User != null
                        ? $"{entity.User.Name} {entity.User.Lastname}"
                        : string.Empty,
                    VisitorDni = entity.Visit?.DniVisit ?? 0,

                    // Mapeo correcto según tu enum
                    CheckResultStatus = entity.Status switch
                    {
                        PermissionStatus.Completed => "Permiso Completado",
                        PermissionStatus.Expirado => "Permiso Expirado",
                        _ => "Permiso Inactivo"
                    },

                    Message = "El permiso ya fue consumido, utilizado o está inactivo."
                };
            }

            // --- Lógica de Uso Único (Si la validación es exitosa) ---

            // 2. Cambiar el estado a ENTERED (o el que corresponda)
            entity.Status = PermissionStatus.Completed;
            entity.LastModifiedAt = DateTime.UtcNow;

            // 3. Persistir el cambio de estado
            await _entryPermissionRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // 4. Mapear y devolver el DTO de ÉXITO
            return new GateCheckResponseDto
            {
                PermissionId = entity.Id,
                VisitorFullName = entity.Visit != null
                    ? $"{entity.Visit.NameVisit} {entity.Visit.LastNameVisit}"
                    : string.Empty,
                ResidentFullName = entity.User != null
                    ? $"{entity.User.Name} {entity.User.Lastname}"
                    : string.Empty,
                VisitorDni = entity.Visit?.DniVisit ?? 0,

                CheckResultStatus = "Válido y Consumido",
                Message = "Acceso Autorizado. Datos corroborados."
            };
        }



        // -------------------------------------------------------------------
        // MÉTODOS DE LECTURA
        // -------------------------------------------------------------------

        public async Task<EntryPermissionResponseDto?> GetEntryPermissionByIdAsync(Guid entryPermissionId)
        {
            var entity = await _entryPermissionRepository.GetByIdAsync(entryPermissionId);

            if (entity == null) return null;

            // Mapeo de Entidad a DTO de Respuesta
            return entity.ToResponseDto();
        }

        public async Task<IEnumerable<EntryPermissionResponseDto>> GetAllEntryPermissionsAsync(int pageNumber , int pageSize )
        {
            var entities = await _entryPermissionRepository.GetAllAsync(pageNumber, pageSize);
            // Mapeamos la colección a DTOs
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
    }
}
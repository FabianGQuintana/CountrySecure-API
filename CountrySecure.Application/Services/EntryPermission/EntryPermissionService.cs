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

            // Generar un valor único para el código QR
            newPermissionEntity.QrCodeValue = Guid.NewGuid().ToString();

            // 2. Asignación de Auditoría y Estado Inicial
            newPermissionEntity.CreatedBy = currentUserId.ToString();
            newPermissionEntity.Status = PermissionStatus.Pending;
            newPermissionEntity.CreatedAt = DateTime.UtcNow;

            // 3. Guardar en Repositorio
            var addedPermission = await _entryPermissionRepository.AddAsync(newPermissionEntity);
            await _unitOfWork.SaveChangesAsync();

            // 4. Mapeo de Entidad a DTO de Respuesta
            return addedPermission.ToResponseDto();
        }

        public async Task<EntryPermissionResponseDto?> UpdateEntryPermissionAsync(UpdateEntryPermissionDto dto, Guid entryPermissionId, Guid currentUserId)
        {
            // 1. Buscar y Validar existencia
            var existingEntity = await _entryPermissionRepository.GetByIdAsync(entryPermissionId);

            if (existingEntity == null)
            {
                // Devolvemos NULL para que el controlador retorne 404 Not Found
                return null;
            }

            // 2. Aplicar la lógica de negocio de autorización (Ej: Solo el creador puede actualizar)
             if (existingEntity.CreatedBy != currentUserId.ToString())
            {
                throw new UnauthorizedAccessException("User is not authorized to update this EntryPermission. Only the creator may modify it.");
            }
           
            // 3. Aplicar cambios del DTO a la Entidad existente
            dto.MapToEntity(existingEntity);

            // 4. Actualizar Auditoría
            existingEntity.LastModifiedAt = DateTime.UtcNow;
           

            // 5. Guardar Cambios
            var updatedEntity = await _entryPermissionRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();

            // 6. Mapeo de Entidad a DTO de Respuesta
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
                throw new KeyNotFoundException("QR Code not found in the system.");

            // --- Validaciones de Reglas de Negocio ---
            if (entity.Status != PermissionStatus.Pending)
            {
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
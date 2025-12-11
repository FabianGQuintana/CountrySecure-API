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

            // 3. Validar si está dado de baja
            if (entity.Status == "Baja")
                throw new InvalidOperationException("El permiso asociado a este QR se encuentra dado de baja.");

            // 4. Validar expiración (si aplica)
            if (entity.ValidFrom < DateTime.UtcNow)
                throw new InvalidOperationException("El QR ha expirado.");

            // 5. Validar si ya se registró entrada y salida
            var visit = entity.Visit;

            if (visit != null)
            {
                // ya ingresó y salió
                if (entity.EntryTime != null && entity.DepartureTime != null)
                    throw new InvalidOperationException("El visitante ya registró ingreso y salida.");

                // ya ingresó pero aún no salió (puede usarse para marcar salida)
                if (entity.EntryTime != null && entity.DepartureTime == null)
                {
                    return new GateCheckResponseDto
                    {
                        PermissionId = entity.Id,
                        VisitorFullName = entity.Visit != null? $"{entity.Visit.NameVisit} {entity.Visit.LastNameVisit}": string.Empty,
                        ResidentFullName = entity.User != null ? $"{entity.User.Name} {entity.User.Lastname}": string.Empty,
                        VisitorDni = entity.Visit?.DniVisit ?? 0,
                        EntryTime = entity.EntryTime,
                        DepartureTime = entity?.DepartureTime,
                        CheckResultStatus = "Válido y Consumido",
                        Message = "Acceso Autorizado. Datos corroborados."
                    };
                }
            }

            // 6. Si nunca ingresó → puede entrar
            return new GateCheckResponseDto
            {
                PermissionId = entity.Id,
                VisitorFullName = entity.Visit != null ? $"{entity.Visit.NameVisit} {entity.Visit.LastNameVisit}" : string.Empty,
                ResidentFullName = entity.User != null ? $"{entity.User.Name} {entity.User.Lastname}" : string.Empty,
                VisitorDni = entity.Visit?.DniVisit ?? 0,
                CheckResultStatus = "Válido y Sin Consumido",
                Message = "QR válido. El visitante puede registrar su entrada.",
                EntryTime = entity?.EntryTime,
                DepartureTime = entity?.DepartureTime
            };
        }
            // 4. Mapear y devolver el DTO de ÉXITO
            



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


    }
}
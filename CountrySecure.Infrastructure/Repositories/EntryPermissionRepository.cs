using CountrySecure.Domain.Entities;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CountrySecure.Domain.Enums;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;


namespace CountrySecure.Infrastructure.Repositories
{
    public class EntryPermissionRepository : GenericRepository<EntryPermission>, IEntryPermissionRepository
    {
        private readonly CountrySecureDbContext _dbContext;

        public EntryPermissionRepository(CountrySecureDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        //Obtener permiso de entrada por valor de código QR.
        public async Task<EntryPermission?> GetEntryPermissionByQrCodeValueAsync(string qrCodeValue)
        {
            return await _dbContext.EntryPermissions
                .FirstOrDefaultAsync(ep => ep.QrCodeValue == qrCodeValue);
        }

        //Obtener todos los permisos de entrada asociados a un usuario,visita,servicio específico.
        public async Task<IEnumerable<EntryPermission>> GetEntryPermissionsByUserIdAsync(Guid userId)
        {
            return await _dbContext.EntryPermissions
                .Where(ep => ep.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<EntryPermission>> GetEntryPermissionsByVisitIdAsync(Guid visitId)
        {
            return await _dbContext.EntryPermissions
                .Where(ep => ep.VisitId == visitId)
                .ToListAsync();
        }

        public async Task<IEnumerable<EntryPermission>> GetEntryPermissionsByServiceIdAsync(Guid OrderId)
        {
            return await _dbContext.EntryPermissions
                .Where(ep => ep.OrderId == OrderId)
                .ToListAsync();
        }


        //Obtener todos los permisos en base al tipo de permiso.
        public async Task<IEnumerable<EntryPermission>> GetEntryPermissionsByTypeAsync(PermissionType permissionType, int pageNumber, int pageSize)
        {
            return await _dbContext.EntryPermissions
                .Where(ep => ((int)ep.PermissionType) == ((int)permissionType))
                .OrderBy(ep => ep.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        //Obtener todos los permisos en base al estado (1-ALTA/0-BAJA).
        public async Task<IEnumerable<EntryPermission>> GetEntryPermissionsStatusAsync( string status, int pageNumber, int pageSize)
        {
            // Intentar convertir el string al enum PermissionStatus
            if (!Enum.TryParse<PermissionStatus>(status, true, out var statusEnum))
                throw new ArgumentException("Estado inválido", nameof(status));

            return await _dbContext.EntryPermissions
                .Where(ep => ep.Status == statusEnum)
                .OrderBy(ep => ep.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        // Obtener permiso de entrada por valor de código QR con entidades relacionadas.
        public async Task<EntryPermission?> GetByQrCodeValueAsync(string qrCodeValue)
        {
            // Es crucial usar .Include() aquí para cargar las entidades de navegación 
            // (User, Visit, Service) que son necesarias para el mapeo a EntryPermissionResponseDto.
            return await _dbContext.EntryPermissions
                                 .Include(p => p.User)    // El Residente/Creador
                                 .Include(p => p.Visit)   // El Visitante
                                  .Include(p => p.Order) // El Servicio (si aplica)
                                 .FirstOrDefaultAsync(p => p.QrCodeValue == qrCodeValue);
        }

        public async Task<EntryPermission?> GetEntryPermissionWithDetailsAsync(Guid id)
        {
            return await _dbContext.EntryPermissions
                .Include(p => p.User)    // Requerido por el mapeador
                .Include(p => p.Visit)   // Requerido por el mapeador
                .Include(p => p.Order)   // Opcional, pero bueno incluirlo si está en el DTO de respuesta
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}

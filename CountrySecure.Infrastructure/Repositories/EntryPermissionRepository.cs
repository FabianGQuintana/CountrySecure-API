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
                .Include(p => p.User)    // El Residente/Creador
                .Include(p => p.Visit)   // El Visitante
                .Include(p => p.Order)
                .FirstOrDefaultAsync(ep => ep.QrCodeValue == qrCodeValue);
        }

        //Obtener todos los permisos de entrada asociados a un usuario,visita,servicio específico.
        public async Task<IEnumerable<EntryPermission>> GetEntryPermissionsByUserIdAsync(Guid userId)
        {
            return await _dbContext.EntryPermissions
                .Include(p => p.User)    // El Residente/Creador
                .Include(p => p.Visit)   // El Visitante
                .Include(p => p.Order)
                .Where(ep => ep.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<EntryPermission>> GetEntryPermissionsByVisitIdAsync(Guid visitId)
        {
            return await _dbContext.EntryPermissions
                .Include(p => p.User)    // El Residente/Creador
                .Include(p => p.Visit)   // El Visitante
                .Include(p => p.Order)
                .Where(ep => ep.VisitId == visitId)
                .ToListAsync();
        }

        public async Task<IEnumerable<EntryPermission>> GetEntryPermissionsByServiceIdAsync(Guid OrderId)
        {
            return await _dbContext.EntryPermissions
                .Include(p => p.User)    // El Residente/Creador
                .Include(p => p.Visit)   // El Visitante
                .Include(p => p.Order)
                .Where(ep => ep.OrderId == OrderId)
                .ToListAsync();
        }


        //Obtener todos los permisos en base al tipo de permiso.
        public async Task<IEnumerable<EntryPermission>> GetEntryPermissionsByTypeAsync(PermissionType permissionType, int pageNumber, int pageSize)
        {
            return await _dbContext.EntryPermissions.Include(p => p.User).Include(p => p.Visit).Include(p => p.Order)
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

            return await _dbContext.EntryPermissions.Include(p => p.User).Include(p => p.Visit).Include(p => p.Order)
                .Where(ep => ep.EntryPermissionState == statusEnum)
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

        public async Task<EntryPermission?> GetByIdWithIncludesAsync(Guid id)
        {
            return await _dbContext.EntryPermissions
                .Include(p => p.User) // Necesario para Resident
                .Include(p => p.Visit) // Necesario para Visitor
                .Include(p => p.Order) // Necesario para Order (si no es null)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<EntryPermission>> GetAllWithIncludesAsync(int pageNumber, int pageSize)
        {
            // Usar Queryable para aplicar la paginación y luego forzar la carga ansiosa.
            return await _dbContext.EntryPermissions
                .Include(p => p.User)
                .Include(p => p.Visit)
                .Include(p => p.Order)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<PaginatedResult<EntryPermission>> GetByDateRangeWithDetailsAsync(
        DateTime startDate,
        DateTime endDate, // Este parámetro se ignora en este método simplificado para "hoy"
         int pageNumber,
        int pageSize)
        {
            // NOTA: Se asume que 'startDate' viene del Controller como el inicio del día local,
            // ya convertido a UTC (ej: 2025-12-16 00:00:00Z).

            // 1. Definir el rango del día completo en UTC.
            // Usamos el inicio del día que se pasó:
            var startOfDayUtc = startDate;
            // Calculamos el inicio del día siguiente (es el límite exclusivo superior <)
            var endOfNextDayUtc = startOfDayUtc.AddDays(1);

            // 1. Crear el IQueryable base
            var query = _dbContext.EntryPermissions
            .Include(p => p.User)
            .Include(p => p.Visit)
            .Include(p => p.Order)
        // ⚠️ CAMBIO CLAVE: Usamos un rango (>= inicio del día y < inicio del día siguiente)
        // Esto consulta el rango completo de 24 horas sin usar la función .Date() de SQL.
                .Where(ep => ep.ValidFrom >= startOfDayUtc && ep.ValidFrom < endOfNextDayUtc);

            // 2. APLICAR FILTRO DE ESTADO EN LA BASE DE DATOS (Este filtro ya era correcto)
            var activeQuery = query.Where(e =>
            e.EntryPermissionState == PermissionStatus.Pending ||
            (e.EntryPermissionState == PermissionStatus.Completed && e.EntryTime.HasValue && !e.DepartureTime.HasValue)
          );

            // 3. CONTEO TOTAL
            var totalCount = await activeQuery.CountAsync();

            // 4. Aplicar ordenación y paginación
            var skip = (pageNumber - 1) * pageSize;
            var pagedEntities = await activeQuery
              .OrderBy(ep => ep.ValidFrom)
              .Skip(skip)
              .Take(pageSize)
              .ToListAsync();

            // 5. Devolver el resultado paginado
            return new PaginatedResult<EntryPermission>(pagedEntities, totalCount);
        }

        public async Task<IEnumerable<EntryPermission>> GetAllHistoryWithDetailsAsync(
    int pageNumber,
    int pageSize,
    string? searchTerm,
    string? type
)
        {
            var query = _dbContext.EntryPermissions
                .Include(p => p.User)
                .Include(p => p.Visit)
                .Include(p => p.Order)
                .Include(p => p.CheckInGuard)
                .Include(p => p.CheckOutGuard)
                .Where(ep => ep.EntryTime.HasValue);

            // 🔥 FILTRO POR TIPO
            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<PermissionType>(type, true, out var parsedType))
            {
                query = query.Where(ep => ep.PermissionType == parsedType);
            }


            // 🔍 FILTRO SEARCH
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(ep =>
                    (ep.Visit != null &&
                     (ep.Visit.NameVisit.ToLower().Contains(lower) ||
                      ep.Visit.LastNameVisit.ToLower().Contains(lower))) ||

                    (ep.User != null &&
                     (ep.User.Name.ToLower().Contains(lower) ||
                      ep.User.Lastname.ToLower().Contains(lower))) ||

                    (ep.Order != null &&
                     ep.Order.SupplierName.ToLower().Contains(lower)) ||

                    (ep.Visit != null &&
                     ep.Visit.DniVisit.ToString().Contains(lower))
                );
            }

            return await query
                .OrderByDescending(ep => ep.EntryTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

    }
}

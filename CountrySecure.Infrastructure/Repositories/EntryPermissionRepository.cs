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

        public async Task<IEnumerable<EntryPermission>> GetByDateRangeWithDetailsAsync(
         DateTime startDate,
         DateTime endDate,
         int pageNumber, // <--- Parámetro agregado
         int pageSize)   // <--- Parámetro agregado
        {
            // Cálculo para el Skip
            var skip = (pageNumber - 1) * pageSize;

            return await _dbContext.EntryPermissions
                .Include(p => p.User)    // El Residente
                .Include(p => p.Visit)   // El Visitante
                .Include(p => p.Order)   // El Servicio
                                         // Filtramos por la fecha de validez (ValidFrom) que esté dentro del rango del día
                .Where(ep => ep.ValidFrom.Date >= startDate.Date && ep.ValidFrom.Date <= endDate.Date)
                // Opcional: ordenar por hora de entrada esperada
                .OrderBy(ep => ep.ValidFrom)
                // -----------------------------------------------------------------
                // APLICACIÓN DE PAGINACIÓN EN LA BASE DE DATOS (EFICIENTE)
                .Skip(skip)       // Saltar los registros de las páginas anteriores
                .Take(pageSize)  // Tomar solo la cantidad de registros por página
                                 // -----------------------------------------------------------------
                .ToListAsync();
        }

        public async Task<IEnumerable<EntryPermission>> GetAllHistoryWithDetailsAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = _dbContext.EntryPermissions
                .Include(p => p.User)
                .Include(p => p.Visit)
                .Include(p => p.Order)
                .Include(p => p.CheckInGuard)
                .Include(p => p.CheckOutGuard)
                // Filtramos para incluir solo aquellos que al menos tienen registro de entrada
                .Where(ep => ep.EntryTime.HasValue);

            // -------------------------------------------------------------------
            // LÓGICA DE FILTRADO (si searchTerm no es nulo o vacío)
            // -------------------------------------------------------------------
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Convertimos a minúsculas una vez para usarlo en la consulta
                var lowerSearchTerm = searchTerm.ToLower();

                // Aplicamos el filtro OR a todas las columnas buscables
                query = query.Where(ep =>
                    // 1. Por Visitante (Nombre y Apellido)
                    (ep.Visit != null && (ep.Visit.NameVisit.ToLower().Contains(lowerSearchTerm) || ep.Visit.LastNameVisit.ToLower().Contains(lowerSearchTerm))) ||

                    // 2. Por Residente (Nombre y Apellido)
                    (ep.User != null && (ep.User.Name.ToLower().Contains(lowerSearchTerm) || ep.User.Lastname.ToLower().Contains(lowerSearchTerm))) ||

                    // 3. Por Compañía de Servicio (si aplica)
                    (ep.Order != null && ep.Order.SupplierName.ToLower().Contains(lowerSearchTerm)) ||

                    // 4. Por DNI (ej. DNI del Visitante) - Requiere conversión a string
                    (ep.Visit != null && ep.Visit.DniVisit.ToString().Contains(lowerSearchTerm)) // Asumiendo que DniVisit es int

                // Puedes añadir más campos como la placa del vehículo aquí si existe en Visit
                );
            }
            // -------------------------------------------------------------------

            // Aplicar ordenación y paginación
            return await query
                .OrderByDescending(ep => ep.EntryTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

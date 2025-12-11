using CountrySecure.Domain.Entities;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Infrastructure.Repositories;

public class RequestRepository : GenericRepository<Request>, IRequestRepository
{
    private readonly CountrySecureDbContext _dbContext;

    public RequestRepository(CountrySecureDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CountByStatusAsync(RequestStatus status)
    {
        return await _dbContext.Requests
                               .Where(r => (int)r.RequestStatus == (int)status)  
                               .CountAsync();
    }


    public async Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status, int numberPage, int pageSize)
    {
        return await _dbContext.Requests
                               .Where(r => (int)r.RequestStatus == (int)status)  
                               .OrderBy(r => r.Id)
                               .Include(r => r.User)
                               .Include(r => r.Order)
                               .Skip((numberPage - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
    }


    public async Task<IEnumerable<Request>> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.Requests
                               .Include(r => r.User)
                               .Include(r => r.Order)
                               .Where(r => r.IdUser == userId)
                               .ToListAsync();
    }

    public async Task<Request?> GetRequestWithDetailsAsync(Guid id)
    {
        return await _dbContext.Requests
            .Include(r => r.User)       
            .Include(r => r.Order)  
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Request>> GetAllRequestsWithDetailsAsync(int numberPage, int pageSize)
    {
        return await _dbContext.Requests
            .Include(r => r.User)
            .Include(r => r.Order)
            .OrderBy(r => r.Id)
            .Skip((numberPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Request?> ToggleActiveAsync(Guid id)
    {
        // Usar FindAsync es rápido para obtener la entidad por PK
        var request = await _dbContext.Requests.FindAsync(id);

        if (request == null) return null;

        // Lógica para alternar el estado (Eliminación Lógica/Cancelación)

        if (request.RequestStatus == RequestStatus.Cancelled)
        {
            // Si está cancelada, la reactivamos (volver a Pendiente)
            request.RequestStatus = RequestStatus.Pending;
            request.DeletedAt = null; // Borrar la marca de eliminación
        }
        else // Si está Pendiente o en cualquier otro estado 'Activo', la cancelamos
        {
            request.RequestStatus = RequestStatus.Cancelled; // O RequestStatus.Deleted
            request.DeletedAt = DateTime.UtcNow; // Marcar la hora de la eliminación lógica
        }

        request.UpdatedAt = DateTime.UtcNow;

        // Nota: EF Core ya está rastreando la entidad, no es necesario llamar a _dbContext.Update(request);

        // **IMPORTANTE**: Para que el Mapper funcione, debes cargar las relaciones (User y Order) antes de devolver.
        // Como FindAsync no carga relaciones, necesitas hacer un Query explícito si vas a devolver el DTO.

        if (request.DeletedAt == null) // Si la estamos activando, la cargamos para el DTO
        {
            // Cargamos los detalles para el DTO de respuesta
            return await _dbContext.Requests
                .Include(r => r.User)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // Si la estamos cancelando, la devolvemos como está y el Service la mapea.
        // NOTA: Si el Mapper requiere User y Order, SIEMPRE tendrás que hacer el .Include() aquí.
        // La forma más segura es siempre hacer el .Include() si el Service va a mapear a ResponseDto.

        // Opción más simple y segura (aunque menos eficiente si solo se requiere el ID)
        var updatedRequest = await _dbContext.Requests
            .Include(r => r.User)
            .Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.Id == id);

        return updatedRequest; // Devolvemos el request con los detalles cargados.
    }

}

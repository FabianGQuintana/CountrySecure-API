using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IAmenityRepository : IGenericRepository<Amenity>
    {

        Task<Amenity?> GetAmenityByNameAsync(string amenityName);

        Task<Amenity?> GetAmenityWithTurnosAsync(Guid amenityId);

        // serv disponibles 
        Task<IEnumerable<Amenity>> GetAvailableAmenitiesAsync();

        //las aminidades con turnos
        Task<IEnumerable<Amenity>> GetAllAmenitiesWithTurnosAsync(int pageNumber, int pageSize);

        //capacidad disponible
        Task<bool> IsAmenityAvailableForBookingAsync(Guid amenityId, DateTime startTime,DateTime endTime);

        
    }

}


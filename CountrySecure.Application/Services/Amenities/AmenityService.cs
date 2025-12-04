using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CountrySecure.Application.DTOs.Amenity;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Mappers; 

namespace CountrySecure.Application.Services
{
    public class AmenityService : IAmenityService
    {
        // Dependencias (Inyección de Interfaces)
        private readonly IAmenityRepository _amenityRepository;
        private readonly ITurnoRepository _turnoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AmenityService(IAmenityRepository amenityRepository, ITurnoRepository turnoRepository, IUnitOfWork unitOfWork)
        {
            _amenityRepository = amenityRepository;
            _turnoRepository = turnoRepository;
            _unitOfWork = unitOfWork;
        }


        public async Task<AmenityResponseDto> CreateAmenityAsync(AmenityCreateDto dto)
        {
            // 1. Lógica de Negocio: Validación de Unicidad
            var existingAmenity = await _amenityRepository.GetAmenityByNameAsync(dto.AmenityName);
            if (existingAmenity != null)
            {
                throw new InvalidOperationException($"Amenidad con nombre '{dto.AmenityName}' ya existe.");
            }

            // 2. Mapeo Manual (Usando el DTO y solo las propiedades que el cliente puede enviar)
            var amenity = new Amenity
            {
                AmenityName = dto.AmenityName,
                Description = dto.Description,
                Schedules = dto.Schedules,
                Capacity = dto.Capacity,

                // Asignación de auditoría (se obtiene de un contexto de usuario)
                // CreatedBy = _userContextAccessor.GetUserId(), 
                CreatedBy = "SYSTEM", // Manteniendo el valor de tu ejemplo por ahora
                                      // Id, CreatedAt, Status NO se tocan si BaseEntity los maneja por defecto.
            };

            // 3. Persistir la Entidad (El ORM genera el Id, CreatedAt, etc.)
            await _amenityRepository.AddAsync(amenity);

            // 4. Confirmar la transacción
            await _unitOfWork.SaveChangesAsync();

            // 5. Mapear la Entidad persistida (que ahora tiene el Id y la auditoría final) a DTO.
        }

        public async Task<IEnumerable<AmenityResponseDto>> GetAllAsync(int page, int size)
        {
            var amenities = await _amenityRepository.GetAllAsync(page, size);
            return amenities.Select(a => a.ToDto());
        }



    }   
}
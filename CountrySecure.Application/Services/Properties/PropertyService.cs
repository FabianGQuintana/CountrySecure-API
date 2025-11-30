using AutoMapper;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Interfaces.UnitOfWork;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CountrySecure.Application.Services.Properties
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper; 

        
        public PropertyService(IPropertyRepository propertyRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

       
        public async Task<PropertyDto> AddNewPropertyAsync(CreatePropertyDto newPropertyDto)
        {
            
            var newPropertyEntity = _mapper.Map<Property>(newPropertyDto);

            
            var addedEntity = await _propertyRepository.AddAsync(newPropertyEntity);

            
            await _unitOfWork.SaveChangesAsync();

            
            return _mapper.Map<PropertyDto>(addedEntity);
        }

       
        public async Task<PropertyDto?> GetPropertyByIdAsync(int propertyId)
        {
            var propertyEntity = await _propertyRepository.GetByIdAsync(propertyId);

            if (propertyEntity == null)
            {
                return null;
            }

            
            return _mapper.Map<PropertyDto>(propertyEntity);
        }

        

        
        public async Task<IEnumerable<PropertyDto>> GetAllPropertiesAsync(int pageNumber, int pageSize)
        {
            var entities = await _propertyRepository.GetAllAsync(pageNumber, pageSize);

            return _mapper.Map<IEnumerable<PropertyDto>>(entities);
        }

        public async Task<IEnumerable<PropertyDto>> GetPropertiesByStatusAsync(PropertyStatus status, int pageNumber, int pageSize)
        {
            var entities = await _propertyRepository.GetPropertiesByStatusAsync(status, pageNumber, pageSize);

            return _mapper.Map<IEnumerable<PropertyDto>>(entities);
        }

        // MÉTODOS QUE AÚN TRABAJAN CON ENTIDADES (Omiten DTOs de forma temporal)

        //  Este método aún recibe y devuelve Entidades, lo cual es inconsistente.
        // Se recomienda que en el futuro trabaje con DTOs si se usa en la API.
        public async Task<IEnumerable<PropertyDto>> GetPropertiesByOwnerId(int ownerId)
        {
            // 1. Obtener la lista de Entidades del Repositorio (CORRECTO)
            var propertyEntities = await _propertyRepository.GetPropertyByIdUserAsync(ownerId); // <-- USAR await AQUÍ

            // 2. Mapear la colección de Entidades a la colección de DTOs (CORRECTO)
            return _mapper.Map<IEnumerable<PropertyDto>>(propertyEntities);
        }

        //  Este método idealmente recibiría un DTO de Actualización, 
        // pero por ahora mantiene la Entidad por simplicidad.
        public async Task UpdatePropertyAsync(Property updateProperty)
        {
            await _propertyRepository.UpdateAsync(updateProperty);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> SoftDeletePropertyAsync(int propertyId)
        {
            bool marked = await _propertyRepository.DeleteAsync(propertyId);

            if (marked)
            {
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            return false;
        }

    }
}
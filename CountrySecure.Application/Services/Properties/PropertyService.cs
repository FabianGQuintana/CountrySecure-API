using AutoMapper;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Properties;
using CountrySecure.Application.Interfaces.Persistence;

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

       
        public async Task<PropertyResponseDto> AddNewPropertyAsync(CreatePropertyDto newPropertyDto)
        {

            var newPropertyEntity = _mapper.Map<Property>(newPropertyDto);


            var addedEntity = await _propertyRepository.AddAsync(newPropertyEntity);


            await _unitOfWork.SaveChangesAsync();

            
            return _mapper.Map<PropertyResponseDto>(addedEntity);
        }

       
        public async Task<PropertyResponseDto?> GetPropertyByIdAsync(Guid propertyId)
        {
            var propertyEntity = await _propertyRepository.GetByIdAsync(propertyId);

            if (propertyEntity == null)
            {
                return null;
            }

            
            return _mapper.Map<PropertyResponseDto>(propertyEntity);
        }


        public async Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesAsync(int pageNumber, int pageSize)
        {
            var entities = await _propertyRepository.GetAllAsync(pageNumber, pageSize);

            return _mapper.Map<IEnumerable<PropertyResponseDto>>(entities);
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetPropertiesByStatusAsync(PropertyStatus status, int pageNumber, int pageSize)
        {
            var entities = await _propertyRepository.GetPropertiesByStatusAsync(status, pageNumber, pageSize);

            return _mapper.Map<IEnumerable<PropertyResponseDto>>(entities);
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetPropertiesByOwnerId(Guid ownerId)
        {
            
            var propertyEntities = await _propertyRepository.GetPropertyByIdUserAsync(ownerId);

            
            return _mapper.Map<IEnumerable<PropertyResponseDto>>(propertyEntities);
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetPropertiesByLotIdAsync(Guid lotId)
        {
            // 1. Obtener la lista de Entidades (Property) del Repositorio
            var propertyEntities = await _propertyRepository.GetPropertiesByLotIdAsync(lotId);

            // 2. Mapear la colección de Entidades a la colección de DTOs
            return _mapper.Map<IEnumerable<PropertyResponseDto>>(propertyEntities);
        }

        public async Task UpdatePropertyAsync(UpdatePropertyDto updateProperty, Guid currentUser)
        {
            // 1. Recuperar la entidad existente
            var existingProperty = await _propertyRepository.GetByIdAsync(updateProperty.PropertyId);

            // Validación de existencia (aunque el controlador ya maneja la NotFound para el PUT)
            if (existingProperty == null)
            {
                // Lanzamos una excepción que el Controller captura y convierte en 404
                throw new KeyNotFoundException($"Property with ID {updateProperty.PropertyId} not found.");
            }

            // 2. VALIDACIÓN DE SEGURIDAD (Regla de negocio: ¿El usuario es el dueño?)
            if (existingProperty.Id != currentUser) 
            {
                throw new UnauthorizedAccessException("You are not authorized to update this property.");
            }

            // 3. MAPEO (Actualización parcial de campos no nulos)
            // Aplica los cambios del DTO al objeto existente.
            _mapper.Map(updateProperty, existingProperty);

            // 4. Marcar para actualización y guardar la transacción
            await _propertyRepository.UpdateAsync(existingProperty);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<bool> SoftDeletePropertyAsync(Guid propertyId, Guid currentUserId)
        {
            var existingProperty = await _propertyRepository.GetByIdAsync(propertyId);

            if (existingProperty == null)
            {
                return false; 
            }

            if (existingProperty.UserId != currentUserId)
            {
                // El servicio lanza la excepción, que el Controller capturará y devolverá un 403 Forbidden.
                throw new UnauthorizedAccessException("You are not authorized to delete this property.");
            }

            // El repositorio marcará el estado como "Inactive".
            bool marked = await _propertyRepository.DeleteAsync(propertyId);

            if (marked)
            {
                // PERSISTIR CAMBIOS (Transacción)
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            // Esto solo ocurriría si el repositorio no logró encontrar o marcar la entidad.
            return false;
        }

    }
}
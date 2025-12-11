using CountrySecure.Application.DTOs.Request;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace CountrySecure.Application.Services.Request
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RequestService(IRequestRepository requestRepository, IUnitOfWork unitOfWork)
        {
            _requestRepository = requestRepository;
            _unitOfWork = unitOfWork;
        }

        // Crear una nueva solicitud
        public async Task<CreateRequestDto> CreateRequestAsync(CreateRequestDto createRequestDto, Guid currentUserId)
        {

            var requestEntity = createRequestDto.ToEntity();


            requestEntity.CreatedBy = currentUserId.ToString();
            requestEntity.CreatedAt = DateTime.UtcNow; 
            requestEntity.Status = "Active"; 

            // Agregar la nueva solicitud
            await _requestRepository.AddAsync(requestEntity);
            await _unitOfWork.SaveChangesAsync();

            // Mapear la entidad a DTO de respuesta para devolverla
            return createRequestDto;
        }

        // Obtener una solicitud por ID
        public async Task<RequestResponseDto?> GetRequestByIdAsync(Guid requestId)
        {
            var request = await _requestRepository.GetRequestWithDetailsAsync(requestId);


            if (request == null)
                return null;

            // Convertir explícitamente
            return request.ToResponseDto();  
        }

        // Obtener todas las solicitudes con paginación
        public async Task<IEnumerable<RequestResponseDto>> GetAllRequestsAsync(int pageNumber, int pageSize)
        {
            var requests = await _requestRepository.GetAllRequestsWithDetailsAsync(pageNumber, pageSize);
            return requests.ToResponseDto();
        }

        // Obtener solicitudes por estado con paginación
        public async Task<IEnumerable<RequestResponseDto>> GetRequestsByStatusAsync(RequestStatus status, int pageNumber, int pageSize)
        {
            var requests = await _requestRepository.GetByStatusAsync(status, pageNumber, pageSize);
            return requests.ToResponseDto();
        }

        // Contar solicitudes por estado
        public async Task<int> CountRequestsByStatusAsync(RequestStatus status)
        {
            return await _requestRepository.CountByStatusAsync(status);
        }

        // Actualizar una solicitud
        public async Task<RequestResponseDto> UpdateRequestAsync(Guid requestId, UpdateRequestDto updateRequestDto, Guid currentUserId)
        {
            var requestEntity = await _requestRepository.GetByIdAsync(requestId);

            if (requestEntity == null)
                throw new KeyNotFoundException("Request not found.");

            updateRequestDto.MapToEntity(requestEntity);

            requestEntity.LastModifiedAt = DateTime.UtcNow;
            requestEntity.LastModifiedBy = currentUserId.ToString();

            await _unitOfWork.SaveChangesAsync();

            requestEntity = await _requestRepository.GetRequestWithDetailsAsync(requestId);

            if (requestEntity == null)
            {
                
                throw new InvalidOperationException("Entity was updated but failed to be retrieved with details.");
            }
            return requestEntity.ToResponseDto();
        }

        // Eliminar una solicitud (soft delete)
        //public async Task<bool> DeleteRequestAsync(Guid requestId)
        //{
        //    var requestEntity = await _requestRepository.GetByIdAsync(requestId);

        //    if (requestEntity == null)
        //        return false;

        //    // Marcar la solicitud como eliminada
        //    requestEntity.DeletedAt = DateTime.UtcNow;

        //    // Guardar los cambios
        //    await _unitOfWork.SaveChangesAsync();

        //    return true;
        //}

        public async Task<RequestResponseDto?> SoftDeleteToggleAsync(Guid id, Guid currentUserId)
        {
            // 1. Usar el repositorio genérico para alternar el estado (DeletedAt, Status)
            // Usamos el método SoftDeleteToggleAsync, asumiendo que el repositorio RequestRepository lo hereda.
            var request = await _requestRepository.SoftDeleteToggleAsync(id);

            if (request == null)
                return null; // Not found

            // 2. Aplicar Auditoría:
            // **ESTO ES CRUCIAL Y FALTABA en la implementación de ToggleActiveAsync**
            request.LastModifiedAt = DateTime.UtcNow;
            request.LastModifiedBy = currentUserId.ToString();

            // 3. Lógica Específica del Enum (RequestStatus)
            if (request.Status == "Inactive")
            {
                // Si se desactiva, marcamos el estado funcional (Enum) como Cancelled.
                request.RequestStatus = RequestStatus.Cancelled;
            }
            else
            {
                // Si se reactiva, marcamos el estado funcional (Enum) como Pending o el inicial.
                request.RequestStatus = RequestStatus.Pending;
            }

            // 4. Persistencia (Guardar los cambios de Auditoría y RequestStatus)
            // El método ToggleActiveAsync anterior olvidaba el UpdateAsync para guardar los campos de auditoría.
            var updatedEntity = await _requestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            // 5. Mapeo de Retorno
            // NOTA: Si el DTO requiere includes (User y Order), el repositorio debería devolver la entidad con ellos.
            // Usaremos GetRequestWithDetailsAsync para asegurar que el DTO sea completo.
            var fullRequest = await _requestRepository.GetRequestWithDetailsAsync(updatedEntity.Id);

            return fullRequest?.ToResponseDto();
        }

    }
}


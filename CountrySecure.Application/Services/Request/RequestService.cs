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
            public async Task<CreateRequestDto> CreateRequestAsync(CreateRequestDto createRequestDto)
            {
                // Mapear el DTO de creación a una entidad
                var requestEntity = createRequestDto.ToEntity();

                // Agregar la nueva solicitud
                await _requestRepository.AddAsync(requestEntity);
                await _unitOfWork.SaveChangesAsync();

                // Mapear la entidad a DTO de respuesta para devolverla
                return createRequestDto;  // Podrías devolver el DTO de respuesta si lo prefieres
            }

        // Obtener una solicitud por ID
        public async Task<RequestResponseDto?> GetRequestByIdAsync(Guid requestId)
        {
            var request = await _requestRepository.GetByIdAsync(requestId);

            // Convertir explícitamente
            return request.ToResponseDto();  // Asegúrate de que ToResponseDto es un método estático que mapeará de ResponseRequestDto a ResponseRequestDto
        }

        // Obtener todas las solicitudes con paginación
        public async Task<IEnumerable<RequestResponseDto>> GetAllRequestsAsync(int pageNumber, int pageSize)
            {
                var requests = await _requestRepository.GetAllAsync(pageNumber, pageSize);
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
            public async Task<UpdateRequestDto> UpdateRequestAsync(Guid requestId, UpdateRequestDto updateRequestDto)
            {
                var requestEntity = await _requestRepository.GetByIdAsync(requestId);

                if (requestEntity == null)
                    throw new KeyNotFoundException("Request not found.");

                // Mapear los cambios del DTO a la entidad existente
                updateRequestDto.MapToEntity(requestEntity);

                // Guardar los cambios
                await _unitOfWork.SaveChangesAsync();

                return updateRequestDto;  // Podrías devolver el DTO de respuesta si prefieres
            }

            // Eliminar una solicitud (soft delete)
            public async Task<bool> DeleteRequestAsync(Guid requestId)
            {
                var requestEntity = await _requestRepository.GetByIdAsync(requestId);

                if (requestEntity == null)
                    return false;

                // Marcar la solicitud como eliminada
                requestEntity.DeletedAt = DateTime.UtcNow;

                // Guardar los cambios
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
        }
    }


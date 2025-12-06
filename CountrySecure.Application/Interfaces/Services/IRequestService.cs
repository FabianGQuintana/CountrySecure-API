using CountrySecure.Application.DTOs;
using CountrySecure.Application.DTOs.Request;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System.Threading.Tasks;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IRequestService
    {
        Task<CreateRequestDto> CreateRequestAsync(CreateRequestDto createRequestDto);
        Task<RequestResponseDto?> GetRequestByIdAsync(Guid requestId);
        Task<IEnumerable<RequestResponseDto>> GetAllRequestsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<RequestResponseDto>> GetRequestsByStatusAsync(RequestStatus status, int pageNumber, int pageSize);
        Task<int> CountRequestsByStatusAsync(RequestStatus status);
        Task<UpdateRequestDto> UpdateRequestAsync(Guid requestId, UpdateRequestDto updateRequestDto);
        Task<bool> DeleteRequestAsync(Guid requestId);
    }
}

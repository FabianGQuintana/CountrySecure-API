using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Request;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Exceptions;
using RequestEntity = CountrySecure.Domain.Entities.Request;

namespace CountrySecure.Application.Services.Request
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public RequestService(
            IRequestRepository requestRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _requestRepository = requestRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<RequestResponseDto> CreateAsync(CreateRequestDto dto, Guid currentUserId)
        {
            if (await _orderRepository.GetByIdAsync(dto.OrderId) == null)
                throw new KeyNotFoundException($"The service with ID {dto.OrderId} does not exist or is inactive.");

            var request = dto.ToEntity();
            request.UserId = currentUserId;                 // el solicitante es siempre el usuario autenticado
            request.RequestStatus = RequestStatus.Pending;
            request.CreatedBy = currentUserId.ToString();

            await _requestRepository.AddAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(request.Id);
        }

        public async Task<RequestResponseDto> UpdateAsync(Guid id, UpdateRequestDto dto, Guid currentUserId)
        {
            var request = await GetOwnedOrThrowAsync(id);

            dto.MapToEntity(request);
            request.Touch(currentUserId.ToString());

            await _requestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id);
        }

        public async Task<RequestResponseDto> DeleteAsync(Guid id, Guid currentUserId)
        {
            var request = await GetOwnedOrThrowAsync(id);

            request.SoftDelete(currentUserId.ToString());
            request.RequestStatus = RequestStatus.Cancelled;

            await _requestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id, includeDeleted: true);
        }

        public async Task<RequestResponseDto> RestoreAsync(Guid id, Guid currentUserId)
        {
            var request = await _requestRepository.GetByIdIncludingDeletedAsync(id)
                          ?? throw new KeyNotFoundException("Request not found.");

            request.Restore(currentUserId.ToString());
            request.RequestStatus = RequestStatus.Pending;

            await _requestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id);
        }

        public async Task<RequestResponseDto> GetByIdAsync(Guid id)
        {
            var request = await _requestRepository.GetWithDetailsAsync(id)
                          ?? throw new KeyNotFoundException("Request not found.");

            EnsureCanAccess(request);
            return request.ToResponseDto();
        }

        public async Task<PagedResult<RequestResponseDto>> GetPagedAsync(PageQuery paging, RequestStatus? status, bool onlyMine, bool includeInactive)
        {
            // Quien no es Admin solo ve sus propias solicitudes; las canceladas solo las ve un Admin o su dueño
            var restrictToMe = onlyMine || !_currentUser.IsAdmin;
            Guid? userId = restrictToMe ? _currentUser.GetRequiredUserId() : null;

            var page = await _requestRepository.GetPagedAsync(paging, status, userId, includeInactive && (_currentUser.IsAdmin || restrictToMe));
            return page.Map(r => r.ToResponseDto());
        }

        public Task<int> CountByStatusAsync(RequestStatus status) => _requestRepository.CountByStatusAsync(status);

        // --- helpers ---

        private async Task<RequestResponseDto> ReloadAsync(Guid id, bool includeDeleted = false)
        {
            var request = await _requestRepository.GetWithDetailsAsync(id, includeDeleted)
                          ?? throw new KeyNotFoundException("Request not found.");
            return request.ToResponseDto();
        }

        private async Task<RequestEntity> GetOwnedOrThrowAsync(Guid id)
        {
            var request = await _requestRepository.GetByIdAsync(id)
                          ?? throw new KeyNotFoundException("Request not found.");

            EnsureCanAccess(request);
            return request;
        }

        private void EnsureCanAccess(RequestEntity request)
        {
            if (!_currentUser.IsAdmin && request.UserId != _currentUser.UserId)
                throw new UnauthorizedAccessException("You can only access your own requests.");
        }
    }
}

using CountrySecure.Application.DTOs.Turns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface ITurnService
    {
        Task<TurnResponseDto> AddNewTurnAsync(CreateTurnDto newTurnDto, Guid currentUserId);

        Task<TurnResponseDto?> GetTurnByIdAsync(Guid turnId);

        Task<IEnumerable<TurnResponseDto>> GetTurnsByUserIdAsync(Guid userId);

        Task<IEnumerable<TurnResponseDto>> GetTurnsByAmenityIdAsync(Guid amenityId);

        Task<TurnResponseDto?> UpdateTurnAsync(Guid turnId, UpdateTurnDto updateTurnDto, Guid currentUserId);

        Task<bool> SoftDeleteTurnAsync(Guid turnId, Guid currentUserId);

        Task<IEnumerable<TurnResponseDto>> GetTurnsByDateRangeAsync(DateTime startDate, DateTime endDate);

    }
}

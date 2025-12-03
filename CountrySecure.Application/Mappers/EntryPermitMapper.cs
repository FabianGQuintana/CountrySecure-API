using CountrySecure.Application.DTOs.EntryPermit;
using CountrySecure.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CountrySecure.Application.Mappers
{
    public static class EntryPermitMapper
    {
        public static EntryPermitResponseDto ToPermitDto(this EntryPermit entity)
        {
            if (entity == null)
                return null;

            return new EntryPermitResponseDto
            {
                EntryPermitId = entity.Id,
                QR = entity.QR,
                HorarioIngreso = entity.HorarioIngreso,
                HorarioSalida = entity.HorarioSalida,
                FechaVisita = entity.FechaVisita,


                // Relaciones
                VisitId = entity.VisitId,
                UserId = entity.UserId,
                ServiceId = entity.ServiceId,
                ParentPermitId = entity.ParentPermitId,
            };
        }

        public static IEnumerable<EntryPermitResponseDto> ToPermitDto(this IEnumerable<EntryPermit> entities)
        {
            return entities?.Select(e => e.ToPermitDto())
                   ?? Enumerable.Empty<EntryPermitResponseDto>();
        }
    }
}

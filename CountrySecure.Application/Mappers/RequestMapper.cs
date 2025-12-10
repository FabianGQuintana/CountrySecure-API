using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Request;
using System.Collections.Generic;
using System.Linq;
using CountrySecure.Domain.Enums;
using System;

namespace CountrySecure.Application.Mappers
{
    public static class RequestMapper
    {
        // 1. Entidad -> DTO de Respuesta (Lectura/Consulta)
        public static RequestResponseDto ToResponseDto(this Request request)
        {
            return new RequestResponseDto
            {
                // Propiedades de datos
                Details = request.Details,
                Location = request.Location,
                Status = request.RequestStatus,

                CreatedAt = request.CreatedAt,

                User = request.User.ToRequestUserDto(),
                Order = request.Order.ToRequestOrderDto()


            };
        }

        // 2. Colección de Entidades -> Colección de DTOs
        public static IEnumerable<RequestResponseDto> ToResponseDto(this IEnumerable<Request> requests)
        {
            return requests.Select(r => r.ToResponseDto());
        }

        // 3. DTO de Creación -> Entidad (Escritura POST)
        public static Request ToEntity(this CreateRequestDto dto)
        {
            return new Request
            {
                // Propiedades de datos
                Details = dto.Details,
                Location = dto.Location,

                // IDs (Guid a Guid)
                IdUser = dto.UserId,
                IdOrder = dto.OrderId,

                // Status (Se inicializa al valor del DTO o al defecto si no se requiere en DTO)
                RequestStatus = dto.Status,

                // Miembros requeridos de la Entidad (Relationships) para EF Core/Domain
                // Se inicializan a 'null!' para satisfacer el 'required' del compilador, 
                // asumiendo que el contexto de la base de datos o el servicio los gestionará.
                
                User = null!,
                Order = null!   
            };
        }

        // 4. Mapeo de Actualización (Actualización Parcial PUT/PATCH)
        public static void MapToEntity(this UpdateRequestDto dto, Request existingEntity)
        {
            // Solo sobrescribe si el valor del DTO NO es nulo (Comportamiento PATCH)
            existingEntity.Details = dto.Details ?? existingEntity.Details;
            existingEntity.Location = dto.Location ?? existingEntity.Location;

            // Para RequestStatus? (Enum nullable): Solo cambia el estado si se ha enviado un nuevo valor
            if (dto.Status.HasValue)
            {
                existingEntity.RequestStatus = dto.Status.Value;
            }

            // Nota: IdUser e IdOrder (relaciones) generalmente NO se actualizan en un UpdateRequestDto.
        }

        public static RequestOrderDto ToRequestOrderDto(this Order order)
        {
             // Si order es null, devolvemos null o manejamos excepción según preferencia
             if (order == null) throw new ArgumentNullException(nameof(order));

             return new RequestOrderDto
             {
                 Id = order.Id,
                 SupplierName = order.SupplierName,
                 Description = order.Description,
                 OrderType = (int)order.OrderType
             };
        }
        
        public static RequestUserDto ToRequestUserDto(this User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return new RequestUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Lastname = user.Lastname,
                Email = user.Email
            };
        }
    }
}
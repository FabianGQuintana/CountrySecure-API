// using CountrySecure.Domain.Entities;
// using CountrySecure.Application.DTOs.Request;
// using System.Collections.Generic;
// using System.Linq;
// using CountrySecure.Domain.Enums;

// namespace CountrySecure.Application.Mappers
// {
//     public static class RequestMapper
//     {
//         // 1. Entidad -> DTO de Respuesta (Lectura/Consulta)
//         public static RequestResponseDto ToResponseDto(this Request request)
//         {
//             // Convertir el Status del enum a string (si es necesario en el DTO)
//             var statusString = request.Status.ToString();

//             return new RequestResponseDto
//             {
//                 Details = request.Details,
//                 Location = request.Location,
//                 Status = request.Status, // Usa directamente el enum si es compatible con el DTO
//                 IdUser = request.IdUser,
//                 IdOrder = request.IdOrder,
//             };
//         }

//         // 2. Colección de Entidades -> Colección de DTOs
//         public static IEnumerable<RequestResponseDto> ToResponseDto(this IEnumerable<Request> requests)
//         {
//             return requests.Select(r => r.ToResponseDto());
//         }

//         // 3. DTO de Creación -> Entidad (Escritura POST)
//         public static Request ToEntity(this CreateRequestDto dto)
//         {
//             return new Request
//             {
//                 Details = dto.Details,
//                 Location = dto.Location,
//                 IdUser = dto.IdUser,
//                 IdOrder = dto.IdOrder,
//                 Status = dto.Status,  // Se usa directamente el valor del DTO, por lo que no es necesario asignar un valor por defecto aquí
//                 // La fecha y otros valores como Id probablemente se gestionan en la base de datos o en otro lado.
//             };
//         }

//         // 4. Mapeo de Actualización (Actualización Parcial PUT/PATCH)
//         public static void MapToEntity(this UpdateRequestDto dto, Request existingEntity)
//         {
//             // Solo sobrescribe si el valor del DTO NO es nulo
//             existingEntity.Details = dto.Details ?? existingEntity.Details;
//             existingEntity.Location = dto.Location ?? existingEntity.Location;

//             // Solo cambia el estado si se ha enviado un nuevo valor
//             if (dto.Status.HasValue)  // Verifica si el estado es diferente de null
//             {
//                 existingEntity.Status = dto.Status.Value;  // Usa .Value para acceder al valor del nullable
//             }

//             // Actualizar otras propiedades si es necesario (ejemplo: fechas, relaciones)
//             // El usuario y la orden no deben actualizarse aquí, a menos que el DTO contenga valores específicos.
//         }
//     }
// }

using System;
using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.Turns
{
    public class CreateTurnDto
    {
        [Required]
        // FK: El ID de la Amenity (el recurso que se reserva)
        public Guid AmenityId { get; set; }

        [Required]
        // Propiedad de negocio: Hora de inicio del turno
        public DateTime StartTime { get; set; }

        [Required]
        // Propiedad de negocio: Hora de finalización del turno
        public DateTime EndTime { get; set; }
    }
}
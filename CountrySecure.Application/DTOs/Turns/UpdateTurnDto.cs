using System;

namespace CountrySecure.Application.DTOs.Turns
{
    // NO incluye la propiedad Id, ya que será pasada como parámetro al servicio.
    public class UpdateTurnDto
    {
        // La AmenityId no se debería cambiar una vez que el turno está creado.

        public DateTime? StartTime { get; set; } // Opcional, para cambiar la hora de la reserva

        public DateTime? EndTime { get; set; }   // Opcional, para cambiar la hora de finalización

        // El estado lo enviamos como string para mayor flexibilidad (ej: "CancelledByUser")
        public string? Status { get; set; }
    }
}
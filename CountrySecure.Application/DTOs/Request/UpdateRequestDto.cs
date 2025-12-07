using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CountrySecure.Application.DTOs.Request;

public class UpdateRequestDto
{
    public string? Details { get; set; }  // Detalles pueden no ser enviados
    public string? Location { get; set; }  // Ubicación puede no ser enviada
    public RequestStatus? Status { get; set; }  // El estado puede no ser modificado
}

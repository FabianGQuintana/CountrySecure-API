using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CountrySecure.Application.DTOs.Request;

public class UpdateRequestDto
{
    public string? Details { get; set; }  
    public string? Location { get; set; }  
    public RequestStatus? Status { get; set; }  
}

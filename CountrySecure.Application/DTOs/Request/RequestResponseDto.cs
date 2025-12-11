using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Order;
using CountrySecure.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.DTOs.Request;

public class RequestResponseDto
{
    public required Guid Id { get; set; }
    public  required string Details { get; set; }
    public  required string Location { get; set; }  
    public RequestStatus Status { get; set; }

    public required string BaseEntityStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public required RequestUserDto User { get; set; }
    public required RequestOrderDto Order { get; set; }

}


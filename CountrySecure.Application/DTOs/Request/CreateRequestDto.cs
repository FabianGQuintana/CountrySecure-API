using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.DTOs.Request;

public class CreateRequestDto
{
    public required string Details { get; set; }
    public required string Location { get; set; }
    public required Guid IdUser { get; set; }
    public required Guid IdOrder { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
}
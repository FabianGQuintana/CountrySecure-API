using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.DTOs.Request;

public class CreateRequestDto
{
    public string Details { get; set; }
    public string Location { get; set; }
    public int IdUser { get; set; }
    public int IdOrder { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
}
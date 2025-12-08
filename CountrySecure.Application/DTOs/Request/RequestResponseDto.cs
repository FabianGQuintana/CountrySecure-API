using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.DTOs.Request;

public class RequestResponseDto
{
    public  required string Details { get; set; }
    public  required string Location { get; set; }  
    public RequestStatus Status { get; set; }  
    public Guid IdUser { get; set; }  
    public Guid IdOrder { get; set; }  

}


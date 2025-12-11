using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Domain.Enums;

public enum PermissionStatus
{
    Cancelled = 0,
    Pending = 1,
    Completed = 2,
    Expired = 3
}

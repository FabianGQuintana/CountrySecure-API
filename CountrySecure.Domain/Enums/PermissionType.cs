using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Domain.Enums
{
    public enum PermissionType
    {
        // El tipo de permiso para invitados, familiares, etc.
        Visit = 1,

        // El tipo de permiso para técnicos, jardineros, plomeros, etc.
        Maintenance = 2

    }
}

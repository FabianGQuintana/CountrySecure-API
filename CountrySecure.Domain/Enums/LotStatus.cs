using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Domain.Enums
{
    public enum LotStatus
    {
        Inactive = 0,     // Baja logica (Valor por defecto)

        // 1. Estados funcionales
        Occupied = 1,     // Ocupado (Hay propiedades ya ocupando el lote.)
        Available = 2,    // Disponible (Lote listo para nuevas propiedades)

    }
}

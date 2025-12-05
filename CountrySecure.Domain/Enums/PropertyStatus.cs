namespace CountrySecure.Domain.Enums
{
    public enum PropertyStatus
    {
        // 0. El valor por defecto Inactivo/Baja logica
        Inactive = 0,

        // 1. Estados funcionales
        Occupied = 1,     // Ocupado (Residente viviendo en ella)
        Available = 2,    // Disponible (Lista para ser habitada)
        NewBrand = 3      // A estrenar (Nueva construcción, nunca habitada)
    }
}
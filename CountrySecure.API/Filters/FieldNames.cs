namespace CountrySecure.API.Filters
{
    /// <summary>
    /// Nombres en español de los campos de los DTOs, para los mensajes de validación por defecto
    /// de FluentValidation (por ejemplo "'Detalle' no debería estar vacío.").
    /// </summary>
    public static class FieldNames
    {
        private static readonly Dictionary<string, string> Names = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Name"] = "Nombre",
            ["Lastname"] = "Apellido",
            ["Dni"] = "DNI",
            ["Phone"] = "Teléfono",
            ["Email"] = "Email",
            ["Password"] = "Contraseña",
            ["Role"] = "Rol",
            ["AmenityName"] = "Nombre",
            ["Description"] = "Descripción",
            ["Schedules"] = "Horarios",
            ["Capacity"] = "Capacidad",
            ["LotName"] = "Lote",
            ["BlockName"] = "Manzana",
            ["Street"] = "Calle",
            ["PropertyNumber"] = "Número",
            ["SupplierName"] = "Proveedor",
            ["OrderType"] = "Tipo de servicio",
            ["Details"] = "Detalle",
            ["Location"] = "Ubicación",
            ["OrderId"] = "Servicio",
            ["AmenityId"] = "Amenity",
            ["StartTime"] = "Hora de inicio",
            ["EndTime"] = "Hora de fin",
            ["ValidFrom"] = "Válido desde",
            ["ValidTo"] = "Válido hasta",
            ["NameVisit"] = "Nombre",
            ["LastNameVisit"] = "Apellido",
            ["DniVisit"] = "DNI",
            ["CompanionsCount"] = "Acompañantes",
            ["VehiclePlate"] = "Patente",
            ["VehicleModel"] = "Modelo",
            ["CurrentPassword"] = "Contraseña actual",
            ["NewPassword"] = "Nueva contraseña",
        };

        public static string Spanish(string member) => Names.TryGetValue(member, out var name) ? name : member;
    }
}

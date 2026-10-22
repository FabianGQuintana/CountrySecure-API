namespace CountrySecure.Domain.Entities
{
    /// <summary>Aviso para un usuario (p. ej. "Tu visita ingresó al barrio"). Se lee desde la campana del panel.</summary>
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }

        /// <summary>Tipo de aviso (VisitCheckIn, VisitCheckOut...) para elegir el ícono en el front.</summary>
        public required string Type { get; set; }

        /// <summary>Permiso al que se refiere el aviso, si corresponde.</summary>
        public Guid? PermissionId { get; set; }

        public DateTime? ReadAt { get; set; }

        public User? User { get; set; }
    }
}

using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Mappers
{
    public static class AuditExtensions
    {
        public const string ActiveStatus = "Active";
        public const string InactiveStatus = "Inactive";

        /// <summary>
        /// Estado de auditoría para las respuestas de la API, DERIVADO de <see cref="BaseEntity.DeletedAt"/>
        /// ("Active" / "Inactive"). No es un dato almacenado: la única fuente de verdad es DeletedAt.
        /// </summary>
        public static string ToAuditStatus(this BaseEntity entity) => entity.IsDeleted ? InactiveStatus : ActiveStatus;
    }
}

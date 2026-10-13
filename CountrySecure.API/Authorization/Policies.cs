using CountrySecure.Domain.Constants;

namespace CountrySecure.API.Authorization
{
    /// <summary>
    /// Nombres de políticas de autorización por rol. Se registran en Program.cs.
    /// </summary>
    public static class Policies
    {
        public const string AdminOnly = nameof(AdminOnly);
        public const string SecurityStaff = nameof(SecurityStaff); // Security o Admin

        public static readonly string[] SecurityStaffRoles = [RoleTypes.Security, RoleTypes.Admin];
    }
}

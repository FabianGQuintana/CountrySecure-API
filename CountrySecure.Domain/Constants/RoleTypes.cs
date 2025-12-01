namespace CountrySecure.Domain.Constants
{
    public static class RoleTypes
    {
        public const string Admin = "Admin";
        public const string Security = "Security";
        public const string Resident = "Resident";
        public const string Guest = "Guest";
        public const string Provider = "Provider";

        public static readonly string[] All =
        [
            Admin,
            Security,
            Resident,
            Guest,
            Provider
        ];
    }
}
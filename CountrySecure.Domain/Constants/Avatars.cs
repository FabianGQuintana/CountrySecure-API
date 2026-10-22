namespace CountrySecure.Domain.Constants
{
    /// <summary>Avatares ilustrados que puede elegir cualquier usuario (no se suben fotos).</summary>
    public static class Avatars
    {
        public static readonly string[] All =
        [
            "avatar-1",
            "avatar-2",
            "avatar-3",
            "avatar-4",
            "avatar-5",
        ];

        public static bool IsValid(string? avatar) => avatar != null && All.Contains(avatar);
    }
}

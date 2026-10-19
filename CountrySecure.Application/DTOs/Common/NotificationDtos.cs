namespace CountrySecure.Application.DTOs.Common
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public required string Type { get; set; }
        public Guid? PermissionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Read { get; set; }
    }

    public class UnreadCountDto
    {
        public int Count { get; set; }
    }
}

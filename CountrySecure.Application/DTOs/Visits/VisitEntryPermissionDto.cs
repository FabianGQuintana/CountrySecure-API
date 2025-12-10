using CountrySecure.Domain.Enums;

public class VisitEntryPermissionDto
{
    public required string QrCodeValue { get; set; }
    public PermissionType Type { get; set; }
    public PermissionStatus Status { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? EntryTime { get; set; }
    public DateTime? DepartureTime { get; set; }
}

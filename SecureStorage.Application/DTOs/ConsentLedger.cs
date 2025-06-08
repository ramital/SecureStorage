namespace SecureStorage.Application.DTOs;

public class ConsentLedger
{
    public string PatientId { get; set; } = null!;
    public string? Contents { get; set; } = null!;
    public DateTimeOffset Timestamp { get; set; }
}

namespace SecureStorage.Application.DTOs;

public class ConsentResult
{
    public DateTimeOffset Timestamp { get; set; }
    public string PatientId { get; set; } = null!;
    public string? Contents { get; set; } = null!;
}

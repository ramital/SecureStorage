namespace SecureStorage.Domain.Entities;

public class Consent
{
    public string PatientId { get; set; } = null!;
    public string ConsentTxId { get; set; } = null!;
    public DateTimeOffset TimestampUtc { get; set; }
}

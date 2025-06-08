namespace SecureStorage.Application.DTOs;

public class ConsentDto
{
    public string PatientId { get; set; } = null!;
    public string PatientName { get; set; } = null!;
    public ConsentTerms ConsentTerms { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}

public class ConsentTerms
{
    public bool Identifiers { get; set; }
    public bool MedicalRecords { get; set; }
    public bool ContactInfo { get; set; }
    public bool InsuranceInfo { get; set; }
    public bool FinancialInfo { get; set; }
    public bool BiometricData { get; set; }
}

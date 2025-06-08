using SecureStorage.Application.DTOs;

namespace SecureStorage.Application.Interfaces;

public interface IConsentService
{
    Task<ConsentResult> CreateConsentAsync(ConsentDto request);
    Task<ConsentResult?> GetConsentAsync(string patientId);
}

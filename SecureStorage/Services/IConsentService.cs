using SecureStorage.Models;

namespace SecureStorage.Services;

public interface IConsentService
{
    Task<ConsentResponse> CreateConsentAsync(ConsentRequest request);
    Task<ConsentResponse> GetConsentAsync(string patientId);
}

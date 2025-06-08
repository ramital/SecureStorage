
using SecureStorage.Domain.Entities;

namespace SecureStorage.Application.Interfaces;

public interface IBlobRepository
{
    Task UpsertConsentEntityAsync(Consent entity);
    Task<Consent> GetConsentEntityAsync(string patientId);
}

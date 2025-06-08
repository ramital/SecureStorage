using SecureStorage.Entity;
using SecureStorage.Models;

namespace SecureStorage.Repositories;

public interface IBlobRepository
{
    Task UpsertConsentEntityAsync(ConsentEntity entity);
    Task<ConsentEntity> GetConsentEntityAsync(string patientId);
}

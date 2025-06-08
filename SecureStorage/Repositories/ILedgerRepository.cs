using SecureStorage.Models;

namespace SecureStorage.Repositories;

public interface ILedgerRepository
{
    Task<string> CreateConsentAsync(Consent consent);
    Task<Consent> GetConsentAsync(string transactionId);
}

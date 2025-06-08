using SecureStorage.Application.DTOs;

namespace SecureStorage.Application.Interfaces;

public interface ILedgerRepository
{
    Task<string> CreateConsentAsync(ConsentLedger consent);
    Task<ConsentLedger> GetConsentAsync(string transactionId);
}

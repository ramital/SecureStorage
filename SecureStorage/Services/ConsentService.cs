using SecureStorage.Entity;
using SecureStorage.Models;
using SecureStorage.Repositories;
using System.Text.Json;

namespace SecureStorage.Services;

public class ConsentService : IConsentService
{
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IBlobRepository _blobRepository;

    public ConsentService(ILedgerRepository ledgerRepository, IBlobRepository blobRepository)
    {
        _ledgerRepository = ledgerRepository;
        _blobRepository = blobRepository;
    }

    public async Task<ConsentResponse> CreateConsentAsync(ConsentRequest request)
    {
        if (await _blobRepository.GetConsentEntityAsync(request.PatientId) != null)
            throw new Exception("Patient exists");

        var consent = new Consent
        {
            PatientId = request.PatientId,
            Timestamp = DateTimeOffset.UtcNow,
            Contents= JsonSerializer.Serialize(request)
        };

        var transactionId = await _ledgerRepository.CreateConsentAsync(consent);

        var entity = new ConsentEntity
        {
            RowKey = consent.PatientId,
            ConsentTxId = transactionId,
            TimestampUtc = consent.Timestamp
        };
        await _blobRepository.UpsertConsentEntityAsync(entity);

        return new ConsentResponse
        {
            Timestamp = consent.Timestamp,
            PatientId = consent.PatientId,
            Contents = consent.Contents
        };
    }

    public async Task<ConsentResponse> GetConsentAsync(string patientId)
    {
        var entity = await _blobRepository.GetConsentEntityAsync(patientId);
        if(entity==null)
            throw new Exception("Patient doesn't exists");
        var consent = await _ledgerRepository.GetConsentAsync(entity.ConsentTxId);

        return new ConsentResponse
        {
            Timestamp = entity.TimestampUtc,
            PatientId = patientId,
            Contents = consent.Contents
        };
    }
}
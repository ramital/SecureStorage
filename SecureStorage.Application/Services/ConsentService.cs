using SecureStorage.Application.DTOs;
using SecureStorage.Application.Interfaces;
using SecureStorage.Domain.Entities;
using System.Text.Json;

namespace SecureStorage.Application.Services;

public class ConsentService : IConsentService
{
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IBlobRepository _blobRepository;

    public ConsentService(ILedgerRepository ledgerRepository, IBlobRepository blobRepository)
    {
        _ledgerRepository = ledgerRepository;
        _blobRepository = blobRepository;
    }
    public async Task<ConsentResult> CreateConsentAsync(ConsentDto request)
    {
        if (await _blobRepository.GetConsentEntityAsync(request.PatientId) != null)
            throw new Exception("Patient exists");

        var consent = new ConsentLedger
        {
            PatientId = request.PatientId,
            Timestamp = DateTimeOffset.UtcNow,
            Contents = JsonSerializer.Serialize(request)
        };

        var transactionId = await _ledgerRepository.CreateConsentAsync(consent);

        var entity = new Consent
        {
            PatientId = consent.PatientId,
            ConsentTxId = transactionId,
            TimestampUtc = consent.Timestamp
        };
        await _blobRepository.UpsertConsentEntityAsync(entity);

        return new ConsentResult
        {
            Timestamp = consent.Timestamp,
            PatientId = consent.PatientId,
            Contents = consent.Contents
        };
    }


    public async Task<ConsentResult?> GetConsentAsync(string patientId)
    {
        var entity = await _blobRepository.GetConsentEntityAsync(patientId);
         if(entity==null)
        return null;
        //    throw new Exception("Patient doesn't exists");
        var consent = await _ledgerRepository.GetConsentAsync(entity.ConsentTxId);

        return new ConsentResult
        {
            Timestamp = entity.TimestampUtc,
            PatientId = patientId,
            Contents = consent.Contents
        };
    }
}
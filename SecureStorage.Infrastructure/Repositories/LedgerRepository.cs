using Azure.Security.ConfidentialLedger;
using System.Text.Json;
using Azure.Core;
using Azure;
using SecureStorage.Application.Interfaces;
using SecureStorage.Application.DTOs;

namespace SecureStorage.Infrastructure.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly ConfidentialLedgerClient _ledgerClient;

    public LedgerRepository(ConfidentialLedgerClient ledgerClient)
    {
        _ledgerClient = ledgerClient;
    }

    public async Task<string> CreateConsentAsync(ConsentLedger consent)
    {
        var payload = new
        {
            patientId = consent.PatientId,
            timestamp = DateTimeOffset.UtcNow,
            contents = consent.Contents
        };
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var appendResult = await _ledgerClient.PostLedgerEntryAsync(
            waitUntil: WaitUntil.Completed,
            RequestContent.Create(new BinaryData(jsonBytes)));

        return appendResult.Id;
    }

    public async Task<ConsentLedger> GetConsentAsync(string transactionId)
    {
        var ledgerEntry = await _ledgerClient.GetLedgerEntryAsync(transactionId);
        var payload = JsonSerializer.Deserialize<dynamic>(ledgerEntry.Content);
        using var doc = JsonDocument.Parse(ledgerEntry.Content);

        if (doc.RootElement.TryGetProperty("entry", out var contentsElement))
        {
            var contents = contentsElement.GetProperty("contents").GetString();
            return new ConsentLedger { Contents = contents };
        }
        else
        {
            return new ConsentLedger { Contents = null };
        }
    }
}
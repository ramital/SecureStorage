using Azure.Security.ConfidentialLedger;
using SecureStorage.Models;
using System.Text.Json;
using Azure.Core;
using Azure;

namespace SecureStorage.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly ConfidentialLedgerClient _ledgerClient;

    public LedgerRepository(ConfidentialLedgerClient ledgerClient)
    {
        _ledgerClient = ledgerClient;
    }

    public async Task<string> CreateConsentAsync(Consent consent)
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

    public async Task<Consent> GetConsentAsync(string transactionId)
    {
        var ledgerEntry = await _ledgerClient.GetLedgerEntryAsync(transactionId);
        var payload = JsonSerializer.Deserialize<dynamic>(ledgerEntry.Content);
        using var doc = JsonDocument.Parse(ledgerEntry.Content);

        if (doc.RootElement.TryGetProperty("entry", out var contentsElement))
        {
            var contents = contentsElement.GetProperty("contents").GetString();
            return new Consent { Contents = contents };
        }
        else
        {
            return new Consent { Contents = null };
        }
    }
}
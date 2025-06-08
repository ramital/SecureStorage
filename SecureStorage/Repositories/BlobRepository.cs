using Azure.Data.Tables;
using SecureStorage.Entity;

namespace SecureStorage.Repositories;

public class BlobRepository : IBlobRepository
{
    private readonly TableClient _tableClient;

    public BlobRepository(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task UpsertConsentEntityAsync(ConsentEntity entity)
    {
        await _tableClient.UpsertEntityAsync(entity);
    }

    public async Task<ConsentEntity?> GetConsentEntityAsync(string patientId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<ConsentEntity>("Consent", patientId);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
}
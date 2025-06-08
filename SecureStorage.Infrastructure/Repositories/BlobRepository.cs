using Azure.Data.Tables;
using SecureStorage.Application.Interfaces;
using SecureStorage.Domain.Entities;
using SecureStorage.Infrastructure.Entities;
using SecureStorage.Infrastructure.Mappers;

namespace SecureStorage.Infrastructure.Repositories;

public class BlobRepository : IBlobRepository
{
    private readonly TableClient _tableClient;

    public BlobRepository(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task UpsertConsentEntityAsync(Consent consent)
    {
        var entity = ConsentMapper.ToEntity(consent);
        await _tableClient.UpsertEntityAsync(entity);
    }

    public async Task<Consent?> GetConsentEntityAsync(string patientId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<ConsentEntity>("Consent", patientId);

            return ConsentMapper.ToDomain(response.Value);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
}
using SecureStorage.Domain.Entities;
using SecureStorage.Infrastructure.Entities;

namespace SecureStorage.Infrastructure.Mappers;

public static class ConsentMapper
{
    public static ConsentEntity ToEntity(Consent consent)
    {
        return new ConsentEntity
        {
            RowKey = consent.PatientId,
            ConsentTxId = consent.ConsentTxId,
            TimestampUtc = consent.TimestampUtc
        };
    }

    public static Consent ToDomain(ConsentEntity entity)
    {
        return new Consent
        {
            PatientId = entity.RowKey,
            ConsentTxId = entity.ConsentTxId,
            TimestampUtc = entity.TimestampUtc
        };
    }
}

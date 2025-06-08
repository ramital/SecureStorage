using Azure;
using Azure.Data.Tables;

namespace SecureStorage.Infrastructure.Entities;

public class ConsentEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Consent";
    public string RowKey { get; set; } = null!;
    public string ConsentTxId { get; set; } = null!;
    public DateTimeOffset TimestampUtc { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; } = ETag.All;
}

using Azure.Data.Tables;
using Azure;

namespace SecureStorage.Entity;

public class ConsentEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Consent";
    public string RowKey { get; set; } = null!;
    public string ConsentTxId { get; set; } = null!;
    public DateTimeOffset TimestampUtc { get; set; }
    public ETag ETag { get; set; } = ETag.All;
    public DateTimeOffset? Timestamp { get; set; }
}

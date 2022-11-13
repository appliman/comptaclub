using Azure;

namespace ComptaClub.Datas;

public class Account : ITableEntity
{
    public AccountDirection Direction { get; set; }
    public string Label { get; set; } = null!;
    public string? LinkedAccountId { get; set; }
    public DateTime LastUpdate { get; set; }

    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get ; set ; } = DateTime.Now;
    public ETag ETag { get ; set ; }
}

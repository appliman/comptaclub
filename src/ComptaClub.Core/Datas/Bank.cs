using Azure;

namespace ComptaClub.Datas
{
    public class Bank : ITableEntity
    {
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public string? Label { get; set; }

        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; } = DateTime.Now;
        public ETag ETag { get; set; }
    }
}

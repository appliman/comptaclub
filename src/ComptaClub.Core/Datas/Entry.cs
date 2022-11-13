using Azure;

namespace ComptaClub.Datas
{
    public class Entry : ITableEntity
    {
        public Guid Id { get; set; }

        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; } = DateTime.Now;
        public ETag ETag { get; set; }
    }
}

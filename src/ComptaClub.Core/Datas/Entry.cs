using Azure;

namespace ComptaClub.Datas
{
    public class Entry : ITableEntity
    {
        public Guid Id { get; set; }
        public int CreationDate { get; set; }

        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}

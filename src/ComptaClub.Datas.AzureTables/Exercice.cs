using Azure;

namespace ComptaClub.Datas
{
    public class Exercice : ITableEntity
    {
        public string? Label { get; set; } 
        public long InitialAmount { get; set; }

        public int StartDate { get; set; }
        public int EndDate { get; set; }
        public int CreationDate { get; set; }
        public Guid? LastEntryId { get; set; }
        public bool Active { get; set; } = false;

        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}

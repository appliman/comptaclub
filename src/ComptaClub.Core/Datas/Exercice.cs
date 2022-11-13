using Azure;

namespace ComptaClub.Datas
{
    public class Exercice : ITableEntity
    {
        public string? Label { get; set; } 
        public long InitialAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; } = DateTime.Now;
        public ETag ETag { get; set; }
    }
}

using Azure;

namespace ComptaClub.Datas;

public class Account : ITableEntity
{
    public AccountDirection Direction { get; set; }
    public string Label { get; set; } = null!;
    public int CreationDate { get; set; }
    public Guid? ParentAccountId { get; set; }

    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = null!;
    public DateTimeOffset? Timestamp { get ; set ; } 
    public ETag ETag { get ; set ; }
}

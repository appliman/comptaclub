using Azure;

namespace ComptaClub.Datas
{
    public class Entry : ITableEntity
    {
        public Guid Id { get; set; }
        public int CreationDate { get; set; }
        public long BalanceValue { get; set; }
        public long Amount { get; set; }
        public AccountDirection AccountDirection { get; set; }
        public Guid BankId { get; set; }
        public Guid AccountId { get; set; }
        public Guid ExerciceId { get; set; }
        public Guid? UserCreatorId { get; set; }
        public Guid? MemberId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string? ExtraInfos { get; set; }

        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}

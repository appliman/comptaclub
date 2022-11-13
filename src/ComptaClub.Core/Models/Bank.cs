namespace ComptaClub.Models
{
    public class Bank : IEntityKey, ILastUpdatable
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Label { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }
}

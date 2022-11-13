namespace ComptaClub.Models
{
    public class Bank
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Label { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    }
}

namespace ComptaClub.Models
{
    public class Bank : IEntityKey
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Label { get; set; }
        public int CreationDate { get; set; }
    }
}

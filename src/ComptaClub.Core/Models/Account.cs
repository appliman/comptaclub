using ComptaClub.Datas;

namespace ComptaClub.Models
{
    public class Account : IEntityKey
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Label { get; set; } = null!;
        public Guid? ParentAccountId { get; set; }
        public AccountDirection Direction { get; set; }
        public int CreationDate { get; set; }
        public List<Account> Children { get; set; } = new();
        public int Level { get; set; } = -1;
    }
}

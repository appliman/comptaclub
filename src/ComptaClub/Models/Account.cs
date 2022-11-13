using ComptaClub.Datas;

namespace ComptaClub.Models
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Label { get; set; } = null!;
        public Guid? LinkedAccount { get; set; }
        public DateTime CreationDate { get; set; }
        public AccountDirection Direction { get; set; }
    }
}

using ComptaClub.Datas;

namespace ComptaClub.Blazor.ViewModels
{
    public class Account : IMetaEntity
    {
        public int RowIndex { get; set; }
        public Guid Id { get; set; }
        public MetaEntity MetaEntity => MetaEntity.Account;
        public string Code { get; set; } = null!;
        public string Label { get; set; } = null!;
        public Guid? ParentAccountId { get; set; }
        public Datas.AccountDirection Direction { get; set; }
        public int CreationDate { get; set; }
        public List<Account> Children { get; set; } = new();
        public int Level { get; set; } = -1;
    }
}

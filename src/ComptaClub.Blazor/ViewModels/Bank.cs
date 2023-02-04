using ComptaClub.Datas;

namespace ComptaClub.Blazor.ViewModels
{
    public class Bank : IMetaEntity
    {
        public int RowIndex { get; set; }
        public Guid Id { get; set; }
        public MetaEntity MetaEntity => MetaEntity.Bank;
        public string Code { get; set; } = null!;
        public string? Label { get; set; }
        public int CreationDate { get; set; }
        public bool Active { get; set; }
    }
}
